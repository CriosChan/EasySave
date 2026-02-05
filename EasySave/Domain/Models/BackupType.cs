namespace EasySave.Domain.Models;

/// <summary>
/// Types de sauvegarde supportes.
/// </summary>
public enum BackupType
{
    /// <summary>
    /// Sauvegarde complete.
    /// </summary>
    Complete,
    /// <summary>
    /// Sauvegarde differentielle.
    /// </summary>
    Differential
}
