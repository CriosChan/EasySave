using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
///     Runtime controls for parallel backup execution.
/// </summary>
public interface IBackupRuntimeController
{
    event Action<BackupJobState>? JobStateChanged;
    Task RunJobsParallelAsync(IEnumerable<BackupJob> jobs, CancellationToken cancellationToken = default);
    Task RunJobAsync(BackupJob job, CancellationToken cancellationToken = default);
    void PauseJob(int jobId);
    void ResumeJob(int jobId);
    void StopJob(int jobId);
    void PauseAll();
    void ResumeAll();
    void StopAll();
}
