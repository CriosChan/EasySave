using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Infrastructure.IO;

namespace EasySave.Infrastructure.Persistence;

/// <summary>
/// Persists the list of backup jobs to a JSON file (jobs.json).
/// </summary>
/// <remarks>
/// Requirements for v1.x:
/// - At most 5 jobs are supported.
/// - Jobs are stored in a human-readable JSON file.
/// - Writes are performed atomically to reduce corruption risk.
/// </remarks>
public sealed class JobRepository : IJobRepository
{
    private const int MaxJobs = 5;
    private readonly string _jobsPath;

    /// <summary>
    /// Creates a repository that stores jobs under the given configuration directory.
    /// </summary>
    /// <param name="configDirectory">Absolute directory where jobs.json will be stored.</param>
    public JobRepository(string configDirectory)
    {
        _jobsPath = Path.Combine(configDirectory, "jobs.json");
    }

    /// <summary>
    /// Loads jobs from disk.
    /// </summary>
    public List<BackupJob> Load()
    {
        return JsonFile.ReadOrDefault(_jobsPath, new List<BackupJob>());
    }

    /// <summary>
    /// Saves jobs to disk.
    /// </summary>
    /// <param name="jobs">Job list to write.</param>
    public void Save(List<BackupJob> jobs)
    {
        JsonFile.WriteAtomic(_jobsPath, jobs.OrderBy(j => j.Id).ToList());
    }

    /// <summary>
    /// Adds a job and assigns an available id in range 1..5.
    /// </summary>
    /// <param name="jobs">Existing jobs loaded in memory.</param>
    /// <param name="job">Job to add (id will be assigned).</param>
    /// <returns>Success flag and an error message if any.</returns>
    public (bool ok, string error) AddJob(List<BackupJob> jobs, BackupJob job)
    {
        if (jobs.Count >= MaxJobs)
            return (false, "Error.MaxJobs");

        int id = GetNextFreeId(jobs);
        if (id == -1)
            return (false, "Error.NoFreeSlot");

        job.Id = id;
        jobs.Add(job);
        Save(jobs);
        return (true, string.Empty);
    }

    /// <summary>
    /// Removes a job by id or by name.
    /// </summary>
    /// <param name="jobs">Existing jobs loaded in memory.</param>
    /// <param name="idOrName">Id ("1") or job name.</param>
    /// <returns>True if a job was removed.</returns>
    public bool RemoveJob(List<BackupJob> jobs, string idOrName)
    {
        BackupJob? toRemove = null;
        if (int.TryParse(idOrName, out int id))
        {
            toRemove = jobs.FirstOrDefault(j => j.Id == id);
        }
        else
        {
            toRemove = jobs.FirstOrDefault(j => string.Equals(j.Name, idOrName, StringComparison.OrdinalIgnoreCase));
        }

        if (toRemove == null)
            return false;

        jobs.Remove(toRemove);
        Save(jobs);
        return true;
    }

    /// <summary>
    /// Finds the next available slot in 1..5.
    /// </summary>
    private static int GetNextFreeId(IReadOnlyCollection<BackupJob> jobs)
    {
        for (int i = 1; i <= MaxJobs; i++)
        {
            if (jobs.All(j => j.Id != i))
                return i;
        }

        return -1;
    }
}
