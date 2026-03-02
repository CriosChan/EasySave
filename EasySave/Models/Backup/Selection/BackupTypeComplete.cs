using EasySave.Data.Configuration;
using EasySave.Models.Backup.Abstractions;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup.Selection;

public class BackupTypeComplete : IBackupTypeSelector
{
    private static readonly EnumerationOptions RecursiveAccessibleEnumeration = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        ReturnSpecialDirectories = false
    };

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
        var filesToBackup = new List<IFile>();
        var extensionToCrypt = new HashSet<string>(ApplicationConfiguration.Load().ExtensionToCrypt,
            StringComparer.OrdinalIgnoreCase);
        IEnumerable<string> allFiles;

        try
        {
            allFiles = Directory.EnumerateFiles(_sourceDir, "*", RecursiveAccessibleEnumeration);
        }
        catch
        {
            return filesToBackup;
        }

        // Iterate through all files to create backup file objects
        foreach (var file in allFiles)
        {
            var relativePath = PathService.GetRelativePath(_sourceDir, file);
            var targetPath = Path.Combine(_targetDir, relativePath);
            if (extensionToCrypt.Contains(Path.GetExtension(file).TrimStart('.')))
                filesToBackup.Add(new CryptedFile(file, targetPath, _backupName));
            else
                filesToBackup.Add(new NormalFile(file, targetPath, _backupName)); // Create a NormalFile instance
        }

        return filesToBackup; // Return the list of files to be backed up
    }
}
