using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
///     Contract for managing backup execution state.
/// </summary>
public interface IStateService
{
    /// <summary>
    ///     Initializes state for a set of jobs.
    /// </summary>
    /// <param name="jobs">Jobs to initialize.</param>
    void Initialize(IEnumerable<BackupJob> jobs);

    /// <summary>
    ///     Updates the state of a job.
    /// </summary>
    /// <param name="updated">Updated state.</param>
    void Update(BackupJobState updated);

    /// <summary>
    ///     Gets the state of a job or creates it if it does not exist.
    /// </summary>
    /// <param name="job">Target job.</param>
    /// <returns>Current state.</returns>
    BackupJobState GetOrCreate(BackupJob job);
}