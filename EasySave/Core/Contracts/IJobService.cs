using EasySave.Core.Models;

namespace EasySave.Core.Contracts;

/// <summary>
///     Contract for backup job management.
/// </summary>
public interface IJobService
{
    /// <summary>
    ///     Returns all configured jobs.
    /// </summary>
    /// <returns>List of jobs.</returns>
    IReadOnlyList<BackupJob> GetAll();

    /// <summary>
    ///     Adds a new job, applying business rules (max jobs, id assignment).
    /// </summary>
    /// <param name="job">Job to add.</param>
    /// <returns>Result and optional error code.</returns>
    (bool ok, string error) AddJob(BackupJob job);

    /// <summary>
    ///     Removes a job by id or name.
    /// </summary>
    /// <param name="idOrName">Id ("1") or job name.</param>
    /// <returns>True if a job was removed.</returns>
    bool RemoveJob(string idOrName);
}
