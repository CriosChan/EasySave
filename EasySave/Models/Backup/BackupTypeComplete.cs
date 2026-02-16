using EasySave.Data.Configuration;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

public class BackupTypeComplete : IBackupTypeSelector
{
    private readonly string _backupName; // Name of the backup
    private readonly string _sourceDir; // Source directory for backup files
    private readonly string _targetDir; // Target directory for backup files

    /// <summary>
    ///     Initializes a new instance of the BackupTypeComplete class.
    /// </summary>
    /// <param name="sourceDir">The directory to retrieve files from.</param>
    /// <param name="targetDir">The directory where backup files will be stored.</param>
    /// <param name="backupName">The name of the backup process.</param>
    public BackupTypeComplete(string sourceDir, string targetDir, string backupName)
    {
        _sourceDir = sourceDir;
        _targetDir = targetDir;
        _backupName = backupName;
    }

    /// <summary>
    ///     Retrieves the list of files to be backed up from the source directory.
    ///     Creates corresponding file objects for each file in the target directory.
    /// </summary>
    /// <returns>A list of files to back up.</returns>
    public List<IFile> GetFilesToBackup()
    {
        var allFiles = Directory.EnumerateFiles(_sourceDir, "*", SearchOption.AllDirectories);
        var allFilesList = allFiles.ToList(); // Convert the enumerable to a list
        var filesToBackup = new List<IFile>();

        // Iterate through all files to create backup file objects
        foreach (var file in allFilesList)
        {
            var targetPath = file.Replace(_sourceDir, _targetDir);
            if (ApplicationConfiguration.Load().ExtensionToCrypt.Contains(Path.GetExtension(file).TrimStart('.')))
                filesToBackup.Add(new CryptedFile(file, targetPath, _backupName));
            else
                filesToBackup.Add(new NormalFile(file, targetPath, _backupName)); // Create a NormalFile instance
        }

        return filesToBackup; // Return the list of files to be backed up
    }
}