using System.Collections.Concurrent;
using EasySave.Application.Abstractions;
using EasySave.Application.Models;
using EasySave.Domain.Models;

namespace EasySave.Application.Services;

/// <summary>
///     Parallel backup orchestrator with runtime controls (play/pause/stop).
/// </summary>
public sealed class BackupService : IBackupService, IBackupRuntimeController, IDisposable
{
    private readonly IBusinessSoftwareDetector _businessDetector;
    private readonly BackupDirectoryPreparer _directoryPreparer;
    private readonly IFileEncryptionService _encryption;
    private readonly FileCopier _fileCopier;
    private readonly BackupFileSelector _fileSelector;
    private readonly object _globalStopSync = new();
    private readonly SemaphoreSlim _largeFileGate = new(1, 1);
    private readonly ILogWriter<LogEntry> _logger;
    private readonly object _monitorSync = new();
    private readonly IPathService _paths;
    private readonly IProgressReporter _progress;
    private readonly IGeneralSettingsStore _settings;
    private readonly IStateService _state;
    private readonly IJobValidator _validator;
    private int _activeWorkers;
    private volatile bool _businessPauseRequested;
    private readonly ConcurrentDictionary<int, JobControl> _controls = new();
    private CancellationTokenSource _globalStop = new();
    private Task? _monitorTask;
    private CancellationTokenSource? _monitorTokenSource;
    private int _pendingPriorityFiles;

    public BackupService(
        ILogWriter<LogEntry> logger,
        IStateService state,
        IPathService paths,
        BackupFileSelector fileSelector,
        BackupDirectoryPreparer directoryPreparer,
        FileCopier fileCopier,
        IJobValidator validator,
        IProgressReporter progress,
        IGeneralSettingsStore settings,
        IBusinessSoftwareDetector businessDetector,
        IFileEncryptionService encryption)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _fileSelector = fileSelector ?? throw new ArgumentNullException(nameof(fileSelector));
        _directoryPreparer = directoryPreparer ?? throw new ArgumentNullException(nameof(directoryPreparer));
        _fileCopier = fileCopier ?? throw new ArgumentNullException(nameof(fileCopier));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _businessDetector = businessDetector ?? throw new ArgumentNullException(nameof(businessDetector));
        _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));
    }

    public event Action<BackupJobState>? JobStateChanged;

    public void RunJobsSequential(IEnumerable<BackupJob> jobs)
    {
        RunJobsParallelAsync(jobs).GetAwaiter().GetResult();
    }

    public void RunJob(BackupJob job)
    {
        RunJobAsync(job).GetAwaiter().GetResult();
    }

    public async Task RunJobsParallelAsync(IEnumerable<BackupJob> jobs, CancellationToken cancellationToken = default)
    {
        if (jobs == null)
            throw new ArgumentNullException(nameof(jobs));

        var ordered = jobs
            .Where(j => j != null)
            .OrderBy(j => j.Id)
            .ToList();

        if (ordered.Count == 0)
            return;

        var tasks = ordered.Select(job => RunJobAsync(job, cancellationToken)).ToList();
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task RunJobAsync(BackupJob job, CancellationToken cancellationToken = default)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        EnterWorkerScope();
        var control = _controls.GetOrAdd(job.Id, _ => new JobControl(job.Id));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            GetGlobalStopToken(),
            control.StopSource.Token);

        var state = _state.GetOrCreate(job);
        UpdateState(state, s =>
        {
            s.BackupName = job.Name;
            s.State = JobRunState.Inactive;
            s.CurrentAction = "queued";
            s.LastError = null;
            s.ProgressPercent = 0;
        });

        var localRemainingPriority = 0;
        try
        {
            var validation = _validator.Validate(job);
            if (!validation.IsValid)
            {
                HandleValidationFailure(job, state, validation);
                return;
            }

            var sourceDir = validation.SourceDirectory;
            var targetDir = validation.TargetDirectory;
            LogJobStart(job, sourceDir, targetDir);

            _directoryPreparer.EnsureTargetDirectories(job, sourceDir, targetDir);

            var settings = _settings.Current;
            var files = _fileSelector.GetFilesToCopy(job, sourceDir, targetDir);
            var prioritySet = new HashSet<string>(settings.PriorityExtensions, StringComparer.OrdinalIgnoreCase);
            var enriched = files
                .Select(path => CreateTransferFile(path, sourceDir, prioritySet))
                .OrderByDescending(x => x.IsPriority)
                .ThenBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToList();

            localRemainingPriority = enriched.Count(x => x.IsPriority);
            if (localRemainingPriority > 0)
                Interlocked.Add(ref _pendingPriorityFiles, localRemainingPriority);

            var totalSize = enriched.Sum(x => x.SizeBytes);
            InitializeActiveState(state, enriched.Count, totalSize);

            var hadError = false;
            long transferredBytes = 0;
            var thresholdBytes = settings.LargeFileThresholdKb * 1024;

            for (var i = 0; i < enriched.Count; i++)
            {
                linked.Token.ThrowIfCancellationRequested();
                await WaitForExecutionWindowAsync(control, state, linked.Token, enforceManualPause: true)
                    .ConfigureAwait(false);

                var item = enriched[i];
                if (!item.IsPriority)
                    await WaitForPriorityWindowAsync(control, state, linked.Token).ConfigureAwait(false);

                var targetFile = Path.Combine(targetDir, item.RelativePath);
                _directoryPreparer.EnsureTargetDirectoryForFile(job, item.SourcePath, targetFile);

                UpdateState(state, s =>
                {
                    s.State = JobRunState.Active;
                    s.CurrentAction = "file_transfer";
                    s.CurrentSourcePath = _paths.ToFullUncLikePath(item.SourcePath);
                    s.CurrentTargetPath = _paths.ToFullUncLikePath(targetFile);
                });

                long elapsedMs = -1;
                long encryptionMs = 0;
                string? errorMessage = null;
                var requiresLargeFileGate = thresholdBytes > 0 && item.SizeBytes > thresholdBytes;

                if (requiresLargeFileGate)
                    await _largeFileGate.WaitAsync(linked.Token).ConfigureAwait(false);

                try
                {
                    elapsedMs = await _fileCopier.CopyAsync(
                            item.SourcePath,
                            targetFile,
                            linked.Token,
                            token => WaitForExecutionWindowAsync(control, state, token, enforceManualPause: false))
                        .ConfigureAwait(false);

                    encryptionMs = await _encryption.EncryptAsync(targetFile, linked.Token).ConfigureAwait(false);
                    if (encryptionMs < 0)
                    {
                        hadError = true;
                        errorMessage = $"CryptoSoftError({encryptionMs})";
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    hadError = true;
                    errorMessage = $"{ex.GetType().Name}: {ex.Message}";
                }
                finally
                {
                    if (requiresLargeFileGate)
                        _largeFileGate.Release();
                }

                WriteLog(job, item.SourcePath, targetFile, item.SizeBytes, elapsedMs, encryptionMs, errorMessage);

                if (elapsedMs >= 0)
                    transferredBytes += item.SizeBytes;

                if (item.IsPriority)
                {
                    localRemainingPriority--;
                    Interlocked.Decrement(ref _pendingPriorityFiles);
                }

                var remainingFiles = Math.Max(0, enriched.Count - (i + 1));
                var remainingBytes = Math.Max(0, totalSize - transferredBytes);
                var percentage = totalSize <= 0 ? 100 : Math.Min(100, (double)transferredBytes / totalSize * 100d);

                UpdateState(state, s =>
                {
                    s.RemainingFiles = remainingFiles;
                    s.RemainingSizeBytes = remainingBytes;
                    s.ProgressPercent = percentage;
                    s.LastError = errorMessage;
                });
                _progress.Report(percentage);
            }

            var completionAction = hadError ? "completed_with_errors" : "completed";
            FinalizeState(state, hadError, completionAction);
            LogJobCompletion(job, sourceDir, targetDir, hadError, completionAction);
        }
        catch (OperationCanceledException)
        {
            FinalizeState(state, hadError: true, action: "stopped");
            WriteLog(job, state.CurrentSourcePath ?? job.SourceDirectory, state.CurrentTargetPath ?? job.TargetDirectory, 0, -1, 0,
                "Stopped");
        }
        finally
        {
            if (localRemainingPriority > 0)
                Interlocked.Add(ref _pendingPriorityFiles, -localRemainingPriority);

            control.ManualPauseRequested = false;
            if (_controls.TryRemove(job.Id, out var removed))
                removed.StopSource.Dispose();
            ExitWorkerScope();
        }
    }

    public void PauseJob(int jobId)
    {
        if (_controls.TryGetValue(jobId, out var control))
            control.ManualPauseRequested = true;
    }

    public void ResumeJob(int jobId)
    {
        if (_controls.TryGetValue(jobId, out var control))
            control.ManualPauseRequested = false;
    }

    public void StopJob(int jobId)
    {
        if (_controls.TryGetValue(jobId, out var control))
            control.StopSource.Cancel();
    }

    public void PauseAll()
    {
        foreach (var control in _controls.Values)
            control.ManualPauseRequested = true;
    }

    public void ResumeAll()
    {
        foreach (var control in _controls.Values)
            control.ManualPauseRequested = false;
    }

    public void StopAll()
    {
        lock (_globalStopSync)
        {
            _globalStop.Cancel();
        }

        foreach (var control in _controls.Values)
            control.StopSource.Cancel();
    }

    public void Dispose()
    {
        lock (_globalStopSync)
        {
            _globalStop.Cancel();
            _globalStop.Dispose();
        }

        lock (_monitorSync)
        {
            _monitorTokenSource?.Cancel();
            _monitorTokenSource?.Dispose();
        }
    }

    private async Task WaitForPriorityWindowAsync(
        JobControl control,
        BackupJobState state,
        CancellationToken cancellationToken)
    {
        while (Volatile.Read(ref _pendingPriorityFiles) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WaitForExecutionWindowAsync(control, state, cancellationToken, enforceManualPause: true)
                .ConfigureAwait(false);

            if (Volatile.Read(ref _pendingPriorityFiles) <= 0)
                break;

            if (state.CurrentAction != "waiting_priority_files")
                UpdateState(state, s => { s.CurrentAction = "waiting_priority_files"; });

            await Task.Delay(120, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task WaitForExecutionWindowAsync(
        JobControl control,
        BackupJobState state,
        CancellationToken cancellationToken,
        bool enforceManualPause)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var shouldPauseForBusiness = _businessPauseRequested;
            var shouldPauseForManual = enforceManualPause && control.ManualPauseRequested;
            if (!shouldPauseForBusiness && !shouldPauseForManual)
            {
                if (state.State == JobRunState.Paused)
                    UpdateState(state, s =>
                    {
                        s.State = JobRunState.Active;
                        s.CurrentAction = "resumed";
                    });
                return;
            }

            var reason = shouldPauseForBusiness ? "paused_business_software" : "paused_by_user";
            if (state.State != JobRunState.Paused || state.CurrentAction != reason)
                UpdateState(state, s =>
                {
                    s.State = JobRunState.Paused;
                    s.CurrentAction = reason;
                });

            await Task.Delay(120, cancellationToken).ConfigureAwait(false);
        }
    }

    private void EnterWorkerScope()
    {
        if (Interlocked.Increment(ref _activeWorkers) == 1)
            StartBusinessMonitor();
    }

    private void ExitWorkerScope()
    {
        if (Interlocked.Decrement(ref _activeWorkers) == 0)
            StopBusinessMonitor();
    }

    private void StartBusinessMonitor()
    {
        lock (_monitorSync)
        {
            if (_monitorTask != null)
                return;

            _monitorTokenSource = new CancellationTokenSource();
            _monitorTask = Task.Run(() => MonitorBusinessProcessAsync(_monitorTokenSource.Token));
        }
    }

    private void StopBusinessMonitor()
    {
        Task? monitorToWait;
        lock (_monitorSync)
        {
            monitorToWait = _monitorTask;
            if (monitorToWait == null)
                return;

            _monitorTokenSource?.Cancel();
            _monitorTask = null;
        }

        try
        {
            monitorToWait.GetAwaiter().GetResult();
        }
        catch
        {
            // Ignore monitor cancellation errors.
        }

        lock (_monitorSync)
        {
            _monitorTokenSource?.Dispose();
            _monitorTokenSource = null;
            _businessPauseRequested = false;
        }
    }

    private async Task MonitorBusinessProcessAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var cfg = _settings.Current;
                _businessPauseRequested = cfg.EnableBusinessProcessMonitor &&
                                          _businessDetector.IsRunning(cfg.BusinessProcessName);
            }
            catch
            {
                _businessPauseRequested = false;
            }

            var delay = Math.Max(100, _settings.Current.BusinessProcessCheckIntervalMs);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    private CancellationToken GetGlobalStopToken()
    {
        lock (_globalStopSync)
        {
            if (_globalStop.IsCancellationRequested)
            {
                _globalStop.Dispose();
                _globalStop = new CancellationTokenSource();
            }

            return _globalStop.Token;
        }
    }

    private static TransferFile CreateTransferFile(string sourcePath, string sourceRoot, HashSet<string> priorityExtensions)
    {
        long size = 0;
        try
        {
            size = new FileInfo(sourcePath).Length;
        }
        catch
        {
            // Keep size at 0 when metadata cannot be read.
        }

        var extension = Path.GetExtension(sourcePath) ?? string.Empty;
        var relative = Path.GetRelativePath(sourceRoot, sourcePath);
        return new TransferFile(sourcePath, relative, size, priorityExtensions.Contains(extension));
    }

    private void InitializeActiveState(BackupJobState state, int totalFiles, long totalSize)
    {
        UpdateState(state, s =>
        {
            s.State = JobRunState.Active;
            s.TotalFiles = totalFiles;
            s.TotalSizeBytes = totalSize;
            s.ProgressPercent = 0;
            s.RemainingFiles = totalFiles;
            s.RemainingSizeBytes = totalSize;
            s.CurrentAction = "start";
            s.CurrentSourcePath = null;
            s.CurrentTargetPath = null;
            s.LastError = null;
        });
    }

    private void HandleValidationFailure(BackupJob job, BackupJobState state, JobValidationResult validation)
    {
        var action = validation.Error == JobValidationError.SourceMissing ? "source_missing" : "target_missing";
        var message = validation.Error == JobValidationError.SourceMissing
            ? "Source directory not found."
            : "Target directory not found.";

        UpdateState(state, s =>
        {
            s.State = JobRunState.Failed;
            s.CurrentAction = action;
            s.CurrentSourcePath = null;
            s.CurrentTargetPath = null;
            s.ProgressPercent = 0;
            s.RemainingFiles = 0;
            s.RemainingSizeBytes = 0;
            s.LastError = message;
        });

        WriteLog(job, validation.SourceDirectory, validation.TargetDirectory, 0, -1, 0, message);
    }

    private void FinalizeState(BackupJobState state, bool hadError, string action)
    {
        UpdateState(state, s =>
        {
            s.State = action == "stopped"
                ? JobRunState.Stopped
                : hadError
                    ? JobRunState.Failed
                    : JobRunState.Completed;
            s.CurrentAction = action;
            s.CurrentSourcePath = null;
            s.CurrentTargetPath = null;
            s.ProgressPercent = action == "stopped" ? s.ProgressPercent : 100;
            s.RemainingFiles = action == "stopped" ? s.RemainingFiles : 0;
            s.RemainingSizeBytes = action == "stopped" ? s.RemainingSizeBytes : 0;
        });
    }

    private void LogJobStart(BackupJob job, string sourceDir, string targetDir)
    {
        WriteLog(job, sourceDir, targetDir, 0, 0, 0);
    }

    private void LogJobCompletion(BackupJob job, string sourceDir, string targetDir, bool hadError, string action)
    {
        var completionMessage = action == "stopped" ? "Stopped" : null;
        WriteLog(job, sourceDir, targetDir, 0, hadError ? -1 : 0, 0, completionMessage);
    }

    private void WriteLog(
        BackupJob job,
        string sourcePath,
        string targetPath,
        long fileSizeBytes,
        long transferTimeMs,
        long encryptionTimeMs,
        string? errorMessage = null)
    {
        _logger.Log(new LogEntry
        {
            Timestamp = DateTime.Now,
            BackupName = job.Name,
            HostName = Environment.MachineName,
            UserName = Environment.UserName,
            SourcePath = _paths.ToFullUncLikePath(sourcePath),
            TargetPath = _paths.ToFullUncLikePath(targetPath),
            FileSizeBytes = fileSizeBytes,
            TransferTimeMs = transferTimeMs,
            EncryptionTimeMs = encryptionTimeMs,
            ErrorMessage = errorMessage
        });
    }

    private void UpdateState(BackupJobState state, Action<BackupJobState> apply)
    {
        apply(state);
        state.LastActionTimestamp = DateTime.Now;
        _state.Update(state);
        JobStateChanged?.Invoke(CloneState(state));
    }

    private static BackupJobState CloneState(BackupJobState source)
    {
        return new BackupJobState
        {
            JobId = source.JobId,
            BackupName = source.BackupName,
            LastActionTimestamp = source.LastActionTimestamp,
            State = source.State,
            TotalFiles = source.TotalFiles,
            TotalSizeBytes = source.TotalSizeBytes,
            ProgressPercent = source.ProgressPercent,
            RemainingFiles = source.RemainingFiles,
            RemainingSizeBytes = source.RemainingSizeBytes,
            CurrentSourcePath = source.CurrentSourcePath,
            CurrentTargetPath = source.CurrentTargetPath,
            CurrentAction = source.CurrentAction,
            LastError = source.LastError
        };
    }

    private sealed class JobControl
    {
        public JobControl(int jobId)
        {
            JobId = jobId;
        }

        public int JobId { get; }
        public CancellationTokenSource StopSource { get; } = new();
        public bool ManualPauseRequested { get; set; }
    }

    private readonly record struct TransferFile(string SourcePath, string RelativePath, long SizeBytes, bool IsPriority);
}
