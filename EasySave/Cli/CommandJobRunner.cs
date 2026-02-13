using EasySave.Data.Configuration;
using EasySave.Models.Backup;
using EasySave.Models.State;
using EasySave.Presentation.Resources;

namespace EasySave.Cli;

/// <summary>
///     Executes a sequence of backup jobs specified by their IDs.
/// </summary>
internal sealed class CommandJobRunner
{
    /// <summary>
    ///     Executes a list of job IDs in CLI mode.
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
        StateFileSingleton.Instance.Initialize(ApplicationConfiguration.Load().LogPath, jobs);

        foreach (var id in ids)
        {
            var job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                Console.WriteLine(UserInterface.Terminal_log_JobIdNotFound, id);
                continue;
            }

            Console.WriteLine(UserInterface.Launch_RunningOne, job.Id, job.Name);
            job.StartBackup();
        }

        return 0;
    }
}