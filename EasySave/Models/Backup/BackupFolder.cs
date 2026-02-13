using EasySave.Core.Models;
using EasySave.Models.Logger;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup;

public class BackupFolder
{
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
        // Enumerate directories in the source path
        var folders = Directory.EnumerateDirectories(_sourcePath, "*", SearchOption.AllDirectories);

        foreach (var folder in folders)
        {
            var target = folder.Replace(_sourcePath, _targetPath);

            // Skip if the target directory already exists
            if (Directory.Exists(target))
                continue;

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
    }
}