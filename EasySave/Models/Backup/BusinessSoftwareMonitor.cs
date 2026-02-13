using System.Diagnostics;
using EasySave.Data.Configuration;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Detects configured business software process from application settings.
/// </summary>
public sealed class BusinessSoftwareMonitor : IBusinessSoftwareMonitor
{
    private readonly string _normalizedProcessName;

    /// <summary>
    ///     Initializes a new monitor using the configured business software process name.
    /// </summary>
    public BusinessSoftwareMonitor()
    {
        var configuredName = ApplicationConfiguration.Load().BusinessSoftwareProcessName;
        _normalizedProcessName = NormalizeProcessName(configuredName);
    }

    /// <summary>
    ///     Gets the normalized configured software process name.
    /// </summary>
    public string ConfiguredSoftwareName => _normalizedProcessName;

    /// <summary>
    ///     Determines whether the configured business software process is currently running.
    /// </summary>
    /// <returns>
    ///     True when at least one process with the configured name is detected; otherwise, false.
    /// </returns>
    public bool IsBusinessSoftwareRunning()
    {
        if (string.IsNullOrWhiteSpace(_normalizedProcessName))
            return false;

        try
        {
            return Process.GetProcessesByName(_normalizedProcessName).Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Normalizes a configured process value to a process name usable by process lookup.
    ///     Accepts plain names, file names, or full executable paths.
    /// </summary>
    /// <param name="configuredName">Configured business software value from settings.</param>
    /// <returns>A process name without extension, or an empty string if not configured.</returns>
    private static string NormalizeProcessName(string configuredName)
    {
        if (string.IsNullOrWhiteSpace(configuredName))
            return string.Empty;

        var trimmed = configuredName.Trim();
        var filename = Path.GetFileName(trimmed);
        var withoutExtension = Path.GetFileNameWithoutExtension(filename);
        return withoutExtension.Trim();
    }
}
