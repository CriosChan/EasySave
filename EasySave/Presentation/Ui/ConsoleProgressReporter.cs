using EasySave.Application.Abstractions;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     Console progress reporter based on the ProgressWidget.
/// </summary>
internal sealed class ConsoleProgressReporter : IProgressReporter
{
    private readonly ProgressWidget _widget;

    public ConsoleProgressReporter(IConsole console)
    {
        if (console == null)
            throw new ArgumentNullException(nameof(console));

        _widget = new ProgressWidget(console);
    }

    public void Report(double percentage)
    {
        _widget.UpdateProgress(percentage);
    }
}
