namespace EasySave.Models.State;

/// <summary>
///     Describes the execution state of a backup job.
///     This class holds information related to the status and progress of a backup operation.
/// </summary>
public sealed class BackupJobState
{
    /// <summary>
    ///     Gets or sets the unique identifier for the backup job.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the backup job.
    /// </summary>
    public string BackupName { get; set; } = ""; // Name of the backup job

    /// <summary>
    ///     Gets or sets the timestamp of the last action performed in the backup job.
    /// </summary>
    public DateTime LastActionTimestamp { get; set; }

    /// <summary>
    ///     Gets or sets the current state of the backup job.
    /// </summary>
    public JobRunState State { get; set; } = JobRunState.Inactive; // Default state is inactive

    /// <summary>
    ///     Gets or sets the total number of files to be backed up.
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    ///     Gets or sets the total size of files to be backed up in bytes.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    ///     Gets or sets the progress of the backup job as a percentage (0..100).
    /// </summary>
    public double ProgressPercent { get; set; }

    /// <summary>
    ///     Gets or sets the number of files remaining to be processed in the backup job.
    /// </summary>
    public int RemainingFiles { get; set; }

    /// <summary>
    ///     Gets or sets the total remaining size in bytes of files yet to be backed up.
    /// </summary>
    public long RemainingSizeBytes { get; set; }

    /// <summary>
    ///     Gets or sets the source path of the file currently being processed.
    /// </summary>
    public string? CurrentSourcePath { get; set; } // Nullable for current file source path

    /// <summary>
    ///     Gets or sets the target path where the currently processed file will be stored.
    /// </summary>
    public string? CurrentTargetPath { get; set; } // Nullable for current file target path

    /// <summary>
    ///     Gets or sets the name of the action currently being performed.
    /// </summary>
    public string? CurrentAction { get; set; } // Nullable for current action description
}