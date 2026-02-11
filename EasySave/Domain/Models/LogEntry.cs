namespace EasySave.Domain.Models;

/// <summary>
///     Log entry for a backup operation.
/// </summary>
public sealed class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string BackupName { get; set; } = "";
    public string HostName { get; set; } = Environment.MachineName;
    public string UserName { get; set; } = Environment.UserName;

    // Full source/target paths (UNC when applicable)
    public string SourcePath { get; set; } = "";
    public string TargetPath { get; set; } = "";

    public long FileSizeBytes { get; set; }

    // Transfer time in ms, negative if error (per requirements)
    public long TransferTimeMs { get; set; }

    // Encryption time in ms for CryptoSoft:
    // 0 = no encryption, >0 = encryption duration, <0 = error code.
    public long EncryptionTimeMs { get; set; }

    // Optional error message when a transfer fails.
    public string? ErrorMessage { get; set; }
}
