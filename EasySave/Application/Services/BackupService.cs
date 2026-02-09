using EasyLog;
using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Presentation.Ui;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Application.Services;

/// <summary>
/// Executes backup jobs (complete or differential) and writes both log and state files.
/// </summary>
/// <remarks>
/// Version 1.0/1.1 requirement:
/// - Jobs can be executed individually or sequentially (no parallel/multi-thread execution).
/// - Every file transfer and directory creation must be logged in a daily JSON file.
/// - A "state" JSON file must be updated in real-time to show progress.
/// </remarks>
public sealed class BackupService : IBackupService
{
    private readonly AbstractLogger<LogEntry> _logger;
    private readonly IStateService _state;
    private readonly IPathService _paths;
    private readonly IBackupFileSelector _fileSelector;
    private readonly IBackupDirectoryPreparer _directoryPreparer;
    private readonly IFileCopier _fileCopier;

    /// <summary>
    /// Builds the backup orchestrator.
    /// </summary>
    /// <param name="logger">Log writer.</param>
    /// <param name="state">State management service.</param>
    /// <param name="paths">Path service.</param>
    /// <param name="fileSelector">File selector.</param>
    /// <param name="directoryPreparer">Target directory preparer.</param>
    /// <param name="fileCopier">File copier.</param>
    public BackupService(
        AbstractLogger<LogEntry> logger,
        IStateService state,
        IPathService paths,
        IBackupFileSelector fileSelector,
        IBackupDirectoryPreparer directoryPreparer,
        IFileCopier fileCopier)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _fileSelector = fileSelector ?? throw new ArgumentNullException(nameof(fileSelector));
        _directoryPreparer = directoryPreparer ?? throw new ArgumentNullException(nameof(directoryPreparer));
        _fileCopier = fileCopier ?? throw new ArgumentNullException(nameof(fileCopier));
    }

    /// <summary>
    /// Runs multiple jobs sequentially, ordered by id.
    /// </summary>
    /// <param name="jobs">Jobs to run.</param>
    public void RunJobsSequential(IEnumerable<BackupJob> jobs)
    {
        foreach (BackupJob job in jobs.OrderBy(j => j.Id))
            RunJob(job);
    }

    /// <summary>
    /// Runs a single job (complete or differential).
    /// </summary>
    /// <param name="job">Job to execute.</param>
    public void RunJob(BackupJob job)
    {
        BackupJobState jobState = _state.GetOrCreate(job);
        jobState.BackupName = job.Name;
        jobState.LastActionTimestamp = DateTime.Now;

        // Normalize user-provided paths (trim, strip quotes, expand env vars) and validate existence.
        bool sourceOk = _paths.TryNormalizeExistingDirectory(job.SourceDirectory, out string sourceDir);
        bool targetOk = _paths.TryNormalizeExistingDirectory(job.TargetDirectory, out string targetDir);

        // Validate source directory.
        if (!sourceOk)
        {
            jobState.State = JobRunState.Failed;
            jobState.CurrentAction = "source_missing";
            _state.Update(jobState);

            _logger.Log(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = job.Name,
                SourcePath = _paths.ToFullUncLikePath(sourceDir),
                TargetPath = _paths.ToFullUncLikePath(targetDir),
                FileSizeBytes = 0,
                TransferTimeMs = -1,
            });

            return;
        }

        // Validate target directory.
        if (!targetOk)
        {
            jobState.State = JobRunState.Failed;
            jobState.CurrentAction = "target_missing";
            jobState.LastActionTimestamp = DateTime.Now;
            _state.Update(jobState);

            _logger.Log(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = job.Name,
                SourcePath = _paths.ToFullUncLikePath(sourceDir),
                TargetPath = _paths.ToFullUncLikePath(targetDir),
                FileSizeBytes = 0,
                TransferTimeMs = -1,
            });

            return;
        }

        // Always write at least one log entry when a job starts.
        // This ensures the daily log file exists even if there are no eligible files to transfer.
        _logger.Log(new LogEntry
        {
            Timestamp = DateTime.Now,
            BackupName = job.Name,
            SourcePath = _paths.ToFullUncLikePath(sourceDir),
            TargetPath = _paths.ToFullUncLikePath(targetDir),
            FileSizeBytes = 0,
            TransferTimeMs = 0,
        });

        // Mirror the directory structure (including empty directories).
        // This matches the requirement: all subdirectories must be preserved.
        _directoryPreparer.EnsureTargetDirectories(job, sourceDir, targetDir);

        List<string> filesToCopy = _fileSelector.GetFilesToCopy(job, sourceDir, targetDir);
        long totalSize = filesToCopy.Sum(f => new FileInfo(f).Length);

        jobState.State = JobRunState.Active;
        jobState.TotalFiles = filesToCopy.Count;
        jobState.TotalSizeBytes = totalSize;
        jobState.ProgressPercent = 0;
        jobState.RemainingFiles = filesToCopy.Count;
        jobState.RemainingSizeBytes = totalSize;
        jobState.CurrentAction = "start";
        jobState.CurrentSourcePath = null;
        jobState.CurrentTargetPath = null;
        jobState.LastActionTimestamp = DateTime.Now;
        _state.Update(jobState);

        bool hadError = false;
        long transferredBytes = 0;

        ProgressWidget progressWidget = new ProgressWidget(new SystemConsole());
        foreach (string sourceFile in filesToCopy)
        {
            string relative = _paths.GetRelativePath(sourceDir, sourceFile);
            string targetFile = Path.Combine(targetDir, relative);

            _directoryPreparer.EnsureTargetDirectoryForFile(job, sourceFile, targetFile);

            jobState.CurrentAction = "file_transfer";
            jobState.CurrentSourcePath = _paths.ToFullUncLikePath(sourceFile);
            jobState.CurrentTargetPath = _paths.ToFullUncLikePath(targetFile);
            jobState.LastActionTimestamp = DateTime.Now;
            _state.Update(jobState);

            long fileSize = 0;
            long elapsedMs;
            try
            {
                FileInfo fi = new FileInfo(sourceFile);
                fileSize = fi.Length;
                elapsedMs = _fileCopier.Copy(sourceFile, targetFile);
            }
            catch (Exception)
            {
                hadError = true;
                elapsedMs = -1;
            }

            _logger.Log(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = job.Name,
                SourcePath = _paths.ToFullUncLikePath(sourceFile),
                TargetPath = _paths.ToFullUncLikePath(targetFile),
                FileSizeBytes = fileSize,
                TransferTimeMs = elapsedMs,
            });

            if (elapsedMs >= 0)
                transferredBytes += fileSize;

            jobState.RemainingFiles = Math.Max(0, jobState.RemainingFiles - 1);
            jobState.RemainingSizeBytes = Math.Max(0, totalSize - transferredBytes);
            var percentage = totalSize <= 0 ? 100 : Math.Min(100, (double)transferredBytes / totalSize * 100d);
            jobState.ProgressPercent = percentage;
            jobState.LastActionTimestamp = DateTime.Now;
            _state.Update(jobState);
            progressWidget.UpdateProgress(percentage);
        }

        jobState.State = hadError ? JobRunState.Failed : JobRunState.Completed;
        jobState.CurrentAction = hadError ? "completed_with_errors" : "completed";
        jobState.CurrentSourcePath = null;
        jobState.CurrentTargetPath = null;
        jobState.LastActionTimestamp = DateTime.Now;
        jobState.ProgressPercent = 100;
        jobState.RemainingFiles = 0;
        jobState.RemainingSizeBytes = 0;
        _state.Update(jobState);

        // Log job completion (keeps logs consistent and helps troubleshooting).
        _logger.Log(new LogEntry
        {
            Timestamp = DateTime.Now,
            BackupName = job.Name,
            SourcePath = _paths.ToFullUncLikePath(sourceDir),
            TargetPath = _paths.ToFullUncLikePath(targetDir),
            FileSizeBytes = 0,
            TransferTimeMs = hadError ? -1 : 0,
        });
    }

}
