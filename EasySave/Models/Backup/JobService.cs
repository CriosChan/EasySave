using EasySave.Models.Data.Persistence;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Manages backup jobs and enforces business rules.
/// </summary>
public sealed class JobService : IJobService
{
    // Repository to handle backup job data persistence
    private readonly JobRepository _repository = new();

    /// <summary>
    ///     Retrieves all backup jobs, ordered by their ID.
    /// </summary>
    /// <returns>A read-only list of BackupJob objects.</returns>
    public IReadOnlyList<BackupJob> GetAll()
    {
        return _repository.GetAll().OrderBy(j => j.Id).ToList();
    }

    /// <summary>
    ///     Adds a new backup job to the repository.
    /// </summary>
    /// <param name="job">The BackupJob object to add.</param>
    /// <returns>A tuple containing a boolean indicating success and an error message if applicable.</returns>
    public (bool ok, string error) AddJob(BackupJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        if (job.Id != 0)
            throw new InvalidOperationException("New jobs must not have an id assigned.");

        var jobs = _repository.GetAll().ToList();

        var id = GetNextFreeId(jobs);
        if (id == -1)
            return (false, "Error.NoFreeSlot");

        job.Id = id; // Assign a new ID to the job
        jobs.Add(job);
        _repository.SaveAll(jobs); // Persist the updated job list
        return (true, string.Empty);
    }

    /// <summary>
    ///     Removes an existing backup job by ID or name.
    /// </summary>
    /// <param name="idOrName">The ID or name of the job to remove.</param>
    /// <returns>True if the job was successfully removed; otherwise, false.</returns>
    public bool RemoveJob(string idOrName)
    {
        if (string.IsNullOrWhiteSpace(idOrName))
            return false;

        var jobs = _repository.GetAll().ToList();
        BackupJob? toRemove = null;

        // Try to find the job by ID or name
        if (int.TryParse(idOrName, out var id))
            toRemove = jobs.FirstOrDefault(j => j.Id == id);
        else
            toRemove = jobs.FirstOrDefault(j => string.Equals(j.Name, idOrName, StringComparison.OrdinalIgnoreCase));

        if (toRemove == null)
            return false; // Job not found

        jobs.Remove(toRemove); // Remove the job from the list
        _repository.SaveAll(jobs); // Persist the updated job list
        return true;
    }

    /// <summary>
    ///     Gets the next available ID for a new backup job.
    /// </summary>
    /// <param name="jobs">The current list of backup jobs.</param>
    /// <returns>The next free ID, or -1 if no free ID is available.</returns>
    private static int GetNextFreeId(IReadOnlyCollection<BackupJob> jobs)
    {
        var i = 0;
        while (true)
        {
            i++;
            if (jobs.All(j => j.Id != i))
                return i; // Return the first free ID
        }
    }
}
