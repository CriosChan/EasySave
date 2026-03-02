using EasySave.Core.Models;
using EasySave.Data.Configuration;
using EasySave.Models.Backup.Abstractions;
using EasySave.Models.Logger;
using EasySave.Models.State;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup.Execution;

/// <summary>
///     Executes the file-transfer loop for a single backup job.
///     Depends on <see cref="BackupJobIdentity" />, <see cref="IBackupProgressTracker" />,
///     and <see cref="IBackupJobController" /> for all state access.
/// </summary>
public sealed class BackupTransferOrchestrator
{
    private const int LargeFileWaitPollingIntervalMs = 100;

    private readonly BackupJobIdentity _identity;
    private readonly IBackupProgressTracker _progressTracker;
    private readonly IBackupJobController _controller;

    /// <summary> Raised once the transfer loop completes (with or without errors). </summary>
    public event EventHandler? EndEvent;

    /// <summary>
    ///     Gets or sets the monitor used to detect business-software activity.
    /// </summary>
    public IBusinessSoftwareMonitor BusinessSoftwareMonitor { get; set; } = new BusinessSoftwareMonitor();

    /// <summary>
    ///     Gets or sets the global coordinator for automatic pause/resume on business software detection.
    /// </summary>
    public IBusinessSoftwarePauseCoordinator BusinessSoftwarePauseCoordinator { get; set; } =
        GlobalBusinessSoftwarePauseCoordinator.Shared;

    /// <summary>
    ///     Gets or sets the global priority arbitrator shared across all parallel jobs.
    /// </summary>
    public IPriorityArbitrator? PriorityArbitrator { get; set; }

    /// <summary>
    ///     Gets or sets the global limiter that serializes transfers for files above the configured threshold.
    /// </summary>
    public ILargeFileTransferLimiter LargeFileTransferLimiter { get; set; } = GlobalLargeFileTransferLimiter.Shared;

    /// <summary>
    ///     Initializes a new instance of <see cref="BackupTransferOrchestrator" />.
    /// </summary>
    /// <param name="identity">Identity data of the job being executed.</param>
    /// <param name="progressTracker">Progress tracker to update during execution.</param>
    /// <param name="controller">Controller used to check pause/stop state and handle pause signals.</param>
    public BackupTransferOrchestrator(
        BackupJobIdentity identity,
        IBackupProgressTracker progressTracker,
        IBackupJobController controller)
    {
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        _progressTracker = progressTracker ?? throw new ArgumentNullException(nameof(progressTracker));
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <summary>
    ///     Synchronous entry point — wraps <see cref="ExecuteAsync" /> for callers that cannot await.
    /// </summary>
    public void Execute() => ExecuteAsync().GetAwaiter().GetResult();

    /// <summary>
    ///     Executes the full backup transfer asynchronously: directory check, file selection,
    ///     partitioning, and parallel transfer dispatch.
    /// </summary>
    private async Task ExecuteAsync()
    {
        _controller.WasStoppedByBusinessSoftware = false;
        _controller.NotifyBusinessSoftwarePause(false);
        _controller.ResetStopped();
        _progressTracker.SetCurrentProgress(0);

        // Save crypto key to file if missing
        CryptoSoftConfiguration.Load().Save();
        StateFileSingleton.Instance.Initialize(ApplicationConfiguration.Load().LogPath);
        var state = StateFileSingleton.Instance.GetOrCreate(_identity.Id, _identity.Name);
        using var businessSoftwareRegistration =
            BusinessSoftwarePauseCoordinator.RegisterJob(_identity.Id, _identity.Name, BusinessSoftwareMonitor);

        // Verify source and target directories
        if (!Check(out var errorMessage))
        {
            Console.WriteLine($"[ERROR] Backup job '{_identity.Name}' (ID: {_identity.Id}) failed: {errorMessage}");
            StateLogger.SetStateFailed(state);
            _controller.Stop();
            return;
        }

        // Mirror directory structure and retrieve file list
        new BackupFolder(_identity.SourceDirectory, _identity.TargetDirectory, _identity.Name).MirrorFolder();
        var selector = TypeSelectorHelper.GetSelector(
            _identity.Type,
            _identity.SourceDirectory,
            _identity.TargetDirectory,
            _identity.Name);
        _identity.Files = selector.GetFilesToBackup();
        var hadError = false;
        var hadErrorLock = new object();
        _progressTracker.TotalSize = ComputeTotalSizeSafely(_identity.Files, ref hadError);
        _progressTracker.TransferredSize = 0;
        _progressTracker.SetFilesCount(_identity.Files.Count);

        // Partition files into priority and standard queues
        var config = ApplicationConfiguration.Load();
        var (priorityQueue, standardQueue) = FilePartitioner.Partition(_identity.Files, config.PriorityExtensions);

        // Initialize the global priority arbitrator if available
        PriorityArbitrator?.Initialize(new Dictionary<int, int> { { _identity.Id, priorityQueue.Count } });

        StateLogger.SetStateActive(state, _progressTracker.FilesCount, _progressTracker.TotalSize);
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.PriorityFilesRemaining = priorityQueue.Count;
            s.StandardFilesRemaining = standardQueue.Count;
        });

        // processedCount is only incremented on the dispatch thread (sequential); no Interlocked needed.
        var processedCount = 0;
        // transferredBytes is updated concurrently by file tasks; use Interlocked for thread-safety.
        long transferredBytes = 0;

        var fileTasks = new List<Task>();

        foreach (var (queue, transferType) in new[]
        {
            (priorityQueue, FileTransferPriority.High),
            (standardQueue, FileTransferPriority.Low)
        })
        {
            if (_controller.WasStopped) break;

            while (queue.Count > 0)
            {
                if (!WaitForRuntimeAvailability(state, queue.Peek()))
                    break;

                // Enforce priority arbitration before dispatching standard files
                if (transferType == FileTransferPriority.Low)
                {
                    while (!CanProcessStandardFile(PriorityArbitrator, _identity.Id))
                    {
                        StateLogger.SetStatePausedPriority(state);
                        if (!WaitForRuntimeAvailability(state, queue.Peek(), 100))
                            goto done;
                    }

                    StateLogger.SetStateActive(state, _progressTracker.FilesCount, _progressTracker.TotalSize);
                }

                var file = queue.Dequeue();
                if (!TryGetFileSize(file, out var fileSizeBytes, out var fileSizeError))
                {
                    lock (hadErrorLock) { hadError = true; }
                    LogFilePreparationError(file, fileSizeError);
                    continue;
                }

                // Update priority counter immediately after dequeue (dispatch order)
                if (transferType == FileTransferPriority.High)
                    PriorityArbitrator?.UpdateGlobalPriorityCount(_identity.Id, queue.Count);

                StateFileSingleton.Instance.UpdateState(state, s =>
                {
                    s.PriorityFilesRemaining = priorityQueue.Count;
                    s.StandardFilesRemaining = standardQueue.Count;
                });

                // Acquire large-file slot on the dispatch thread before launching the task
                var hasExclusiveLargeFileSlot = false;
                if (RequiresExclusiveLargeFileSlot(LargeFileTransferLimiter, fileSizeBytes))
                {
                    var waitStartedAtUtc = DateTime.UtcNow;
                    var waitedForSlot = false;

                    while (!LargeFileTransferLimiter.TryAcquireExclusiveSlot(
                               TimeSpan.FromMilliseconds(LargeFileWaitPollingIntervalMs)))
                    {
                        waitedForSlot = true;
                        StateLogger.SetStateWaitingLargeFile(state, file);
                        if (!WaitForRuntimeAvailability(state, file, LargeFileWaitPollingIntervalMs))
                            goto done;
                    }

                    hasExclusiveLargeFileSlot = true;

                    if (waitedForSlot)
                    {
                        var waitedMs = Math.Max(0, (long)(DateTime.UtcNow - waitStartedAtUtc).TotalMilliseconds);
                        LogLargeFileWait(file, fileSizeBytes, waitedMs);
                    }
                }

                StateLogger.SetStateStartTransfer(
                    state,
                    file,
                    transferType == FileTransferPriority.High ? "Priority file transfer" : "Standard File Transfer");

                // Capture loop variables for the closure
                var capturedFile = file;
                var capturedSize = fileSizeBytes;
                var capturedSlot = hasExclusiveLargeFileSlot;
                var capturedIndex = processedCount++;

                _progressTracker.SetCurrentFileIndex(capturedIndex);

                // Dispatch the actual I/O to a thread-pool task
                var fileTask = Task.Run(async () =>
                {
                    try
                    {
                        if (!_controller.WasStopped)
                            await capturedFile.CopyAsync();
                    }
                    catch (Exception)
                    {
                        lock (hadErrorLock) { hadError = true; }
                    }
                    finally
                    {
                        if (capturedSlot)
                            LargeFileTransferLimiter.ReleaseExclusiveSlot();
                    }

                    // Update progress counters thread-safely
                    var newTransferred = Interlocked.Add(ref transferredBytes, capturedSize);
                    _progressTracker.TransferredSize = newTransferred;

                    _progressTracker.SetCurrentProgress(
                        MathUtil.Percentage(_progressTracker.TransferredSize, _progressTracker.TotalSize));

                    StateLogger.SetStateEndTransfer(
                        state,
                        _progressTracker.FilesCount,
                        capturedIndex,
                        _progressTracker.TotalSize,
                        _progressTracker.TransferredSize,
                        _progressTracker.CurrentProgress);
                });

                fileTasks.Add(fileTask);
            }
        }

        done:
        // Wait for all in-flight file transfers to complete
        await Task.WhenAll(fileTasks);

        PriorityArbitrator?.OnJobCompleted(_identity.Id);

        StateLogger.SetStateEnd(state, hadError, _controller.WasStopped);

        if (!_controller.WasStopped)
            _progressTracker.SetCurrentProgress(100);

        EndEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Applies runtime gates in order: automatic business-software pause, then manual pause/stop.
    /// </summary>
    /// <param name="state">Current job runtime state.</param>
    /// <param name="blockedFile">Current blocked file when available.</param>
    /// <param name="pauseWaitTimeoutMs">
    ///     Optional manual-pause wait timeout. When null, waits indefinitely like standard file loop behavior.
    /// </param>
    /// <returns>True when execution may continue; false when job is stopped.</returns>
    private bool WaitForRuntimeAvailability(BackupJobState state, IFile? blockedFile, int? pauseWaitTimeoutMs = null)
    {
        BusinessSoftwarePauseCoordinator.WaitWhileBusinessSoftwareRuns(
            _identity.Id,
            state,
            blockedFile,
            () => _controller.WasStopped,
            _controller.NotifyBusinessSoftwarePause);

        if (_controller.WasStopped)
            return false;

        if (_controller.IsPaused())
            StateLogger.SetStatePaused(state);

        if (pauseWaitTimeoutMs.HasValue)
            _controller.WaitIfPaused(pauseWaitTimeoutMs.Value);
        else
            _controller.WaitIfPaused();

        return !_controller.WasStopped;
    }

    /// <summary>
    ///     Checks that both source and target directories are accessible.
    /// </summary>
    /// <param name="errorMessage">Populated with a description when a directory is not accessible.</param>
    /// <returns>True if both directories exist and are accessible; otherwise, false.</returns>
    private bool Check(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!PathService.IsDirectoryAccessible(_identity.SourceDirectory, out var sourceError))
        {
            errorMessage = $"Source directory error: {sourceError}";
            Console.WriteLine($"[ERROR] {errorMessage}");
            return false;
        }

        if (!PathService.IsDirectoryAccessible(_identity.TargetDirectory, out var targetError))
        {
            errorMessage = $"Target directory error: {targetError}";
            Console.WriteLine($"[ERROR] {errorMessage}");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Computes the total size of files while tolerating transient/unreadable files.
    /// </summary>
    private long ComputeTotalSizeSafely(IEnumerable<IFile> files, ref bool hadError)
    {
        long totalSize = 0;
        foreach (var file in files)
        {
            if (TryGetFileSize(file, out var fileSizeBytes, out var fileSizeError))
            {
                totalSize += fileSizeBytes;
                continue;
            }

            hadError = true;
            LogFilePreparationError(file, fileSizeError);
        }

        return totalSize;
    }

    /// <summary>
    ///     Tries to read file size without letting a single inaccessible file stop the whole job.
    /// </summary>
    private static bool TryGetFileSize(IFile file, out long fileSizeBytes, out string errorMessage)
    {
        try
        {
            fileSizeBytes = file.GetSize();
            errorMessage = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            fileSizeBytes = 0;
            errorMessage = ex.Message;
            return false;
        }
    }

    /// <summary>
    ///     Logs a pre-transfer file preparation error (typically metadata/permission issues).
    /// </summary>
    private void LogFilePreparationError(IFile file, string errorMessage)
    {
        var logger = new ConfigurableLogWriter<LogEntry>();
        logger.Log(new LogEntry
        {
            BackupName = _identity.Name,
            SourcePath = PathService.ToFullUncLikePath(file.SourceFile),
            TargetPath = PathService.ToFullUncLikePath(file.TargetFile),
            FileSizeBytes = 0,
            TransferTimeMs = 0,
            ErrorMessage = $"Unable to prepare file transfer: {errorMessage}"
        });
    }

    /// <summary>
    ///     Determines whether a standard file can be processed by the given job.
    /// </summary>
    private static bool CanProcessStandardFile(IPriorityArbitrator? arbitrator, int jobId)
    {
        return arbitrator == null || arbitrator.CanProcessStandardFile(jobId);
    }

    /// <summary>
    ///     Determines whether this file requires the global exclusive large-file transfer slot.
    /// </summary>
    private static bool RequiresExclusiveLargeFileSlot(ILargeFileTransferLimiter? limiter, long fileSizeBytes)
    {
        return limiter != null && limiter.RequiresExclusiveSlot(fileSizeBytes);
    }

    /// <summary>
    ///     Logs time spent waiting for the global large-file transfer slot.
    /// </summary>
    private void LogLargeFileWait(IFile file, long fileSizeBytes, long waitedMs)
    {
        var logger = new ConfigurableLogWriter<LogEntry>();
        logger.Log(new LogEntry
        {
            BackupName = _identity.Name,
            SourcePath = PathService.ToFullUncLikePath(file.SourceFile),
            TargetPath = PathService.ToFullUncLikePath(file.TargetFile),
            FileSizeBytes = fileSizeBytes,
            TransferTimeMs = 0,
            ErrorMessage = $"Waited {waitedMs} ms for large-file transfer slot."
        });
    }
}
