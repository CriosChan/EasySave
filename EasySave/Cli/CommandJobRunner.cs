using EasySave.Data.Configuration;
using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.State;
using EasySave.Views.Resources;

namespace EasySave.Cli;

/// <summary>
///     Executes backup jobs in parallel via command line.
/// </summary>
internal sealed class CommandJobRunner
{
    private readonly IBackupExecutionEngine _backupExecutionEngine = new BackupExecutionEngine();
    private readonly ParallelJobOrchestrator _orchestrator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandJobRunner" /> class.
    /// </summary>
    public CommandJobRunner()
    {
        _orchestrator = new ParallelJobOrchestrator(_backupExecutionEngine);
    }

    /// <summary>
    ///     Executes a list of job IDs in parallel.
    /// </summary>
    /// <param name="ids">IDs to execute.</param>
    /// <returns>Exit code.</returns>
    public int RunJobs(IEnumerable<int> ids)
    {
        var jobs = new JobService().GetAll().OrderBy(j => j.Id).ToList();
        if (jobs.Count == 0)
        {
            Console.WriteLine(UserInterface.Terminal_log_NoJobConfigured);
            return 1;
        }

        // Ensure the state file contains all configured jobs before running.
        StateFileSingleton.Instance.Initialize(ApplicationConfiguration.Load().LogPath);

        var jobsToRun = new List<BackupJob>();
        foreach (var id in ids)
        {
            var job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                Console.WriteLine(UserInterface.Terminal_log_JobIdNotFound, id);
                continue;
            }

            Console.WriteLine(UserInterface.Launch_RunningOne, job.Id, job.Name);
            jobsToRun.Add(job);
        }

        if (jobsToRun.Count == 0)
            return 1;

        // Execute jobs in parallel
        var result = _orchestrator.ExecuteAllAsync(jobsToRun).GetAwaiter().GetResult();

        if (result.WasStoppedByBusinessSoftware)
        {
            Console.WriteLine("Execution stopped: business software detected");
            return 0;
        }

        if (result.FailedCount > 0)
        {
            Console.WriteLine("Execution finished: {0} completed, {1} failed", result.CompletedCount, result.FailedCount);
            return 1;
        }

        Console.WriteLine("All jobs completed successfully ({0} jobs)", result.CompletedCount);
        return 0;
    }
}
