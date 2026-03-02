using EasySave.Core.Models;
using EasySave.Models.Logger;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup.Entities;

public class BackupFolder
{
    private static readonly EnumerationOptions RecursiveAccessibleEnumeration = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        ReturnSpecialDirectories = false
    };

    private readonly string _backupName; // Name of the current backup process
    private readonly string _sourcePath; // Source directory for backup
    private readonly string _targetPath; // Target directory for backup storage

    /// <summary>
    ///     Initializes a new instance of the BackupFolder class.
    /// </summary>
    /// <param name="sourcePath">The path of the folder to backup.</param>
    /// <param name="targetPath">The path where the backup will be stored.</param>
    /// <param name="backupName">The name of the backup.</param>
    public BackupFolder(string sourcePath, string targetPath, string backupName)
    {
        _sourcePath = sourcePath;
        _targetPath = targetPath;
        _backupName = backupName;
    }

    /// <summary>
    ///     Mirrors the source folder structure in the target folder.
    ///     Creates directories in the target folder that match the source folder.
    ///     Logs the creation of each directory.
    /// </summary>
    public void MirrorFolder()
    {
        var logger = new ConfigurableLogWriter<LogEntry>();
        IEnumerable<string> folders;
        try
        {
            folders = Directory.EnumerateDirectories(_sourcePath, "*", RecursiveAccessibleEnumeration);
        }
        catch (Exception ex)
        {
            logger.Log(new LogEntry
            {
                BackupName = _backupName,
                SourcePath = PathService.ToFullUncLikePath(_sourcePath),
                TargetPath = PathService.ToFullUncLikePath(_targetPath),
                FileSizeBytes = 0,
                TransferTimeMs = 0,
                ErrorMessage = ex.Message
            });
            return;
        }

        foreach (var folder in folders)
        {
            var relativePath = PathService.GetRelativePath(_sourcePath, folder);
            var target = Path.Combine(_targetPath, relativePath);

            // Skip if the target directory already exists
            if (Directory.Exists(target))
                continue;

            try
            {
                // Create the target directory
                Directory.CreateDirectory(target);
                logger.Log(new LogEntry
                {
                    BackupName = _backupName,
                    SourcePath = PathService.ToFullUncLikePath(folder),
                    TargetPath = PathService.ToFullUncLikePath(target),
                    FileSizeBytes = 0,
                    TransferTimeMs = 0
                });
            }
            catch (Exception ex)
            {
                logger.Log(new LogEntry
                {
                    BackupName = _backupName,
                    SourcePath = PathService.ToFullUncLikePath(folder),
                    TargetPath = PathService.ToFullUncLikePath(target),
                    FileSizeBytes = 0,
                    TransferTimeMs = 0,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
