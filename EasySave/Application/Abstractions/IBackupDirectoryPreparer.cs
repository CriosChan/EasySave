using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
/// Contract for preparing target directories for a backup.
/// </summary>
public interface IBackupDirectoryPreparer
{
    /// <summary>
    /// Creates the full target directory tree for a job.
    /// </summary>
    /// <param name="job">Backup job.</param>
    /// <param name="sourceDir">Normalized source directory.</param>
    /// <param name="targetDir">Normalized target directory.</param>
    void EnsureTargetDirectories(BackupJob job, string sourceDir, string targetDir);

    /// <summary>
    /// Creates the parent folder for a target file if needed.
    /// </summary>
    /// <param name="job">Backup job.</param>
    /// <param name="sourceFile">Source file.</param>
    /// <param name="targetFile">Target file.</param>
    void EnsureTargetDirectoryForFile(BackupJob job, string sourceFile, string targetFile);
}
