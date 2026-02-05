using EasySave.Application.Abstractions;
using EasySave.Domain.Models;

namespace EasySave.Application.Services;

/// <summary>
/// Selects files eligible for copying based on the backup type.
/// </summary>
public sealed class BackupFileSelector : IBackupFileSelector
{
    private readonly IPathService _paths;

    public BackupFileSelector(IPathService paths)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    /// <summary>
    /// Determines the list of files to copy for a job.
    /// </summary>
    /// <param name="job">Backup job.</param>
    /// <param name="sourceDir">Normalized source directory.</param>
    /// <param name="targetDir">Normalized target directory.</param>
    /// <returns>List of files to copy.</returns>
    public List<string> GetFilesToCopy(BackupJob job, string sourceDir, string targetDir)
    {
        IEnumerable<string> allFiles = Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories);

        if (job.Type == BackupType.Complete)
            return allFiles.ToList();

        // Differential: copy files that do not exist in target, or that are older/different.
        List<string> differential = new();
        foreach (string sourceFile in allFiles)
        {
            string rel = _paths.GetRelativePath(sourceDir, sourceFile);
            string targetFile = Path.Combine(targetDir, rel);
            if (!File.Exists(targetFile))
            {
                differential.Add(sourceFile);
                continue;
            }

            FileInfo src = new FileInfo(sourceFile);
            FileInfo dst = new FileInfo(targetFile);

            bool isDifferent = src.Length != dst.Length || src.LastWriteTimeUtc > dst.LastWriteTimeUtc;
            if (isDifferent)
                differential.Add(sourceFile);
        }

        return differential;
    }
}
