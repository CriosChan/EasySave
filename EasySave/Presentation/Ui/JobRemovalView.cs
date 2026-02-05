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
        _console.WriteLine(Resources.UserInterface.Remove_Header);
        _console.WriteLine(string.Empty);

        if (jobs.Count == 0)
        {
            _console.WriteLine(Resources.UserInterface.Jobs_None);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        _console.WriteLine(Resources.UserInterface.Remove_Prompt);
        string? input = _console.ReadLine();
        input = (input ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            _console.WriteLine(Resources.UserInterface.Common_Cancelled);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        bool removed = _repository.RemoveJob(jobs, input);
        if (!removed)
        {
            _console.WriteLine(Resources.UserInterface.Remove_NotFound);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        _stateSync.Refresh();
        _console.WriteLine(Resources.UserInterface.Remove_Success);
        _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
    }
}
