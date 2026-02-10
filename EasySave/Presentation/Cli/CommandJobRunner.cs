using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Presentation.Resources;

namespace EasySave.Presentation.Cli;

/// <summary>
///     Executes a sequence of backup jobs specified by their IDs.
/// </summary>
internal sealed class CommandJobRunner
{
    private readonly IBackupService _backupService;
    private readonly IPathService _paths;
    private readonly IJobRepository _repository;
    private readonly IStateService _stateService;

    /// <summary>
    ///     Builds the CLI runner with its dependencies.
    /// </summary>
    /// <param name="repository">Job repository.</param>
    /// <param name="backupService">Backup service.</param>
    /// <param name="stateService">State service.</param>
    /// <param name="paths">Path service.</param>
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
    ///     Executes a list of job IDs in CLI mode.
    /// </summary>
    /// <param name="ids">IDs to execute.</param>
    /// <returns>Exit code.</returns>
    public int RunJobs(IEnumerable<int> ids)
    {
        var jobs = _repository.Load().OrderBy(j => j.Id).ToList();
        if (jobs.Count == 0)
        {
            Console.WriteLine(UserInterface.Terminal_log_NoJobConfigured);
            return 1;
        }

        // Ensure the state file contains all configured jobs before running.
        _stateService.Initialize(jobs);

        foreach (var id in ids)
        {
            var job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                Console.WriteLine(UserInterface.Terminal_log_JobIdNotFound, id);
                continue;
            }

            if (!IsJobRunnable(job, out var reason))
            {
                Console.WriteLine(reason);
                continue;
            }

            Console.WriteLine(UserInterface.Launch_RunningOne, job.Id, job.Name);
            _backupService.RunJob(job);
        }

        return 0;
    }

    /// <summary>
    ///     Verifies a job can run (valid source/target directories).
    /// </summary>
    /// <param name="job">Target job.</param>
    /// <param name="message">Error message if not runnable.</param>
    /// <returns>True if the job is runnable.</returns>
    private bool IsJobRunnable(BackupJob job, out string? message)
    {
        // Validate directories before reporting a run.
        if (!_paths.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
        {
            message = string.Format(UserInterface.Terminal_log_JobSourceNotFound, job.Id);
            return false;
        }

        if (!_paths.TryNormalizeExistingDirectory(job.TargetDirectory, out _))
        {
            message = string.Format(UserInterface.Terminal_log_JobTargetNotFound, job.Id);
            return false;
        }

        message = null;
        return true;
    }
}