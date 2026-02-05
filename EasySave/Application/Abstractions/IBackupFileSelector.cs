using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat de selection des fichiers a copier pour un job.
/// </summary>
public interface IBackupFileSelector
{
    /// <summary>
    /// Retourne la liste des fichiers a copier pour un job donne.
    /// </summary>
    /// <param name="job">Job de sauvegarde.</param>
    /// <param name="sourceDir">Dossier source normalise.</param>
    /// <param name="targetDir">Dossier cible normalise.</param>
    /// <returns>Liste des fichiers a copier.</returns>
    List<string> GetFilesToCopy(BackupJob job, string sourceDir, string targetDir);
}
