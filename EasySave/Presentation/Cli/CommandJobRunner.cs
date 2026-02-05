using EasySave.Application.Abstractions;
using EasySave.Domain.Models;

namespace EasySave.Presentation.Cli;

/// <summary>
/// Executes a sequence of backup jobs specified by their IDs.
/// </summary>
internal sealed class CommandJobRunner
{
    private readonly IJobRepository _repository;
    private readonly IBackupService _backupService;
    private readonly IStateService _stateService;
    private readonly IPathService _paths;

    /// <summary>
    /// Construit l'executant CLI avec ses dependances.
    /// </summary>
    /// <param name="repository">Depot des jobs.</param>
    /// <param name="backupService">Service de sauvegarde.</param>
    /// <param name="stateService">Service d'etat.</param>
    /// <param name="paths">Service de chemins.</param>
    public CommandJobRunner(
        IJobRepository repository,
        IBackupService backupService,
        IStateService stateService,
        IPathService paths)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    /// <summary>
    /// Execute une liste d'IDs de jobs en mode CLI.
    /// </summary>
    /// <param name="ids">IDs a executer.</param>
    /// <returns>Code de sortie.</returns>
    public int RunJobs(IEnumerable<int> ids)
    {
        List<BackupJob> jobs = _repository.Load().OrderBy(j => j.Id).ToList();
        if (jobs.Count == 0)
        {
            Console.WriteLine("No backup job configured.");
            return 1;
        }

        // Ensure the state file contains all configured jobs before running.
        _stateService.Initialize(jobs);

        foreach (int id in ids)
        {
            BackupJob? job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                Console.WriteLine($"Job {id} not found.");
                continue;
            }

            if (!IsJobRunnable(job, out string? reason))
            {
                Console.WriteLine(reason);
                continue;
            }

            Console.WriteLine($"Running job {job.Id} - {job.Name}...");
            _backupService.RunJob(job);
        }

        return 0;
    }

    /// <summary>
    /// Verifie qu'un job peut etre execute (sources/targets valides).
    /// </summary>
    /// <param name="job">Job cible.</param>
    /// <param name="message">Message d'erreur si non executable.</param>
    /// <returns>Vrai si le job est executable.</returns>
    private bool IsJobRunnable(BackupJob job, out string? message)
    {
        // Validate directories before reporting a run.
        if (!_paths.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
        {
            message = $"Job {job.Id} skipped: source directory not found.";
            return false;
        }

        if (!_paths.TryNormalizeExistingDirectory(job.TargetDirectory, out _))
        {
            message = $"Job {job.Id} skipped: target directory not found.";
            return false;
        }

        message = null;
        return true;
    }
}
