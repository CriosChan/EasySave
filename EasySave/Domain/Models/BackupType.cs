namespace EasySave.Domain.Models;

/// <summary>
/// Supported backup types.
/// </summary>
public enum BackupType
{
    /// <summary>
    /// Full backup.
    /// </summary>
    Complete,
    /// <summary>
    /// Differential backup.
    /// </summary>
    Differential
}
