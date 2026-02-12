namespace EasySave.Core.Models;

/// <summary>
///     Possible states for a running job.
/// </summary>
public enum JobRunState
{
    /// <summary>
    ///     Job is inactive.
    /// </summary>
    Inactive,

    /// <summary>
    ///     Job is active.
    /// </summary>
    Active,

    /// <summary>
    ///     Job completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    ///     Job completed with errors.
    /// </summary>
    Failed
}