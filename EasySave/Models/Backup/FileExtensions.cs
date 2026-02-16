using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

public static class FileExtensions
{
    /// <summary>
    ///     Calculates the total size of all files in the list.
    /// </summary>
    /// <param name="files">The list of files to calculate the total size for.</param>
    /// <returns>The total size in bytes of all files.</returns>
    public static long GetAllSize(this List<IFile> files)
    {
        long size = 0; // Initialize total size
        // Iterate through the list of files and accumulate their sizes
        foreach (var file in files)
            size += file.GetSize();

        return size; // Return the calculated total size
    }
}