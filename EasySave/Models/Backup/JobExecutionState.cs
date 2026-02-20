namespace EasySave.Models.Backup;

/// <summary>
///     Represents the execution state of a job within the orchestrator.
/// </summary>
public enum JobExecutionState
{
    /// <summary>
    ///     Job is waiting to start.
    /// </summary>
    Pending,

    /// <summary>
    ///     Job is currently executing.
    /// </summary>
    Active,

    /// <summary>
    ///     Job completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    ///     Job failed with an error.
    /// </summary>
    Failed,

    /// <summary>
    ///     Job was cancelled by user request.
    /// </summary>
    Cancelled,

    /// <summary>
    ///     Job was stopped because business software was detected.
    /// </summary>
    StoppedByBusinessSoftware,

    /// <summary>
    ///     Job was skipped (e.g., due to earlier business software detection).
    /// </summary>
    Skipped
}