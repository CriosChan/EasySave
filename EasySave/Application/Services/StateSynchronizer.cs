using EasySave.Application.Abstractions;

namespace EasySave.Application.Services;

/// <summary>
///     Synchronizes execution state with the current configuration.
/// </summary>
public sealed class StateSynchronizer : IStateSynchronizer
{
    private readonly IJobRepository _repository;
    private readonly IStateService _state;

    public StateSynchronizer(IJobRepository repository, IStateService state)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>
    ///     Reloads jobs from the repository and reinitializes state.
    /// </summary>
    public void Refresh()
    {
        var jobs = _repository.GetAll().OrderBy(j => j.Id).ToList();
        _state.Initialize(jobs);
    }
}
