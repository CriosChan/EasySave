using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
///     Contract for backup job persistence.
/// </summary>
public interface IJobRepository
{
    /// <summary>
    ///     Loads the list of jobs.
    /// </summary>
    /// <returns>List of jobs.</returns>
    IReadOnlyList<BackupJob> GetAll();

    /// <summary>
    ///     Persists the list of jobs.
    /// </summary>
    /// <param name="jobs">Jobs to persist.</param>
    void SaveAll(IEnumerable<BackupJob> jobs);
}
