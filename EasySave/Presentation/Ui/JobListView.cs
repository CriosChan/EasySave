using EasySave.Application.Abstractions;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     View for listing backup jobs.
/// </summary>
internal sealed class JobListView
{
    private readonly IConsole _console;
    private readonly ConsolePrompter _prompter;
    private readonly IJobService _jobService;

    /// <summary>
    ///     Builds the list view.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="jobService">Job service.</param>
    /// <param name="prompter">Input prompter.</param>
    public JobListView(IConsole console, IJobService jobService, ConsolePrompter prompter)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
    }

    /// <summary>
    ///     Displays the list of available jobs.
    /// </summary>
    public void Show()
    {
        var jobs = _jobService.GetAll().OrderBy(j => j.Id).ToList();

        _console.Clear();
        _console.WriteLine(Resources.UserInterface.Jobs_Header);
        _console.WriteLine(string.Empty);

        if (jobs.Count == 0)
        {
            _console.WriteLine(Resources.UserInterface.Jobs_None);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        _console.WriteLine(Resources.UserInterface.Jobs_Columns);
        _console.WriteLine(new string('-', 80));

        foreach (var job in jobs)
            _console.WriteLine(
                $"{job.Id,2} | {job.Name,-20} | {job.Type,-12} | {job.SourceDirectory} -> {job.TargetDirectory}");

        _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
    }
}
