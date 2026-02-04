using System.Diagnostics;
using EasyLog;
using EasySave.Models;
using EasySave.Utils;

namespace EasySave.Services;

/// <summary>
/// Executes backup jobs (complete or differential) and writes both log and state files.
/// </summary>
/// <remarks>
/// Version 1.0/1.1 requirement:
/// - Jobs can be executed individually or sequentially (no parallel/multi-thread execution).
/// - Every file transfer and directory creation must be logged in a daily JSON file.
/// - A "state" JSON file must be updated in real-time to show progress.
/// </remarks>
public sealed class BackupService
{
    private readonly AbstractLogger<LogEntry> _logger;
    private readonly StateFileService _state;

    /// <summary>
    /// Creates a new backup service.
    /// </summary>
    /// <param name="logger">Logger implementation used to write daily logs.</param>
    /// <param name="state">State service used to update the real-time state file.</param>
    internal BackupService(AbstractLogger<LogEntry> logger, StateFileService state)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>
    /// Creates a new backup service.
    /// </summary>
    /// <param name="logDirectory">Directory where the daily log file will be created.</param>
    /// <param name="state">State service used to update the real-time state file.</param>
    public BackupService(string logDirectory, StateFileService state)
        : this(new JsonLogger<LogEntry>(logDirectory), state)
    {
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
        bool sourceOk = PathTools.TryNormalizeExistingDirectory(job.SourceDirectory, out string sourceDir);
        bool targetOk = PathTools.TryNormalizeExistingDirectory(job.TargetDirectory, out string targetDir);

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
                SourcePath = PathTools.ToFullUncLikePath(sourceDir),
                TargetPath = PathTools.ToFullUncLikePath(targetDir),
                FileSizeBytes = 0,
                TransferTimeMs = -1,
                // Action and Error fields removed from LogEntry
            });

            return;
        }

        // Validate target directory.
        // For v1.0, we only execute the job if both source and target directories exist.
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
                SourcePath = PathTools.ToFullUncLikePath(sourceDir),
                TargetPath = PathTools.ToFullUncLikePath(targetDir),
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
            SourcePath = PathTools.ToFullUncLikePath(sourceDir),
            TargetPath = PathTools.ToFullUncLikePath(targetDir),
            FileSizeBytes = 0,
            TransferTimeMs = 0,
        });

        // Mirror the directory structure (including empty directories).
        // This matches the requirement: all subdirectories must be preserved.
        CreateMissingTargetDirectories(job, sourceDir, targetDir);

        List<string> filesToCopy = GetFilesToCopy(job, sourceDir, targetDir);
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

        foreach (string sourceFile in filesToCopy)
        {
            string relative = PathTools.GetRelativePath(sourceDir, sourceFile);
            string targetFile = Path.Combine(targetDir, relative);
            string? targetFileDir = Path.GetDirectoryName(targetFile);

            if (!string.IsNullOrWhiteSpace(targetFileDir) && !Directory.Exists(targetFileDir))
            {
                Directory.CreateDirectory(targetFileDir);
                // Log directory creation (requested by the specification).
                _logger.Log(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourcePath = PathTools.ToFullUncLikePath(Path.GetDirectoryName(sourceFile) ?? sourceDir),
                    TargetPath = PathTools.ToFullUncLikePath(targetFileDir),
                    FileSizeBytes = 0,
                    TransferTimeMs = 0,
                    // Action field removed from LogEntry
                });
            }

            jobState.CurrentAction = "file_transfer";
            jobState.CurrentSourcePath = PathTools.ToFullUncLikePath(sourceFile);
            jobState.CurrentTargetPath = PathTools.ToFullUncLikePath(targetFile);
            jobState.LastActionTimestamp = DateTime.Now;
            _state.Update(jobState);

            long fileSize = 0;
            long elapsedMs;
            string? error = null;

            try
            {
                FileInfo fi = new FileInfo(sourceFile);
                fileSize = fi.Length;

                elapsedMs = CopyFileWithTiming(sourceFile, targetFile);

                // Preserve source timestamps.
                File.SetLastWriteTimeUtc(targetFile, fi.LastWriteTimeUtc);
            }
            catch (Exception ex)
            {
                hadError = true;
                elapsedMs = -1;
                error = ex.Message;
            }

            _logger.Log(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = job.Name,
                SourcePath = PathTools.ToFullUncLikePath(sourceFile),
                TargetPath = PathTools.ToFullUncLikePath(targetFile),
                FileSizeBytes = fileSize,
                TransferTimeMs = elapsedMs,
                // Action and Error fields removed from LogEntry
            });

            if (elapsedMs >= 0)
                transferredBytes += fileSize;

            jobState.RemainingFiles = Math.Max(0, jobState.RemainingFiles - 1);
            jobState.RemainingSizeBytes = Math.Max(0, totalSize - transferredBytes);
            jobState.ProgressPercent = totalSize <= 0 ? 100 : Math.Min(100, (double)transferredBytes / totalSize * 100d);
            jobState.LastActionTimestamp = DateTime.Now;
            _state.Update(jobState);
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
            SourcePath = PathTools.ToFullUncLikePath(sourceDir),
            TargetPath = PathTools.ToFullUncLikePath(targetDir),
            FileSizeBytes = 0,
            TransferTimeMs = hadError ? -1 : 0,
        });
    }

    /// <summary>
    /// Computes the list of eligible files for the given job.
    /// </summary>
    /// <remarks>
    /// - Complete: all files from the source directory (recursive).
    /// - Differential: only files that are missing in the target or that differ
    ///   (size mismatch or source last write time is newer).
    /// </remarks>
    private List<string> GetFilesToCopy(BackupJob job)
    {
        return GetFilesToCopy(job, job.SourceDirectory, job.TargetDirectory);
    }

    private List<string> GetFilesToCopy(BackupJob job, string sourceDir, string targetDir)
    {
        IEnumerable<string> allFiles = Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories);

        if (job.Type == BackupType.Complete)
            return allFiles.ToList();

        // Differential: copy files that do not exist in target, or that are older/different.
        List<string> differential = new();
        foreach (string sourceFile in allFiles)
        {
            string rel = PathTools.GetRelativePath(sourceDir, sourceFile);
            string targetFile = Path.Combine(targetDir, rel);
            if (!File.Exists(targetFile))
            {
                differential.Add(sourceFile);
                continue;
            }

            FileInfo src = new FileInfo(sourceFile);
            FileInfo dst = new FileInfo(targetFile);

            bool isDifferent = src.Length != dst.Length || src.LastWriteTimeUtc > dst.LastWriteTimeUtc;
            if (isDifferent)
                differential.Add(sourceFile);
        }

        return differential;
    }

    private void CreateMissingTargetDirectories(BackupJob job, string sourceDir, string targetDir)
    {
        try
        {
            foreach (string srcDir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relative = PathTools.GetRelativePath(sourceDir, srcDir);
                string dstDir = Path.Combine(targetDir, relative);

                if (Directory.Exists(dstDir))
                    continue;

                Directory.CreateDirectory(dstDir);
                _logger.Log(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourcePath = PathTools.ToFullUncLikePath(srcDir),
                    TargetPath = PathTools.ToFullUncLikePath(dstDir),
                    FileSizeBytes = 0,
                    TransferTimeMs = 0,
                });
            }
        }
        catch
        {
            // Directory enumeration/creation errors will be reflected during file transfers.
        }
    }

    /// <summary>
    /// Copies a file using a buffered stream and returns the transfer duration in milliseconds.
    /// </summary>
    /// <remarks>
    /// Uses a buffered stream copy (1 MiB) to keep reasonable performance for large files.
    /// </remarks>
    /// <param name="sourceFile">Full path to the source file.</param>
    /// <param name="targetFile">Full path to the target file.</param>
    /// <returns>Elapsed time in milliseconds.</returns>
    private static long CopyFileWithTiming(string sourceFile, string targetFile)
    {
        const int BufferSize = 1024 * 1024; // 1 MiB
        Stopwatch sw = Stopwatch.StartNew();

        using FileStream source = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan);
        using FileStream target = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.SequentialScan);
        source.CopyTo(target, BufferSize);

        sw.Stop();
        return sw.ElapsedMilliseconds;
    }
}
