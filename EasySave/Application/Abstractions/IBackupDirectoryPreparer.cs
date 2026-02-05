using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat de preparation des dossiers cibles pour une sauvegarde.
/// </summary>
public interface IBackupDirectoryPreparer
{
    /// <summary>
    /// Cree l'arborescence cible complete pour un job.
    /// </summary>
    /// <param name="job">Job de sauvegarde.</param>
    /// <param name="sourceDir">Dossier source normalise.</param>
    /// <param name="targetDir">Dossier cible normalise.</param>
    void EnsureTargetDirectories(BackupJob job, string sourceDir, string targetDir);

    /// <summary>
    /// Cree le dossier parent d'un fichier cible si necessaire.
    /// </summary>
    /// <param name="job">Job de sauvegarde.</param>
    /// <param name="sourceFile">Fichier source.</param>
    /// <param name="targetFile">Fichier cible.</param>
    void EnsureTargetDirectoryForFile(BackupJob job, string sourceFile, string targetFile);
}
