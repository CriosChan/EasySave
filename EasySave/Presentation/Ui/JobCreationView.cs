using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Presentation.Resources;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
/// View for creating a backup job.
/// </summary>
internal sealed class JobCreationView
{
    private readonly IConsole _console;
    private readonly IJobRepository _repository;
    private readonly IStateSynchronizer _stateSync;
    private readonly ConsolePrompter _prompter;
    private readonly JobRepositoryErrorTranslator _errorTranslator;

    /// <summary>
    /// Builds the creation view.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="repository">Job repository.</param>
    /// <param name="stateSync">State synchronizer.</param>
    /// <param name="prompter">Input prompter.</param>
    /// <param name="errorTranslator">Error translator.</param>
    public JobCreationView(
        IConsole console,
        IJobRepository repository,
        IStateSynchronizer stateSync,
        ConsolePrompter prompter,
        JobRepositoryErrorTranslator errorTranslator)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _stateSync = stateSync ?? throw new ArgumentNullException(nameof(stateSync));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
        _errorTranslator = errorTranslator ?? throw new ArgumentNullException(nameof(errorTranslator));
    }

    /// <summary>
    /// Displays the creation screen and saves the job.
    /// </summary>
    public void Show()
    {
        List<BackupJob> jobs = _repository.Load();

        _console.Clear();
        _console.WriteLine(Resources.UserInterface.Add_Header);
        _console.WriteLine(string.Empty);

        string name = _prompter.ReadNonEmpty(Resources.UserInterface.Add_PromptName);
        string source = _prompter.ReadExistingDirectory(Resources.UserInterface.Add_PromptSource, Resources.UserInterface.Path_SourceNotFound);
        string target = _prompter.ReadExistingDirectory(Resources.UserInterface.Add_PromptTarget, Resources.UserInterface.Path_TargetNotFound);

        BackupType type = _prompter.ReadBackupType(
            Resources.UserInterface.Add_PromptType,
            Resources.UserInterface.Add_TypeOptions,
            Resources.UserInterface.Common_InvalidInput);

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
            _console.WriteLine(Resources.UserInterface.Add_Failed);
            _console.WriteLine(_errorTranslator.Translate(error));
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        _stateSync.Refresh();

        _console.WriteLine(string.Empty);
        _console.WriteLine(Resources.UserInterface.Add_Success);
        _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
    }
}
