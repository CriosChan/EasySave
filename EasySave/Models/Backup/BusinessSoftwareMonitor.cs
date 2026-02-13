using System.Diagnostics;
using EasySave.Data.Configuration;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Detects configured business software processes from application settings.
/// </summary>
public sealed class BusinessSoftwareMonitor : IBusinessSoftwareMonitor
{
    private readonly string[] _normalizedProcessNames;

    /// <summary>
    ///     Initializes a new monitor using configured business software process names.
    /// </summary>
    public BusinessSoftwareMonitor()
    {
        var config = ApplicationConfiguration.Load();
        _normalizedProcessNames = BuildProcessNames(
            config.BusinessSoftwareProcessNames,
            config.BusinessSoftwareProcessName
        );
    }

    /// <summary>
    ///     Gets the normalized configured software process names.
    /// </summary>
    public IReadOnlyList<string> ConfiguredSoftwareNames => _normalizedProcessNames;

    /// <summary>
    ///     Determines whether at least one configured business software process is currently running.
    /// </summary>
    /// <returns>
    ///     True when at least one process with a configured name is detected; otherwise, false.
    /// </returns>
    public bool IsBusinessSoftwareRunning()
    {
        if (_normalizedProcessNames.Length == 0)
            return false;

        try
        {
            foreach (var processName in _normalizedProcessNames)
                if (Process.GetProcessesByName(processName).Length > 0)
                    return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Builds the final normalized process name list from the new list-based setting and legacy single-value setting.
    /// </summary>
    /// <param name="configuredNames">Configured process names from the list-based setting.</param>
    /// <param name="legacyConfiguredName">Configured process names from the legacy single-value setting.</param>
    /// <returns>Distinct normalized process names used for detection.</returns>
    private static string[] BuildProcessNames(IReadOnlyList<string>? configuredNames, string legacyConfiguredName)
    {
        var rawNames = new List<string>();

        if (configuredNames != null)
            rawNames.AddRange(configuredNames);

        if (!string.IsNullOrWhiteSpace(legacyConfiguredName))
            rawNames.AddRange(SplitConfiguredNames(legacyConfiguredName));

        return rawNames
            .Select(NormalizeProcessName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    ///     Splits a raw process configuration value into individual names.
    /// </summary>
    /// <param name="configuredNames">Raw configured names separated by ';' or ','.</param>
    /// <returns>Individual configured process names.</returns>
    private static IEnumerable<string> SplitConfiguredNames(string configuredNames)
    {
        return configuredNames.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
