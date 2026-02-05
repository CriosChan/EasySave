using EasySave.Models;
using EasySave.Services;
using EasySave.Utils;

namespace EasySave.Controller.CommandLine;

/// <summary>
/// Executes a sequence of backup jobs specified by their IDs.
/// </summary>
internal sealed class CommandJobRunner
{
    private readonly JobRepository _repository;
    private readonly BackupService _backupService;
    private readonly StateFileService _stateService;

    public CommandJobRunner(JobRepository repository, BackupService backupService, StateFileService stateService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
    }

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

    private static bool IsJobRunnable(BackupJob job, out string? message)
    {
        // Validate directories before reporting a run.
        if (!PathTools.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
        {
            message = $"Job {job.Id} skipped: source directory not found.";
            return false;
        }

        if (!PathTools.TryNormalizeExistingDirectory(job.TargetDirectory, out _))
        {
            message = $"Job {job.Id} skipped: target directory not found.";
            return false;
        }

        message = null;
        return true;
    }
}
