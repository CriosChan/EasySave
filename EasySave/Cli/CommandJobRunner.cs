using EasySave.Core.Contracts;
using EasySave.Core.Validation;
using EasySave.Core.Models;
using EasySave.Views.Resources;

namespace EasySave.Cli;

/// <summary>
///     Executes a sequence of backup jobs specified by their IDs.
/// </summary>
internal sealed class CommandJobRunner
{
    private readonly IBackupService _backupService;
    private readonly IJobService _jobService;
    private readonly IJobValidator _validator;
    private readonly IStateService _stateService;

    /// <summary>
    ///     Builds the CLI runner with its dependencies.
    /// </summary>
    /// <param name="jobService">Job service.</param>
    /// <param name="backupService">Backup service.</param>
    /// <param name="stateService">State service.</param>
    /// <param name="validator">Job validator.</param>
    public CommandJobRunner(
        IJobService jobService,
        IBackupService backupService,
        IStateService stateService,
        IJobValidator validator)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    ///     Executes a list of job IDs in CLI mode.
    /// </summary>
    /// <param name="ids">IDs to execute.</param>
    /// <returns>Exit code.</returns>
    public int RunJobs(IEnumerable<int> ids)
    {
        var jobs = _jobService.GetAll().OrderBy(j => j.Id).ToList();
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
        var validation = _validator.Validate(job);
        if (validation.IsValid)
        {
            message = null;
            return true;
        }

        message = validation.Error switch
        {
            JobValidationError.SourceMissing => string.Format(UserInterface.Terminal_log_JobSourceNotFound, job.Id),
            JobValidationError.TargetMissing => string.Format(UserInterface.Terminal_log_JobTargetNotFound, job.Id),
            _ => string.Format(UserInterface.Terminal_log_JobSourceNotFound, job.Id)
        };

        return false;
    }
}
