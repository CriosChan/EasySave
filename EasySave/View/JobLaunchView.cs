using EasySave.Models;
using EasySave.Services;
using EasySave.Utils;
using EasySave.View.Console;

namespace EasySave.View;

internal sealed class JobLaunchView
{
    private readonly IConsole _console;
    private readonly JobRepository _repository;
    private readonly BackupService _backupService;
    private readonly StateFileService _stateService;
    private readonly ConsolePrompter _prompter;

    public JobLaunchView(
        IConsole console,
        JobRepository repository,
        BackupService backupService,
        StateFileService stateService,
        ConsolePrompter prompter)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
    }

    public void Show()
    {
        List<BackupJob> jobs = _repository.Load().OrderBy(j => j.Id).ToList();

        _console.Clear();
        _console.WriteLine(Text.Get("Launch.Header"));
        _console.WriteLine(string.Empty);

        if (jobs.Count == 0)
        {
            _console.WriteLine(Text.Get("Jobs.None"));
            _prompter.Pause(Text.Get("Common.PressAnyKey"));
            return;
        }

        _console.WriteLine(Text.Get("Launch.Prompt"));
        string? input = _console.ReadLine();
        input = (input ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            _console.WriteLine(Text.Get("Common.Cancelled"));
            _prompter.Pause(Text.Get("Common.PressAnyKey"));
            return;
        }

        // Always re-initialize the state file so it contains all jobs before execution.
        _stateService.Initialize(jobs);

        if (input == "0")
        {
            RunAll(jobs);
            _prompter.Pause(Text.Get("Common.PressAnyKey"));
            return;
        }

        if (!int.TryParse(input, out int id))
        {
            _console.WriteLine(Text.Get("Launch.Invalid"));
            _prompter.Pause(Text.Get("Common.PressAnyKey"));
            return;
        }

        BackupJob? job = jobs.FirstOrDefault(j => j.Id == id);
        if (job == null)
        {
            _console.WriteLine(Text.Get("Launch.NotFound"));
            _prompter.Pause(Text.Get("Common.PressAnyKey"));
            return;
        }

        RunOne(job);
        _prompter.Pause(Text.Get("Common.PressAnyKey"));
    }

    private void RunAll(List<BackupJob> jobs)
    {
        _console.WriteLine(Text.Get("Launch.RunningAll"));

        foreach (BackupJob j in jobs.OrderBy(j => j.Id))
        {
            if (!PathTools.TryNormalizeExistingDirectory(j.SourceDirectory, out _))
            {
                _console.WriteLine($"[{j.Id}] {Text.Get("Path.SourceNotFound")}");
                continue;
            }
            if (!PathTools.TryNormalizeExistingDirectory(j.TargetDirectory, out _))
            {
                _console.WriteLine($"[{j.Id}] {Text.Get("Path.TargetNotFound")}");
                continue;
            }

            _backupService.RunJob(j);
        }

        _console.WriteLine(Text.Get("Launch.Done"));
    }

    private void RunOne(BackupJob job)
    {
        _console.WriteLine(string.Format(Text.Get("Launch.RunningOne"), job.Id, job.Name));

        // Validate directories before launching so the UI does not report a run for invalid paths.
        if (!PathTools.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
        {
            _console.WriteLine(Text.Get("Path.SourceNotFound"));
            return;
        }
        if (!PathTools.TryNormalizeExistingDirectory(job.TargetDirectory, out _))
        {
            _console.WriteLine(Text.Get("Path.TargetNotFound"));
            return;
        }

        _backupService.RunJob(job);
        _console.WriteLine(Text.Get("Launch.Done"));
    }
}
