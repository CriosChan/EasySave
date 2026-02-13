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

    public BusinessSoftwareMonitor()
    {
        var configuredName = ApplicationConfiguration.Load().BusinessSoftwareProcessName;
        _normalizedProcessName = NormalizeProcessName(configuredName);
    }

    public string ConfiguredSoftwareName => _normalizedProcessName;

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
