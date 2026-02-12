using EasySave.Core.Contracts;
using EasySave.Core.Validation;
using EasySave.Core.Models;

namespace EasySave.Services;

/// <summary>
///     Executes backup jobs (complete or differential) and writes both log and state files.
/// </summary>
/// <remarks>
///     Version 1.0/1.1 requirement:
///     - Jobs can be executed individually or sequentially (no parallel/multi-thread execution).
///     - Every file transfer and directory creation must be logged in a daily JSON file.
///     - A "state" JSON file must be updated in real-time to show progress.
/// </remarks>
public sealed class BackupService : IBackupService
{
    private readonly BackupDirectoryPreparer _directoryPreparer;
    private readonly FileCopier _fileCopier;
    private readonly BackupFileSelector _fileSelector;
    private readonly ILogWriter<LogEntry> _logger;
    private readonly IPathService _paths;
    private readonly IProgressReporter _progress;
    private readonly IStateService _state;
    private readonly IJobValidator _validator;

    public BackupService(
        ILogWriter<LogEntry> logger,
        IStateService state,
        IPathService paths,
        BackupFileSelector fileSelector,
        BackupDirectoryPreparer directoryPreparer,
        FileCopier fileCopier,
        IJobValidator validator,
        IProgressReporter progress)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _fileSelector = fileSelector ?? throw new ArgumentNullException(nameof(fileSelector));
        _directoryPreparer = directoryPreparer ?? throw new ArgumentNullException(nameof(directoryPreparer));
        _fileCopier = fileCopier ?? throw new ArgumentNullException(nameof(fileCopier));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _progress = progress ?? throw new ArgumentNullException(nameof(progress));
    }

    /// <summary>
    ///     Runs multiple jobs sequentially, ordered by id.
    /// </summary>
    /// <param name="jobs">Jobs to run.</param>
    public void RunJobsSequential(IEnumerable<BackupJob> jobs)
    {
        foreach (var job in jobs.OrderBy(j => j.Id))
            RunJob(job);
    }

    /// <summary>
    ///     Runs a single job (complete or differential).
    /// </summary>
    /// <param name="job">Job to execute.</param>
    public void RunJob(BackupJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        var jobState = _state.GetOrCreate(job);
        InitializeJobState(jobState, job);

        var validation = _validator.Validate(job);
        if (!validation.IsValid)
        {
            HandleValidationFailure(job, jobState, validation);
            return;
        }

        var sourceDir = validation.SourceDirectory;
        var targetDir = validation.TargetDirectory;

        LogJobStart(job, sourceDir, targetDir);

        _directoryPreparer.EnsureTargetDirectories(job, sourceDir, targetDir);

        var filesToCopy = _fileSelector.GetFilesToCopy(job, sourceDir, targetDir);
        var totalSize = ComputeTotalSize(filesToCopy);

        InitializeActiveState(jobState, filesToCopy.Count, totalSize);

        var result = ExecuteTransfers(job, jobState, sourceDir, targetDir, filesToCopy, totalSize);

        FinalizeState(jobState, result.HadError);
        LogJobCompletion(job, sourceDir, targetDir, result.HadError);
    }

    private void InitializeJobState(BackupJobState state, BackupJob job)
    {
        UpdateState(state, s => { s.BackupName = job.Name; });
    }

    private void HandleValidationFailure(BackupJob job, BackupJobState state, JobValidationResult validation)
    {
        var action = validation.Error == JobValidationError.SourceMissing ? "source_missing" : "target_missing";
        UpdateState(state, s =>
        {
            s.State = JobRunState.Failed;
            s.CurrentAction = action;
            s.CurrentSourcePath = null;
            s.CurrentTargetPath = null;
            s.ProgressPercent = 0;
            s.RemainingFiles = 0;
            s.RemainingSizeBytes = 0;
        });

        var errorMessage = validation.Error == JobValidationError.SourceMissing
            ? "Source directory not found."
            : "Target directory not found.";

        WriteLog(job, validation.SourceDirectory, validation.TargetDirectory, 0, -1, errorMessage);
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
        });

        _progress.Report(0);
    }

    private TransferResult ExecuteTransfers(
        BackupJob job,
        BackupJobState state,
        string sourceDir,
        string targetDir,
        IReadOnlyList<string> filesToCopy,
        long totalSize)
    {
        var hadError = false;
        long transferredBytes = 0;

        for (var i = 0; i < filesToCopy.Count; i++)
        {
            var sourceFile = filesToCopy[i];
            var relative = _paths.GetRelativePath(sourceDir, sourceFile);
            var targetFile = Path.Combine(targetDir, relative);

            _directoryPreparer.EnsureTargetDirectoryForFile(job, sourceFile, targetFile);

            UpdateState(state, s =>
            {
                s.CurrentAction = "file_transfer";
                s.CurrentSourcePath = _paths.ToFullUncLikePath(sourceFile);
                s.CurrentTargetPath = _paths.ToFullUncLikePath(targetFile);
            });

            long fileSize = 0;
            long elapsedMs;
            string? errorMessage = null;

            try
            {
                var fi = new FileInfo(sourceFile);
                fileSize = fi.Length;
                elapsedMs = _fileCopier.Copy(sourceFile, targetFile);
            }
            catch (Exception ex)
            {
                hadError = true;
                elapsedMs = -1;
                errorMessage = $"{ex.GetType().Name}: {ex.Message}";
            }

            WriteLog(job, sourceFile, targetFile, fileSize, elapsedMs, errorMessage);

            if (elapsedMs >= 0)
                transferredBytes += fileSize;

            var remainingFiles = Math.Max(0, filesToCopy.Count - (i + 1));
            var remainingBytes = Math.Max(0, totalSize - transferredBytes);
            var percentage = totalSize <= 0 ? 100 : Math.Min(100, (double)transferredBytes / totalSize * 100d);

            UpdateState(state, s =>
            {
                s.RemainingFiles = remainingFiles;
                s.RemainingSizeBytes = remainingBytes;
                s.ProgressPercent = percentage;
            });

            _progress.Report(percentage);
        }

        return new TransferResult(hadError);
    }

    private void FinalizeState(BackupJobState state, bool hadError)
    {
        UpdateState(state, s =>
        {
            s.State = hadError ? JobRunState.Failed : JobRunState.Completed;
            s.CurrentAction = hadError ? "completed_with_errors" : "completed";
            s.CurrentSourcePath = null;
            s.CurrentTargetPath = null;
            s.ProgressPercent = 100;
            s.RemainingFiles = 0;
            s.RemainingSizeBytes = 0;
        });

        _progress.Report(100);
    }

    private void LogJobStart(BackupJob job, string sourceDir, string targetDir)
    {
        WriteLog(job, sourceDir, targetDir, 0, 0);
    }

    private void LogJobCompletion(BackupJob job, string sourceDir, string targetDir, bool hadError)
    {
        WriteLog(job, sourceDir, targetDir, 0, hadError ? -1 : 0);
    }

    private void WriteLog(BackupJob job, string sourcePath, string targetPath, long size, long elapsedMs,
        string? errorMessage = null)
    {
        _logger.Log(new LogEntry
        {
            Timestamp = DateTime.Now,
            BackupName = job.Name,
            SourcePath = _paths.ToFullUncLikePath(sourcePath),
            TargetPath = _paths.ToFullUncLikePath(targetPath),
            FileSizeBytes = size,
            TransferTimeMs = elapsedMs,
            ErrorMessage = errorMessage
        });
    }

    private void UpdateState(BackupJobState state, Action<BackupJobState> apply)
    {
        apply(state);
        state.LastActionTimestamp = DateTime.Now;
        _state.Update(state);
    }

    private static long ComputeTotalSize(IEnumerable<string> files)
    {
        long total = 0;
        foreach (var file in files)
        {
            try
            {
                total += new FileInfo(file).Length;
            }
            catch
            {
                // Ignore files we cannot read; they will fail during transfer and be logged.
            }
        }

        return total;
    }

    private readonly record struct TransferResult(bool HadError);
}
