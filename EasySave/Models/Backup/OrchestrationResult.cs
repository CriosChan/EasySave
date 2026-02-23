namespace EasySave.Models.Backup;

/// <summary>
///     Result summary from orchestrated execution.
/// </summary>
public sealed class OrchestrationResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OrchestrationResult" /> class.
    /// </summary>
    public OrchestrationResult(int completedCount, int failedCount, int cancelledCount, bool wasStoppedByBusinessSoftware)
    {
        CompletedCount = completedCount;
        FailedCount = failedCount;
        CancelledCount = cancelledCount;
        WasStoppedByBusinessSoftware = wasStoppedByBusinessSoftware;
    }

    /// <summary>
    ///     Gets the number of jobs that completed successfully.
    /// </summary>
    public int CompletedCount { get; }

    /// <summary>
    ///     Gets the number of jobs that failed.
    /// </summary>
    public int FailedCount { get; }

    /// <summary>
    ///     Gets the number of jobs that were cancelled or skipped.
    /// </summary>
    public int CancelledCount { get; }

    /// <summary>
    ///     Gets a value indicating whether execution was stopped due to business software.
    /// </summary>
    public bool WasStoppedByBusinessSoftware { get; }
}