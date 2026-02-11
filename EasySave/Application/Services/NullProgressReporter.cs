using EasySave.Application.Abstractions;

namespace EasySave.Application.Services;

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
