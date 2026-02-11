using EasySave.Application.Abstractions;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     View for removing a backup job.
/// </summary>
internal sealed class JobRemovalView
{
    private readonly IConsole _console;
    private readonly IMenuNavigator _navigator;
    private readonly ConsolePrompter _prompter;
    private readonly IJobService _jobService;
    private readonly IStateSynchronizer _stateSync;

    /// <summary>
    ///     Builds the removal view.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="jobService">Job service.</param>
    /// <param name="stateSync">State synchronizer.</param>
    /// <param name="prompter">Input prompter.</param>
    /// <param name="navigator">Menu navigator.</param>
    public JobRemovalView(
        IConsole console,
        IJobService jobService,
        IStateSynchronizer stateSync,
        ConsolePrompter prompter,
        IMenuNavigator navigator)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _stateSync = stateSync ?? throw new ArgumentNullException(nameof(stateSync));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
    }

    /// <summary>
    ///     Displays the removal screen and applies the request.
    /// </summary>
    public void Show()
    {
        var jobs = _jobService.GetAll().ToList();

        _console.Clear();

        if (jobs.Count == 0)
        {
            _console.WriteLine(Resources.UserInterface.Menu_Title_RemoveJob);
            _console.WriteLine(string.Empty);
            _console.WriteLine(Resources.UserInterface.Jobs_None);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            _navigator.ShowMainMenu();
            return;
        }

        List<Option> options = [];
        foreach (var _job in jobs)
            options.Add(new Option(string.Format(Resources.UserInterface.Jobs_Remove_ID, _job.Id, _job.Name),
                () => RemoveJob(_job.Id.ToString())));

        options.Add(new Option(Resources.UserInterface.Return, _navigator.ShowMainMenu));

        ListWidget.ShowList(options, _console, Resources.UserInterface.Menu_Title_RemoveJob);
    }

    private void RemoveJob(string jobId)
    {
        if (_jobService.RemoveJob(jobId))
            _stateSync.Refresh();
        Show();
    }
}
