namespace EasySave.Models.Backup.Interfaces;

public interface IBackupTypeSelector
{
    /// <summary>
    ///     Retrieves a list of files to be backed up.
    ///     Implementations should define the logic for selecting files based on specific criteria.
    /// </summary>
    /// <returns>A list of files to back up.</returns>
    public List<IFile> GetFilesToBackup();
}