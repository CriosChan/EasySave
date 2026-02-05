using EasySave.Application.Abstractions;
using EasySave.Domain.Models;

namespace EasySave.Application.Services;

/// <summary>
/// Prepare l'arborescence cible et journalise les creations de dossiers.
/// </summary>
public sealed class BackupDirectoryPreparer : IBackupDirectoryPreparer
{
    private readonly ILogWriter<LogEntry> _logger;
    private readonly IPathService _paths;

    public BackupDirectoryPreparer(ILogWriter<LogEntry> logger, IPathService paths)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    /// <summary>
    /// Cree tous les dossiers manquants dans la cible a partir de la source.
    /// </summary>
    /// <param name="job">Job de sauvegarde.</param>
    /// <param name="sourceDir">Dossier source normalise.</param>
    /// <param name="targetDir">Dossier cible normalise.</param>
    public void EnsureTargetDirectories(BackupJob job, string sourceDir, string targetDir)
    {
        try
        {
            foreach (string srcDir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relative = _paths.GetRelativePath(sourceDir, srcDir);
                string dstDir = Path.Combine(targetDir, relative);

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
                    TransferTimeMs = 0,
                });
            }
        }
        catch
        {
            // Directory enumeration/creation errors will be reflected during file transfers.
        }
    }

    /// <summary>
    /// S'assure que le dossier parent du fichier cible existe.
    /// </summary>
    /// <param name="job">Job de sauvegarde.</param>
    /// <param name="sourceFile">Fichier source.</param>
    /// <param name="targetFile">Fichier cible.</param>
    public void EnsureTargetDirectoryForFile(BackupJob job, string sourceFile, string targetFile)
    {
        string? targetFileDir = Path.GetDirectoryName(targetFile);
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
            TransferTimeMs = 0,
        });
    }
}
