using EasySave.Core.Contracts;

namespace EasySave.Services;

/// <summary>
///     No-op progress reporter.
/// </summary>
public sealed class NullProgressReporter : IProgressReporter
{
    public void Report(double percentage)
    {
        // Intentionally left blank.
    }
}
