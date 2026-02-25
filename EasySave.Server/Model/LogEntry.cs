namespace EasySave.Log.Model
{
    /// <summary>
    /// Log entry for a backup operation.
    /// Contains information about the backup operation's execution details.
    /// </summary>
    public sealed class LogEntry
    {
        /// <summary>
        /// Gets or sets the timestamp of the log entry.
        /// Default is the current time.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the IP address of the client that initiated the backup.
        /// </summary>
        public string ClientIPAddress { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the backup associated with this log entry.
        /// </summary>
        public string BackupName { get; set; } = "";

        /// <summary>
        /// Gets or sets the full source path of the file being backed up.
        /// The path may be in UNC format when applicable.
        /// </summary>
        public string SourcePath { get; set; } = "";

        /// <summary>
        /// Gets or sets the full target path where the backup is stored.
        /// The path may be in UNC format when applicable.
        /// </summary>
        public string TargetPath { get; set; } = "";

        /// <summary>
        /// Gets or sets the size of the file in bytes being backed up.
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the transfer time in milliseconds.
        /// A negative value indicates an error during the transfer.
        /// </summary>
        public long TransferTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the time taken for crypting the file in milliseconds.
        /// Default is 0 ms.
        /// </summary>
        public long CryptingTimeMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets an optional error message when a transfer fails.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Formats the log entry as a string for display purposes.
        /// </summary>
        /// <returns>A formatted string representing the log entry.</returns>
        public override string ToString()
        {
            return $@"
            Log Entry:
            -------------------------
            Timestamp      : {Timestamp:yyyy-MM-dd HH:mm:ss}
            Backup Name    : {BackupName}
            Source Path    : {SourcePath}
            Target Path    : {TargetPath}
            File Size (Bytes) : {FileSizeBytes:n0}
            Transfer Time (ms) : {TransferTimeMs}
            Crypting Time (ms) : {CryptingTimeMs}
            Error Message   : {(ErrorMessage ?? "None")}
            -------------------------
            ";
        }
    }
}
