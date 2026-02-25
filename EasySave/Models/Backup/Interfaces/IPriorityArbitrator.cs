namespace EasySave.Models.Backup.Interfaces;

/// <summary>
///     Defines a global priority arbitrator that manages file processing order across all backup jobs.
///     Ensures that standard (non-priority) files are not processed while any priority files remain
///     in the system, enforcing the v3 business rule globally.
/// </summary>
public interface IPriorityArbitrator
{
    /// <summary>
    ///     Gets the current global count of priority files remaining across all active jobs.
    /// </summary>
    /// <returns>Total number of priority files not yet processed.</returns>
    int GetGlobalPriorityFilesRemaining();

    /// <summary>
    ///     Updates the priority file count for a specific job when its partition changes
    ///     (e.g., after processing a priority file).
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="newPriorityCount">The updated priority queue count for this job.</param>
    void UpdateGlobalPriorityCount(int jobId, int newPriorityCount);

    /// <summary>
    ///     Determines whether a standard (non-priority) file can be processed by the given job.
    ///     Returns false if any priority files remain globally, enforcing the priority rule.
    /// </summary>
    /// <param name="jobId">The job requesting to process a standard file.</param>
    /// <returns>True if the file can be processed; false if it should be deferred.</returns>
    bool CanProcessStandardFile(int jobId);

    /// <summary>
    ///     Signals that a job has completed, removing it from priority tracking.
    /// </summary>
    /// <param name="jobId">The job that has completed.</param>
    void OnJobCompleted(int jobId);

    /// <summary>
    ///     Initializes the arbitrator with the priority file counts for all jobs.
    ///     Must be called before jobs begin processing files.
    /// </summary>
    /// <param name="jobPriorityCounts">
    ///     Dictionary mapping job IDs to their respective priority file queue counts.
    /// </param>
    void Initialize(Dictionary<int, int> jobPriorityCounts);
}
