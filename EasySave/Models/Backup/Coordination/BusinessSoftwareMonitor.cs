using System.Diagnostics;
using EasySave.Data.Configuration;
using EasySave.Models.Backup.Abstractions;

namespace EasySave.Models.Backup.Coordination;

/// <summary>
///     Detects configured business software processes from application settings.
/// </summary>
public sealed class BusinessSoftwareMonitor : IBusinessSoftwareMonitor
{
    private const int MinimumLooseMatchLength = 4;

    private readonly Func<IReadOnlyList<string>?> _getConfiguredProcessNames;
    private readonly Func<string, bool> _isProcessRunningByName;
    private readonly Func<IReadOnlyList<string>> _getRunningProcessNames;

    /// <summary>
    ///     Initializes a new monitor using configured business software process names.
    /// </summary>
    public BusinessSoftwareMonitor()
        : this(
            () => ApplicationConfiguration.Load().BusinessSoftwareProcessNames,
            processName => Process.GetProcessesByName(processName).Length > 0,
            () => Process.GetProcesses()
                .Select(process => process.ProcessName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray())
    {
    }

    /// <summary>
    ///     Initializes a new monitor using explicit configuration and process lookup delegates.
    /// </summary>
    /// <param name="configuredProcessNames">Configured business software process names.</param>
    /// <param name="isProcessRunningByName">Delegate used for exact process-name detection.</param>
    /// <param name="getRunningProcessNames">Delegate used to retrieve running process names for loose matching.</param>
    public BusinessSoftwareMonitor(
        IReadOnlyList<string>? configuredProcessNames,
        Func<string, bool> isProcessRunningByName,
        Func<IReadOnlyList<string>> getRunningProcessNames)
        : this(
            () => configuredProcessNames,
            isProcessRunningByName,
            getRunningProcessNames)
    {
    }

    /// <summary>
    ///     Initializes a new monitor using dynamic configuration and process lookup delegates.
    /// </summary>
    /// <param name="getConfiguredProcessNames">Delegate returning configured business software process names.</param>
    /// <param name="isProcessRunningByName">Delegate used for exact process-name detection.</param>
    /// <param name="getRunningProcessNames">Delegate used to retrieve running process names for loose matching.</param>
    public BusinessSoftwareMonitor(
        Func<IReadOnlyList<string>?> getConfiguredProcessNames,
        Func<string, bool> isProcessRunningByName,
        Func<IReadOnlyList<string>> getRunningProcessNames)
    {
        _getConfiguredProcessNames =
            getConfiguredProcessNames ?? throw new ArgumentNullException(nameof(getConfiguredProcessNames));
        _isProcessRunningByName = isProcessRunningByName ?? throw new ArgumentNullException(nameof(isProcessRunningByName));
        _getRunningProcessNames = getRunningProcessNames ?? throw new ArgumentNullException(nameof(getRunningProcessNames));
    }

    /// <summary>
    ///     Gets the normalized configured software process names.
    /// </summary>
    public IReadOnlyList<string> ConfiguredSoftwareNames => BuildProcessNames(_getConfiguredProcessNames());

    /// <summary>
    ///     Determines whether at least one configured business software process is currently running.
    /// </summary>
    /// <returns>
    ///     True when at least one process with a configured name is detected; otherwise, false.
    /// </returns>
    public bool IsBusinessSoftwareRunning()
    {
        try
        {
            var normalizedProcessNames = BuildProcessNames(_getConfiguredProcessNames());
            if (normalizedProcessNames.Length == 0)
                return false;

            foreach (var processName in normalizedProcessNames)
                if (_isProcessRunningByName(processName))
                    return true;

            var runningProcessNames = _getRunningProcessNames()
                .Select(NormalizeProcessName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var configuredName in normalizedProcessNames)
            foreach (var runningName in runningProcessNames)
                if (IsLooseMatch(configuredName, runningName))
                    return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Builds the final normalized process name list from the list-based setting.
    /// </summary>
    /// <param name="configuredNames">Configured process names from the list-based setting.</param>
    /// <returns>Distinct normalized process names used for detection.</returns>
    private static string[] BuildProcessNames(IReadOnlyList<string>? configuredNames)
    {
        var rawNames = new List<string>();

        if (configuredNames != null)
            rawNames.AddRange(configuredNames);

        return rawNames
            .Select(NormalizeProcessName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
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

    /// <summary>
    ///     Performs robust name matching between configured and running process names.
    /// </summary>
    private static bool IsLooseMatch(string configuredName, string runningProcessName)
    {
        if (string.Equals(configuredName, runningProcessName, StringComparison.OrdinalIgnoreCase))
            return true;

        var configuredKey = NormalizeForComparison(configuredName);
        var runningKey = NormalizeForComparison(runningProcessName);

        if (string.IsNullOrWhiteSpace(configuredKey) || string.IsNullOrWhiteSpace(runningKey))
            return false;

        if (string.Equals(configuredKey, runningKey, StringComparison.OrdinalIgnoreCase))
            return true;

        if (configuredKey.Length >= MinimumLooseMatchLength &&
            runningKey.Contains(configuredKey, StringComparison.OrdinalIgnoreCase))
            return true;

        if (runningKey.Length >= MinimumLooseMatchLength &&
            configuredKey.Contains(runningKey, StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var configuredToken in ExtractTokens(configuredName))
            if (runningKey.Contains(configuredToken, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    /// <summary>
    ///     Normalizes process names for loose comparison by keeping alphanumeric characters only.
    /// </summary>
    private static string NormalizeForComparison(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var chars = value.Where(char.IsLetterOrDigit).ToArray();
        return new string(chars);
    }

    /// <summary>
    ///     Extracts alphanumeric tokens with support for separators and camel-case boundaries.
    /// </summary>
    private static IReadOnlyList<string> ExtractTokens(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        var tokens = new List<string>();
        var current = new List<char>();
        char? previous = null;

        foreach (var character in value)
        {
            if (!char.IsLetterOrDigit(character))
            {
                FlushToken();
                continue;
            }

            if (previous.HasValue &&
                char.IsLower(previous.Value) &&
                char.IsUpper(character))
                FlushToken();

            current.Add(char.ToLowerInvariant(character));
            previous = character;
        }

        FlushToken();

        return tokens
            .Where(token => token.Length >= MinimumLooseMatchLength)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        void FlushToken()
        {
            if (current.Count > 0)
                tokens.Add(new string(current.ToArray()));

            current.Clear();
            previous = null;
        }
    }
}
