namespace EasySave.Models.Backup.Interfaces;

/// <summary>
///     Detects whether the configured business software is currently running.
/// </summary>
public interface IBusinessSoftwareMonitor
{
    /// <summary>
    ///     Gets the configured software names used for detection.
    /// </summary>
    IReadOnlyList<string> ConfiguredSoftwareNames { get; }

    /// <summary>
    ///     Returns true when at least one configured business software process is running.
    /// </summary>
    bool IsBusinessSoftwareRunning();
}
