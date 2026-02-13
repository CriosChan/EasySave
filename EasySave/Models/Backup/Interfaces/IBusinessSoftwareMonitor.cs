namespace EasySave.Models.Backup.Interfaces;

/// <summary>
///     Detects whether the configured business software is currently running.
/// </summary>
public interface IBusinessSoftwareMonitor
{
    /// <summary>
    ///     Gets the configured software name used for detection.
    /// </summary>
    string ConfiguredSoftwareName { get; }

    /// <summary>
    ///     Returns true when the configured business software is running.
    /// </summary>
    bool IsBusinessSoftwareRunning();
}
