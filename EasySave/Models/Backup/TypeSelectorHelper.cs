using EasySave.Core.Models;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

public static class TypeSelectorHelper
{
    /// <summary>
    ///     Retrieves the appropriate backup type selector based on the specified backup type.
    /// </summary>
    /// <param name="backupType">The type of backup to perform.</param>
    /// <param name="sourceDirectory">The directory from which to back up files.</param>
    /// <param name="targetDirectory">The directory where the backup files will be stored.</param>
    /// <param name="name">The name of the backup process.</param>
    /// <returns>An instance of IBackupTypeSelector for the specified backup type.</returns>
    public static IBackupTypeSelector GetSelector(BackupType backupType, string sourceDirectory, string targetDirectory,
        string name)
    {
        switch (backupType)
        {
            case BackupType.Complete:
                return new BackupTypeComplete(sourceDirectory, targetDirectory, name); // Complete backup
            case BackupType.Differential:
                return new BackupTypeDifferential(sourceDirectory, targetDirectory, name); // Differential backup
            default:
                return new BackupTypeComplete(sourceDirectory, targetDirectory, name); // Fallback to complete backup
        }
    }
}