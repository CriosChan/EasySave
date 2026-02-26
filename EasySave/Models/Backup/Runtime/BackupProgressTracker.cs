using EasySave.Models.Backup.Abstractions;

namespace EasySave.Models.Backup.Runtime;

/// <summary>
///     Tracks progress metrics (file count, transferred size, percentage) for a single backup job.
/// </summary>
public sealed class BackupProgressTracker : IBackupProgressTracker
{
    /// <inheritdoc />
    public long TotalSize { get; set; }

    /// <inheritdoc />
    public long TransferredSize { get; set; }

    /// <inheritdoc />
    public double CurrentProgress { get; private set; }

    /// <inheritdoc />
    public int FilesCount { get; private set; }

    /// <inheritdoc />
    public int CurrentFileIndex { get; private set; }

    /// <inheritdoc />
    public event EventHandler? ProgressChanged;

    /// <inheritdoc />
    public event EventHandler? FilesCountEvent;

    /// <inheritdoc />
    public void SetCurrentProgress(double value)
    {
        CurrentProgress = value;
        ProgressChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SetFilesCount(int value)
    {
        FilesCount = value;
        FilesCountEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SetCurrentFileIndex(int value)
    {
        CurrentFileIndex = value;
    }
}

