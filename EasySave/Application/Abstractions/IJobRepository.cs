using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
/// Contract for backup job persistence.
/// </summary>
public interface IJobRepository
{
    /// <summary>
    /// Loads the list of jobs.
    /// </summary>
    /// <returns>List of jobs.</returns>
    List<BackupJob> Load();

    /// <summary>
    /// Saves the list of jobs.
    /// </summary>
    /// <param name="jobs">Jobs to persist.</param>
    void Save(List<BackupJob> jobs);

    /// <summary>
    /// Adds a job to the list and persists it.
    /// </summary>
    /// <param name="jobs">Current in-memory list.</param>
    /// <param name="job">Job to add.</param>
    /// <returns>Result and optional error code.</returns>
    (bool ok, string error) AddJob(List<BackupJob> jobs, BackupJob job);

    /// <summary>
    /// Removes a job by id or by name.
    /// </summary>
    /// <param name="jobs">Current in-memory list.</param>
    /// <param name="idOrName">Job id or name.</param>
    /// <returns>True if a job was removed.</returns>
    bool RemoveJob(List<BackupJob> jobs, string idOrName);
}
