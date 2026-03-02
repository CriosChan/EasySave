using EasySave.Data.Configuration;
using EasySave.Models.Backup.Abstractions;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup.Selection;

public class BackupTypeDifferential : IBackupTypeSelector
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
    ///     Initializes a new instance of the BackupTypeDifferential class.
    /// </summary>
    /// <param name="sourceDir">The directory to check for files to back up.</param>
    /// <param name="targetDir">The directory where backup files will be stored.</param>
    /// <param name="backupName">The name of the backup process.</param>
    public BackupTypeDifferential(string sourceDir, string targetDir, string backupName)
    {
        _sourceDir = sourceDir;
        _targetDir = targetDir;
        _backupName = backupName;
    }

    /// <summary>
    ///     Retrieves the files to be backed up using differential backup logic.
    ///     Only files that are new or have changed since the last backup will be selected.
    /// </summary>
    /// <returns>A list of files to back up based on the differential criteria.</returns>
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

        // Iterate through all files to determine which need to be backed up
        foreach (var file in allFiles)
        {
            var relativePath = PathService.GetRelativePath(_sourceDir, file);
            var targetPath = Path.Combine(_targetDir, relativePath);

            try
            {
                // Add the file if it does not exist in the target directory
                if (!File.Exists(targetPath))
                {
                    AddBackupFile(filesToBackup, extensionToCrypt, file, targetPath);
                    continue;
                }

                var src = new FileInfo(file); // Source file info
                var dst = new FileInfo(targetPath); // Target file info

                // Check if the files are different based on size or last write time
                var isDifferent = src.Length != dst.Length || src.LastWriteTimeUtc > dst.LastWriteTimeUtc;
                if (isDifferent)
                    AddBackupFile(filesToBackup, extensionToCrypt, file, targetPath);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip inaccessible files instead of failing the whole backup job.
            }
            catch (IOException)
            {
                // Skip transient I/O failures for a single file.
            }
        }

        return filesToBackup; // Return the list of files to be backed up based on differential criteria
    }

    private void AddBackupFile(List<IFile> filesToBackup, ISet<string> extensionToCrypt, string sourcePath, string targetPath)
    {
        if (extensionToCrypt.Contains(Path.GetExtension(sourcePath).TrimStart('.')))
            filesToBackup.Add(new CryptedFile(sourcePath, targetPath, _backupName));
        else
            filesToBackup.Add(new NormalFile(sourcePath, targetPath, _backupName));
    }
}
