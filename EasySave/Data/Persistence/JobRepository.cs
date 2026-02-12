using EasySave.Core.Contracts;
using EasySave.Core.Models;
using EasySave.Platform.IO;

namespace EasySave.Data.Persistence;

/// <summary>
///     Persists the list of backup jobs to a JSON file (jobs.json).
/// </summary>
public sealed class JobRepository : IJobRepository
{
    private readonly string _jobsPath;

    /// <summary>
    ///     Creates a repository that stores jobs under the given configuration directory.
    /// </summary>
    /// <param name="configDirectory">Absolute directory where jobs.json will be stored.</param>
    public JobRepository(string configDirectory)
    {
        _jobsPath = Path.Combine(configDirectory, "jobs.json");
    }

    /// <summary>
    ///     Loads jobs from disk.
    /// </summary>
    public IReadOnlyList<BackupJob> GetAll()
    {
        return JsonFile.ReadOrDefault(_jobsPath, new List<BackupJob>());
    }

    /// <summary>
    ///     Saves jobs to disk.
    /// </summary>
    /// <param name="jobs">Job list to write.</param>
    public void SaveAll(IEnumerable<BackupJob> jobs)
    {
        if (jobs == null)
            throw new ArgumentNullException(nameof(jobs));

        JsonFile.WriteAtomic(_jobsPath, jobs.OrderBy(j => j.Id).ToList());
    }
}
