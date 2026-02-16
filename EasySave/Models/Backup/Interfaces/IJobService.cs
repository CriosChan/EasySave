namespace EasySave.Models.Backup.Interfaces;

/// <summary>
///     Defines backup job management operations exposed to presentation layers.
/// </summary>
public interface IJobService
{
    /// <summary>
    ///     Retrieves all backup jobs, ordered by their identifier.
    /// </summary>
    /// <returns>Read-only list of configured backup jobs.</returns>
    IReadOnlyList<BackupJob> GetAll();

    /// <summary>
    ///     Adds a new backup job.
    /// </summary>
    /// <param name="job">Job to create.</param>
    /// <returns>Operation result and optional error code.</returns>
    (bool ok, string error) AddJob(BackupJob job);

    /// <summary>
    ///     Removes a backup job by id or name.
    /// </summary>
    /// <param name="idOrName">Identifier or name value.</param>
    /// <returns>True when a job was removed; otherwise, false.</returns>
    bool RemoveJob(string idOrName);
}
