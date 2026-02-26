namespace EasySave.Models.Backup.Interfaces;

public interface IFile
{
    /// <summary>
    ///     Gets the source file path that is to be backed up.
    /// </summary>
    public string SourceFile { get; }

    /// <summary>
    ///     Gets the target file path where the backup will be stored.
    /// </summary>
    public string TargetFile { get; }

    /// <summary>
    ///     Copies the file from the source to the target location.
    /// </summary>
    public void Copy();

    /// <summary>
    ///     Retrieves the size of the file in bytes.
    /// </summary>
    /// <returns>The size of the file in bytes.</returns>
    public long GetSize();
}