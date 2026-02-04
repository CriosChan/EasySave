using EasySave.Models;

namespace EasySave.Services;

/// <summary>
/// Keeps the state file aligned with the current job configuration.
/// </summary>
internal sealed class StateFileSynchronizer
{
    private readonly JobRepository _repository;
    private readonly StateFileService _state;

    public StateFileSynchronizer(JobRepository repository, StateFileService state)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public void Refresh()
    {
        List<BackupJob> jobs = _repository.Load().OrderBy(j => j.Id).ToList();
        _state.Initialize(jobs);
    }
}