using EasySave.Models;
using EasySave.Services;
using EasySave.View.Console;

namespace EasySave.View;

internal sealed class JobListView
{
    private readonly IConsole _console;
    private readonly JobRepository _repository;
    private readonly ConsolePrompter _prompter;

    public JobListView(IConsole console, JobRepository repository, ConsolePrompter prompter)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
    }

    public void Show()
    {
        List<BackupJob> jobs = _repository.Load().OrderBy(j => j.Id).ToList();

        _console.Clear();
        _console.WriteLine(Ressources.UserInterface.Jobs_Header);
        _console.WriteLine(string.Empty);

        if (jobs.Count == 0)
        {
            _console.WriteLine(Ressources.UserInterface.Jobs_None);
            _prompter.Pause(Ressources.UserInterface.Common_PressAnyKey);
            return;
        }

        _console.WriteLine(Ressources.UserInterface.Jobs_Columns);
        _console.WriteLine(new string('-', 80));

        foreach (BackupJob job in jobs)
            _console.WriteLine($"{job.Id,2} | {job.Name,-20} | {job.Type,-12} | {job.SourceDirectory} -> {job.TargetDirectory}");

        _prompter.Pause(Ressources.UserInterface.Common_PressAnyKey);
    }
}
