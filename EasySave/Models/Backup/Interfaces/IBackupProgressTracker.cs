namespace EasySave.Models.Backup.Interfaces;

/// <summary>
///     Defines the progress-tracking contract for a backup job.
/// </summary>
public interface IBackupProgressTracker
{
    /// <summary> Gets or sets the total size of files to back up (bytes). </summary>
    long TotalSize { get; set; }

    /// <summary> Gets or sets the size already transferred (bytes). </summary>
    long TransferredSize { get; set; }

    /// <summary> Gets the current progress percentage. </summary>
    double CurrentProgress { get; }

    /// <summary> Gets the total file count. </summary>
    int FilesCount { get; }

    /// <summary> Gets the zero-based index of the file being processed. </summary>
    int CurrentFileIndex { get; }

    /// <summary> Sets the current progress percentage and raises <see cref="ProgressChanged" />. </summary>
    void SetCurrentProgress(double value);

    /// <summary> Sets the total file count and raises <see cref="FilesCountEvent" />. </summary>
    void SetFilesCount(int value);

    /// <summary> Sets the current file index. </summary>
    void SetCurrentFileIndex(int value);

    /// <summary> Raised whenever <see cref="CurrentProgress" /> changes. </summary>
    event EventHandler? ProgressChanged;

    /// <summary> Raised whenever <see cref="FilesCount" /> changes. </summary>
    event EventHandler? FilesCountEvent;
}

