using EasySave.Models;
using EasySave.Services;
using EasySave.Utils;
using EasySave.View.Ressources;

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
            Console.WriteLine(UserInterface.Terminal_log_NoJobConfigured);
            return 1;
        }

        // Ensure the state file contains all configured jobs before running.
        _stateService.Initialize(jobs);

        foreach (int id in ids)
        {
            BackupJob? job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                Console.WriteLine(UserInterface.Terminal_log_JobIdNotFound, id);
                continue;
            }

            if (!IsJobRunnable(job, out string? reason))
            {
                Console.WriteLine(reason);
                continue;
            }

            Console.WriteLine(UserInterface.Launch_RunningOne, job.Id, job.Name);
            _backupService.RunJob(job);
        }

        return 0;
    }

    private static bool IsJobRunnable(BackupJob job, out string? message)
    {
        // Validate directories before reporting a run.
        if (!PathTools.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
        {
            message = string.Format(UserInterface.Terminal_log_JobSourceNotFound, job.Id);
            return false;
        }

        if (!PathTools.TryNormalizeExistingDirectory(job.TargetDirectory, out _))
        {
            message = string.Format(UserInterface.Terminal_log_JobTargetNotFound, job.Id);
            return false;
        }

        message = null;
        return true;
    }
}
