using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Presentation.Resources;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
/// View for removing a backup job.
/// </summary>
internal sealed class JobRemovalView
{
    private readonly IConsole _console;
    private readonly IJobRepository _repository;
    private readonly IStateSynchronizer _stateSync;
    private readonly ConsolePrompter _prompter;

    /// <summary>
    /// Builds the removal view.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="repository">Job repository.</param>
    /// <param name="stateSync">State synchronizer.</param>
    /// <param name="prompter">Input prompter.</param>
    public JobRemovalView(IConsole console, IJobRepository repository, IStateSynchronizer stateSync, ConsolePrompter prompter)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _stateSync = stateSync ?? throw new ArgumentNullException(nameof(stateSync));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
    }

    /// <summary>
    /// Displays the removal screen and applies the request.
    /// </summary>
    public void Show()
    {
        List<BackupJob> jobs = _repository.Load();

        _console.Clear();

        if (jobs.Count == 0)
        {
            _console.WriteLine(Resources.UserInterface.Menu_Title_RemoveJob);
            _console.WriteLine(string.Empty);
            _console.WriteLine(Resources.UserInterface.Jobs_None);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            UserInterface.ShowMenu();
            return;
        }

        List<Option> options = [];
        foreach (var _job in jobs)
        {
            options.Add(new Option(String.Format(Resources.UserInterface.Jobs_Remove_ID, _job.Id, _job.Name),
                () => RemoveJob(jobs, _job.Id.ToString())));
        }

        options.Add(new Option(Resources.UserInterface.Return, UserInterface.ShowMenu));
        
        ListWidget.ShowList(options, _console, Resources.UserInterface.Menu_Title_RemoveJob);
    }

    private void RemoveJob(List<BackupJob> jobs, string jobId)
    {
        _repository.RemoveJob(jobs, jobId);
        Show();
    }
}
