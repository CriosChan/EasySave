using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
/// Contract for backup execution.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Executes a list of jobs sequentially.
    /// </summary>
    /// <param name="jobs">Jobs to execute.</param>
    void RunJobsSequential(IEnumerable<BackupJob> jobs);

    /// <summary>
    /// Executes a backup job.
    /// </summary>
    /// <param name="job">Job to execute.</param>
    void RunJob(BackupJob job);
}
