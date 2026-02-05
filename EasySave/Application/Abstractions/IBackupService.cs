using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat pour l'execution des sauvegardes.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Execute une liste de jobs sequentiellement.
    /// </summary>
    /// <param name="jobs">Jobs a executer.</param>
    void RunJobsSequential(IEnumerable<BackupJob> jobs);

    /// <summary>
    /// Execute un job de sauvegarde.
    /// </summary>
    /// <param name="job">Job a executer.</param>
    void RunJob(BackupJob job);
}
