using EasySave.Core.Contracts;

namespace EasySave.Services;

public sealed class UiProgressReporter : IProgressReporter
{
    public event Action<double>? ProgressChanged;

    public void Report(double percentage)
    {
        ProgressChanged?.Invoke(percentage);
    }
}
