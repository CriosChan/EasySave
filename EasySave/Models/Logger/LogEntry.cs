namespace EasySave.Core.Models;

/// <summary>
///     Log entry for a backup operation.
///     Contains information about the backup operation's execution details.
/// </summary>
public sealed class LogEntry
{
    /// <summary>
    ///     Gets or sets the timestamp of the log entry.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now; // Default to the current time

    /// <summary>
    ///     Gets or sets the name of the backup associated with this log entry.
    /// </summary>
    public string BackupName { get; set; } = ""; // Name of the backup

    /// <summary>
    ///     Gets or sets the full source path of the file being backed up.
    ///     The path may be in UNC format when applicable.
    /// </summary>
    public string SourcePath { get; set; } = ""; // Full source file path

    /// <summary>
    ///     Gets or sets the full target path where the backup is stored.
    ///     The path may be in UNC format when applicable.
    /// </summary>
    public string TargetPath { get; set; } = ""; // Full target file path

    /// <summary>
    ///     Gets or sets the size of the file in bytes being backed up.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    ///     Gets or sets the transfer time in milliseconds.
    ///     A negative value indicates an error during the transfer.
    /// </summary>
    public long TransferTimeMs { get; set; }

    /// <summary>
    ///     Gets or sets the time taken for crypting the file in milliseconds.
    /// </summary>
    public long CryptingTimeMs { get; set; } = 0; // Default to 0 ms

    /// <summary>
    ///     Gets or sets an optional error message when a transfer fails.
    /// </summary>
    public string? ErrorMessage { get; set; } // Nullable string for error message
}