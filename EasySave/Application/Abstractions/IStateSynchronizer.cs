namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat de synchronisation entre configuration des jobs et etat d'execution.
/// </summary>
public interface IStateSynchronizer
{
    /// <summary>
    /// Recharge les jobs et reinitialise l'etat.
    /// </summary>
    void Refresh();
}
