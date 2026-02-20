namespace EasySave.Models.State;

/// <summary>
///     Possible states for a running job.
/// </summary>
public enum JobRunState
{
    /// <summary>
    ///     Job is inactive.
    /// </summary>
    Inactive = 0,

    /// <summary>
    ///     Job is active.
    /// </summary>
    Active = 1,

    /// <summary>
    ///     Job completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    ///     Job completed with errors.
    /// </summary>
    Failed = 3,

    /// <summary>
    ///     Job is paused by user action.
    /// </summary>
    Paused = 4,

    /// <summary>
    ///     Job was explicitly stopped.
    /// </summary>
    Stopped = 5,

    /// <summary>
    ///     Job is waiting because priority-file scheduling blocks non-priority work.
    /// </summary>
    WaitingPriority = 6,

    /// <summary>
    ///     Job is waiting for large-file transfer slot availability.
    /// </summary>
    WaitingLargeFile = 7,

    /// <summary>
    ///     Job is paused due to business software detection.
    /// </summary>
    PausedBusinessSoftware = 8
}
