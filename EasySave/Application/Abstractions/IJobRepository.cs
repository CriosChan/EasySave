using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat de persistence des jobs de sauvegarde.
/// </summary>
public interface IJobRepository
{
    /// <summary>
    /// Charge la liste des jobs.
    /// </summary>
    /// <returns>Liste de jobs.</returns>
    List<BackupJob> Load();

    /// <summary>
    /// Sauvegarde la liste des jobs.
    /// </summary>
    /// <param name="jobs">Jobs a persister.</param>
    void Save(List<BackupJob> jobs);

    /// <summary>
    /// Ajoute un job a la liste et le persiste.
    /// </summary>
    /// <param name="jobs">Liste courante en memoire.</param>
    /// <param name="job">Job a ajouter.</param>
    /// <returns>Resultat et code d'erreur eventuel.</returns>
    (bool ok, string error) AddJob(List<BackupJob> jobs, BackupJob job);

    /// <summary>
    /// Supprime un job par identifiant ou par nom.
    /// </summary>
    /// <param name="jobs">Liste courante en memoire.</param>
    /// <param name="idOrName">Identifiant ou nom du job.</param>
    /// <returns>Vrai si un job a ete supprime.</returns>
    bool RemoveJob(List<BackupJob> jobs, string idOrName);
}
