using EasySave.Models;
using EasySave.Services;
using EasySave.View.Console;

namespace EasySave.View;

internal sealed class JobRemovalView
{
    private readonly IConsole _console;
    private readonly JobRepository _repository;
    private readonly StateFileSynchronizer _stateSync;
    private readonly ConsolePrompter _prompter;

    public JobRemovalView(IConsole console, JobRepository repository, StateFileSynchronizer stateSync, ConsolePrompter prompter)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _stateSync = stateSync ?? throw new ArgumentNullException(nameof(stateSync));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
    }

    public void Show()
    {
        List<BackupJob> jobs = _repository.Load();

        _console.Clear();
        _console.WriteLine(Ressources.UserInterface.Remove_Header);
        _console.WriteLine(string.Empty);

        if (jobs.Count == 0)
        {
            _console.WriteLine(Ressources.UserInterface.Jobs_None);
            _prompter.Pause(Ressources.UserInterface.Common_PressAnyKey);
            return;
        }

        _console.WriteLine(Ressources.UserInterface.Remove_Prompt);
        string? input = _console.ReadLine();
        input = (input ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            _console.WriteLine(Ressources.UserInterface.Common_Cancelled);
            _prompter.Pause(Ressources.UserInterface.Common_PressAnyKey);
            return;
        }

        bool removed = _repository.RemoveJob(jobs, input);
        if (!removed)
        {
            _console.WriteLine(Ressources.UserInterface.Remove_NotFound);
            _prompter.Pause(Ressources.UserInterface.Common_PressAnyKey);
            return;
        }

        _stateSync.Refresh();
        _console.WriteLine(Ressources.UserInterface.Remove_Success);
        _prompter.Pause(Ressources.UserInterface.Common_PressAnyKey);
    }
}
