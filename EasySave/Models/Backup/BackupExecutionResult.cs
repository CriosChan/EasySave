namespace EasySave.Models.Backup;

/// <summary>
///     Immutable result payload returned after executing a backup job.
/// </summary>
public sealed class BackupExecutionResult
{
    private BackupExecutionResult(int jobId, string jobName, bool wasStoppedByBusinessSoftware)
    {
        JobId = jobId;
        JobName = jobName;
        WasStoppedByBusinessSoftware = wasStoppedByBusinessSoftware;
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
    ///     Gets a value indicating whether execution stopped because business software was detected.
    /// </summary>
    public bool WasStoppedByBusinessSoftware { get; }

    /// <summary>
    ///     Creates a result payload from a completed job instance.
    /// </summary>
    /// <param name="job">Executed job.</param>
    /// <returns>Execution result.</returns>
    public static BackupExecutionResult FromJob(BackupJob job)
    {
        ArgumentNullException.ThrowIfNull(job);
        return new BackupExecutionResult(job.Id, job.Name, job.WasStoppedByBusinessSoftware);
    }
}
