using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
///     Contract for selecting files to copy for a job.
/// </summary>
public interface IBackupFileSelector
{
    /// <summary>
    ///     Returns the list of files to copy for a given job.
    /// </summary>
    /// <param name="job">Backup job.</param>
    /// <param name="sourceDir">Normalized source directory.</param>
    /// <param name="targetDir">Normalized target directory.</param>
    /// <returns>List of files to copy.</returns>
    List<string> GetFilesToCopy(BackupJob job, string sourceDir, string targetDir);
}