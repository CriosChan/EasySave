using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Presentation.Resources;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
/// Vue de lancement d'un job de sauvegarde.
/// </summary>
internal sealed class JobLaunchView
{
    private readonly IConsole _console;
    private readonly IJobRepository _repository;
    private readonly IBackupService _backupService;
    private readonly IStateService _stateService;
    private readonly ConsolePrompter _prompter;
    private readonly IPathService _paths;

    /// <summary>
    /// Construit la vue de lancement.
    /// </summary>
    /// <param name="console">Console cible.</param>
    /// <param name="repository">Depot des jobs.</param>
    /// <param name="backupService">Service de sauvegarde.</param>
    /// <param name="stateService">Service d'etat.</param>
    /// <param name="prompter">Gestionnaire de saisie.</param>
    /// <param name="paths">Service de chemins.</param>
    public JobLaunchView(
        IConsole console,
        IJobRepository repository,
        IBackupService backupService,
        IStateService stateService,
        ConsolePrompter prompter,
        IPathService paths)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    /// <summary>
    /// Affiche l'ecran de lancement et execute le(s) job(s) selectionne(s).
    /// </summary>
    public void Show()
    {
        List<BackupJob> jobs = _repository.Load().OrderBy(j => j.Id).ToList();

        _console.Clear();
        _console.WriteLine(Resources.UserInterface.Launch_Header);
        _console.WriteLine(string.Empty);

        if (jobs.Count == 0)
        {
            _console.WriteLine(Resources.UserInterface.Jobs_None);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        _console.WriteLine(Resources.UserInterface.Launch_Prompt);
        string? input = _console.ReadLine();
        input = (input ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            _console.WriteLine(Resources.UserInterface.Common_Cancelled);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        // Always re-initialize the state file so it contains all jobs before execution.
        _stateService.Initialize(jobs);

        if (input == "0")
        {
            RunAll(jobs);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        if (!int.TryParse(input, out int id))
        {
            _console.WriteLine(Resources.UserInterface.Launch_Invalid);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        BackupJob? job = jobs.FirstOrDefault(j => j.Id == id);
        if (job == null)
        {
            _console.WriteLine(Resources.UserInterface.Launch_NotFound);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        RunOne(job);
        _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
    }

    /// <summary>
    /// Execute tous les jobs valides.
    /// </summary>
    /// <param name="jobs">Liste des jobs.</param>
    private void RunAll(List<BackupJob> jobs)
    {
        _console.WriteLine(Resources.UserInterface.Launch_RunningAll);

        foreach (BackupJob j in jobs.OrderBy(j => j.Id))
        {
            if (!_paths.TryNormalizeExistingDirectory(j.SourceDirectory, out _))
            {
                _console.WriteLine($"[{j.Id}] {Resources.UserInterface.Path_SourceNotFound}");
                continue;
            }
            if (!_paths.TryNormalizeExistingDirectory(j.TargetDirectory, out _))
            {
                _console.WriteLine($"[{j.Id}] {Resources.UserInterface.Path_TargetNotFound}");
                continue;
            }

            _backupService.RunJob(j);
        }

        _console.WriteLine(Resources.UserInterface.Launch_Done);
    }

    /// <summary>
    /// Execute un job specifique.
    /// </summary>
    /// <param name="job">Job a lancer.</param>
    private void RunOne(BackupJob job)
    {
        _console.WriteLine(string.Format(Resources.UserInterface.Launch_RunningOne, job.Id, job.Name));

        // Validate directories before launching so the UI does not report a run for invalid paths.
        if (!_paths.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
        {
            _console.WriteLine(Resources.UserInterface.Path_SourceNotFound);
            return;
        }
        if (!_paths.TryNormalizeExistingDirectory(job.TargetDirectory, out _))
        {
            _console.WriteLine(Resources.UserInterface.Path_TargetNotFound);
            return;
        }

        _backupService.RunJob(job);
        _console.WriteLine(Resources.UserInterface.Launch_Done);
    }
}
