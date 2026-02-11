using EasySave.Application.Abstractions;
using EasySave.Domain.Models;

namespace EasySave.Application.Services;

/// <summary>
///     Prepares the target directory tree and logs directory creations.
/// </summary>
public sealed class BackupDirectoryPreparer
{
    private readonly ILogWriter<LogEntry> _logger;
    private readonly IPathService _paths;

    public BackupDirectoryPreparer(ILogWriter<LogEntry> logger, IPathService paths)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    /// <summary>
    ///     Creates all missing target folders based on the source tree.
    /// </summary>
    /// <param name="job">Backup job.</param>
    /// <param name="sourceDir">Normalized source directory.</param>
    /// <param name="targetDir">Normalized target directory.</param>
    public void EnsureTargetDirectories(BackupJob job, string sourceDir, string targetDir)
    {
        try
        {
            foreach (var srcDir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relative = _paths.GetRelativePath(sourceDir, srcDir);
                var dstDir = Path.Combine(targetDir, relative);

                if (Directory.Exists(dstDir))
                    continue;

                Directory.CreateDirectory(dstDir);
                _logger.Log(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourcePath = _paths.ToFullUncLikePath(srcDir),
                    TargetPath = _paths.ToFullUncLikePath(dstDir),
                    FileSizeBytes = 0,
                    TransferTimeMs = 0
                });
            }
        }
        catch
        {
            // Directory enumeration/creation errors will be reflected during file transfers.
        }
    }

    /// <summary>
    ///     Ensures the parent directory of the target file exists.
    /// </summary>
    /// <param name="job">Backup job.</param>
    /// <param name="sourceFile">Source file.</param>
    /// <param name="targetFile">Target file.</param>
    public void EnsureTargetDirectoryForFile(BackupJob job, string sourceFile, string targetFile)
    {
        var targetFileDir = Path.GetDirectoryName(targetFile);
        if (string.IsNullOrWhiteSpace(targetFileDir) || Directory.Exists(targetFileDir))
            return;

        Directory.CreateDirectory(targetFileDir);
        _logger.Log(new LogEntry
        {
            Timestamp = DateTime.Now,
            BackupName = job.Name,
            SourcePath = _paths.ToFullUncLikePath(Path.GetDirectoryName(sourceFile) ?? job.SourceDirectory),
            TargetPath = _paths.ToFullUncLikePath(targetFileDir),
            FileSizeBytes = 0,
            TransferTimeMs = 0
        });
    }
}
