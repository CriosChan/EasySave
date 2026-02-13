using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

public class BackupTypeDifferential : IBackupTypeSelector
{
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
        var allFiles = Directory.EnumerateFiles(_sourceDir, "*", SearchOption.AllDirectories);
        var allFilesList = allFiles.ToList(); // Convert the enumerable to a list
        var filesToBackup = new List<IFile>();

        // Iterate through all files to determine which need to be backed up
        foreach (var file in allFilesList)
        {
            var targetPath = file.Replace(_sourceDir, _targetDir);
            // Add the file if it does not exist in the target directory
            if (!File.Exists(targetPath))
            {
                filesToBackup.Add(new NormalFile(file, targetPath, _backupName));
                continue;
            }

            var src = new FileInfo(file); // Source file info
            var dst = new FileInfo(targetPath); // Target file info

            // Check if the files are different based on size or last write time
            var isDifferent = src.Length != dst.Length || src.LastWriteTimeUtc > dst.LastWriteTimeUtc;
            if (isDifferent)
                filesToBackup.Add(new NormalFile(file, targetPath, _backupName)); // Add file if different
        }

        return filesToBackup; // Return the list of files to be backed up based on differential criteria
    }
}