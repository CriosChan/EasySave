namespace EasySave.Core.Contracts;

/// <summary>
///     Reports progress for long-running operations.
/// </summary>
public interface IProgressReporter
{
    /// <summary>
    ///     Reports a progress percentage (0..100).
    /// </summary>
    /// <param name="percentage">Progress percentage.</param>
    void Report(double percentage);
}
