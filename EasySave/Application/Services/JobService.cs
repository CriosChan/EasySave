using EasySave.Application.Abstractions;
using EasySave.Domain.Models;

namespace EasySave.Application.Services;

/// <summary>
///     Manages backup jobs and enforces business rules.
/// </summary>
public sealed class JobService : IJobService
{
    private const int MaxJobs = 5;
    private readonly IJobRepository _repository;

    public JobService(IJobRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IReadOnlyList<BackupJob> GetAll()
    {
        return _repository.GetAll().OrderBy(j => j.Id).ToList();
    }

    public (bool ok, string error) AddJob(BackupJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        if (job.Id != 0)
            throw new InvalidOperationException("New jobs must not have an id assigned.");

        var jobs = _repository.GetAll().ToList();

        if (jobs.Count >= MaxJobs)
            return (false, "Error.MaxJobs");

        var id = GetNextFreeId(jobs);
        if (id == -1)
            return (false, "Error.NoFreeSlot");

        job.AssignId(id);
        jobs.Add(job);
        _repository.SaveAll(jobs);
        return (true, string.Empty);
    }

    public bool RemoveJob(string idOrName)
    {
        if (string.IsNullOrWhiteSpace(idOrName))
            return false;

        var jobs = _repository.GetAll().ToList();
        BackupJob? toRemove = null;

        if (int.TryParse(idOrName, out var id))
            toRemove = jobs.FirstOrDefault(j => j.Id == id);
        else
            toRemove = jobs.FirstOrDefault(j => string.Equals(j.Name, idOrName, StringComparison.OrdinalIgnoreCase));

        if (toRemove == null)
            return false;

        jobs.Remove(toRemove);
        _repository.SaveAll(jobs);
        return true;
    }

    private static int GetNextFreeId(IReadOnlyCollection<BackupJob> jobs)
    {
        for (var i = 1; i <= MaxJobs; i++)
            if (jobs.All(j => j.Id != i))
                return i;

        return -1;
    }
}
