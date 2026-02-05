using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat de gestion de l'etat d'execution des sauvegardes.
/// </summary>
public interface IStateService
{
    /// <summary>
    /// Initialise l'etat pour un ensemble de jobs.
    /// </summary>
    /// <param name="jobs">Jobs a initialiser.</param>
    void Initialize(IEnumerable<BackupJob> jobs);

    /// <summary>
    /// Met a jour l'etat d'un job.
    /// </summary>
    /// <param name="updated">Etat mis a jour.</param>
    void Update(BackupJobState updated);

    /// <summary>
    /// Recupere l'etat d'un job ou le cree s'il n'existe pas.
    /// </summary>
    /// <param name="job">Job cible.</param>
    /// <returns>Etat courant.</returns>
    BackupJobState GetOrCreate(BackupJob job);
}
