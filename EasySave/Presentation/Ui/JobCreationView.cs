using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Presentation.Resources;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
/// Vue de creation d'un job de sauvegarde.
/// </summary>
internal sealed class JobCreationView
{
    private readonly IConsole _console;
    private readonly IJobRepository _repository;
    private readonly IStateSynchronizer _stateSync;
    private readonly ConsolePrompter _prompter;
    private readonly JobRepositoryErrorTranslator _errorTranslator;

    /// <summary>
    /// Construit la vue de creation.
    /// </summary>
    /// <param name="console">Console cible.</param>
    /// <param name="repository">Depot des jobs.</param>
    /// <param name="stateSync">Synchroniseur d'etat.</param>
    /// <param name="prompter">Gestionnaire de saisie.</param>
    /// <param name="errorTranslator">Traducteur d'erreurs.</param>
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
    /// Affiche l'ecran de creation et enregistre le job.
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
