using EasySave.Models;
using EasySave.Services;
using EasySave.View.Console;

namespace EasySave.View;

internal sealed class JobCreationView
{
    private readonly IConsole _console;
    private readonly JobRepository _repository;
    private readonly StateFileSynchronizer _stateSync;
    private readonly ConsolePrompter _prompter;
    private readonly JobRepositoryErrorTranslator _errorTranslator;

    public JobCreationView(
        IConsole console,
        JobRepository repository,
        StateFileSynchronizer stateSync,
        ConsolePrompter prompter,
        JobRepositoryErrorTranslator errorTranslator)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _stateSync = stateSync ?? throw new ArgumentNullException(nameof(stateSync));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
        _errorTranslator = errorTranslator ?? throw new ArgumentNullException(nameof(errorTranslator));
    }

    public void Show()
    {
        List<BackupJob> jobs = _repository.Load();

        _console.Clear();
        _console.WriteLine(Text.Get("Add.Header"));
        _console.WriteLine(string.Empty);

        string name = _prompter.ReadNonEmpty(Text.Get("Add.PromptName"));
        string source = _prompter.ReadExistingDirectory(Text.Get("Add.PromptSource"), Text.Get("Path.SourceNotFound"));
        string target = _prompter.ReadExistingDirectory(Text.Get("Add.PromptTarget"), Text.Get("Path.TargetNotFound"));

        BackupType type = _prompter.ReadBackupType(
            Text.Get("Add.PromptType"),
            Text.Get("Add.TypeOptions"),
            Text.Get("Common.InvalidInput"));

        BackupJob newJob = new BackupJob
        {
            Name = name,
            SourceDirectory = source,
            TargetDirectory = target,
            Type = type
        };

        (bool ok, string error) = _repository.AddJob(jobs, newJob);

        if (!ok)
        {
            _console.WriteLine(string.Empty);
            _console.WriteLine(Text.Get("Add.Failed"));
            _console.WriteLine(_errorTranslator.Translate(error));
            _prompter.Pause(Text.Get("Common.PressAnyKey"));
            return;
        }

        _stateSync.Refresh();

        _console.WriteLine(string.Empty);
        _console.WriteLine(Text.Get("Add.Success"));
        _prompter.Pause(Text.Get("Common.PressAnyKey"));
    }
}
