namespace EasySave.Models.Backup;

/// <summary>
///     Immutable progress payload emitted during backup execution.
/// </summary>
public sealed class BackupExecutionProgressSnapshot
{
    private BackupExecutionProgressSnapshot(
        int jobId,
        string jobName,
        int currentFileIndex,
        int filesCount,
        long transferredSize,
        long totalSize,
        double currentProgress)
    {
        JobId = jobId;
        JobName = jobName;
        CurrentFileIndex = currentFileIndex;
        FilesCount = filesCount;
        TransferredSize = transferredSize;
        TotalSize = totalSize;
        CurrentProgress = currentProgress;
    }

    /// <summary>
    ///     Gets the backup job identifier.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    ///     Gets the backup job display name.
    /// </summary>
    public string JobName { get; }

    /// <summary>
    ///     Gets the zero-based index of the current file.
    /// </summary>
    public int CurrentFileIndex { get; }

    /// <summary>
    ///     Gets the total file count.
    /// </summary>
    public int FilesCount { get; }

    /// <summary>
    ///     Gets the transferred size in bytes.
    /// </summary>
    public long TransferredSize { get; }

    /// <summary>
    ///     Gets the total size in bytes.
    /// </summary>
    public long TotalSize { get; }

    /// <summary>
    ///     Gets the current progress percentage.
    /// </summary>
    public double CurrentProgress { get; }

    /// <summary>
    ///     Creates an immutable snapshot from a live backup job.
    /// </summary>
    /// <param name="job">Source job.</param>
    /// <returns>Progress snapshot.</returns>
    public static BackupExecutionProgressSnapshot FromJob(BackupJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return new BackupExecutionProgressSnapshot(
            job.Id,
            job.Name,
            job.CurrentFileIndex,
            job.FilesCount,
            job.TransferredSize,
            job.TotalSize,
            job.CurrentProgress);
    }
}
