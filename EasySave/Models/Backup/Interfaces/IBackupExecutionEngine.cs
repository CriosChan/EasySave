namespace EasySave.Models.Backup.Interfaces;

/// <summary>
///     Defines a shared asynchronous execution pipeline for backup jobs.
/// </summary>
public interface IBackupExecutionEngine
{
    /// <summary>
    ///     Executes a single backup job asynchronously.
    /// </summary>
    /// <param name="job">Job to execute.</param>
    /// <param name="progress">Optional progress sink receiving immutable runtime snapshots.</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation before execution starts.</param>
    /// <returns>Execution result information.</returns>
    Task<BackupExecutionResult> ExecuteJobAsync(
        BackupJob job,
        IProgress<BackupExecutionProgressSnapshot>? progress = null,
        CancellationToken cancellationToken = default);
}
