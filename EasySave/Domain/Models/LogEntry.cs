namespace EasySave.Domain.Models;

/// <summary>
/// Entree de log d'une operation de sauvegarde.
/// </summary>
public sealed class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string BackupName { get; set; } = "";

    // Full source/target paths (UNC when applicable)
    public string SourcePath { get; set; } = "";
    public string TargetPath { get; set; } = "";

    public long FileSizeBytes { get; set; }

    // Transfer time in ms, negative if error (per requirements)
    public long TransferTimeMs { get; set; }

    // Optional additional information for support.
    // (Action and Error fields removed per request)
}
