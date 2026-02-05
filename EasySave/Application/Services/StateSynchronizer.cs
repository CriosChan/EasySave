using EasySave.Application.Abstractions;
using EasySave.Domain.Models;

namespace EasySave.Application.Services;

/// <summary>
/// Synchronise l'etat d'execution avec la configuration courante.
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
    /// Recharge les jobs depuis le depot et reinitialise l'etat.
    /// </summary>
    public void Refresh()
    {
        List<BackupJob> jobs = _repository.Load().OrderBy(j => j.Id).ToList();
        _state.Initialize(jobs);
    }
}
