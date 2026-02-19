using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Shared asynchronous execution engine used by GUI and CLI callers.
/// </summary>
public sealed class BackupExecutionEngine : IBackupExecutionEngine
{
    /// <inheritdoc />
    public async Task<BackupExecutionResult> ExecuteJobAsync(
        BackupJob job,
        IProgress<BackupExecutionProgressSnapshot>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);
        cancellationToken.ThrowIfCancellationRequested();

        EventHandler? progressHandler = null;
        if (progress != null)
        {
            progressHandler = (_, _) => progress.Report(BackupExecutionProgressSnapshot.FromJob(job));
            job.ProgressChanged += progressHandler;
        }

        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                job.StartBackup();
            }, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            return BackupExecutionResult.FromJob(job);
        }
        finally
        {
            if (progressHandler != null)
                job.ProgressChanged -= progressHandler;
        }
    }
}
