using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace EasySave.Models.BusinessSoftware;

/// <summary>
///     Provides a catalog of installed software candidates usable as business software blockers.
/// </summary>
public sealed class BusinessSoftwareCatalogService : IBusinessSoftwareCatalogService
{
    /// <summary>
    ///     Loads available software candidates from the local machine.
    /// </summary>
    /// <returns>Read-only list of software candidates.</returns>
    public IReadOnlyList<BusinessSoftwareCatalogItem> GetAvailableSoftware()
    {
        var items = new List<BusinessSoftwareCatalogItem>();
        if (OperatingSystem.IsWindows())
        {
            items.AddRange(ReadFromRegistry());
            items.AddRange(ReadFromWindowsSystemApplications());
        }

        items.AddRange(ReadFromRunningProcesses());

        return items
            .GroupBy(item => item.ProcessName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase).First())
            .OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    ///     Reads software candidates from Windows uninstall registry keys.
    /// </summary>
    /// <returns>List of detected software candidates.</returns>
    [SupportedOSPlatform("windows")]
    private static List<BusinessSoftwareCatalogItem> ReadFromRegistry()
    {
        var items = new List<BusinessSoftwareCatalogItem>();

        foreach (var source in GetRegistrySources())
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(source.hive, source.view);
                using var uninstallKey = baseKey.OpenSubKey(source.uninstallKeyPath);
                if (uninstallKey == null)
                    continue;

                foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                {
                    using var appKey = uninstallKey.OpenSubKey(subKeyName);
                    if (appKey == null)
                        continue;

                    var displayName = (appKey.GetValue("DisplayName") as string)?.Trim();
                    if (string.IsNullOrWhiteSpace(displayName))
                        continue;

                    var displayIcon = appKey.GetValue("DisplayIcon") as string;
                    var uninstallString = appKey.GetValue("UninstallString") as string;
                    var installLocation = appKey.GetValue("InstallLocation") as string;

                    var processName = TryExtractProcessName(displayIcon);
                    if (string.IsNullOrWhiteSpace(processName))
                        processName = TryExtractProcessName(uninstallString);

                    if (string.IsNullOrWhiteSpace(processName))
                        processName = TryResolveFromInstallLocation(installLocation, displayName);

                    if (string.IsNullOrWhiteSpace(processName))
                        processName = BuildFallbackProcessName(displayName);

                    if (string.IsNullOrWhiteSpace(processName))
                        continue;

                    items.Add(new BusinessSoftwareCatalogItem(displayName, processName));
                }
            }
            catch
            {
                // Keep best-effort behavior and continue with next source.
            }

        return items;
    }

    /// <summary>
    ///     Returns Windows registry locations used to enumerate installed software.
    /// </summary>
    /// <returns>Enumeration sources for installed software lookup.</returns>
    [SupportedOSPlatform("windows")]
    private static (RegistryHive hive, RegistryView view, string uninstallKeyPath)[] GetRegistrySources()
    {
        return
        [
            (RegistryHive.LocalMachine, RegistryView.Registry64,
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
            (RegistryHive.LocalMachine, RegistryView.Registry32,
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
            (RegistryHive.CurrentUser, RegistryView.Default, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
        ];
    }

    /// <summary>
    ///     Reads built-in Windows software and executable candidates from system directories.
    /// </summary>
    /// <returns>List of system software candidates.</returns>
    [SupportedOSPlatform("windows")]
    private static List<BusinessSoftwareCatalogItem> ReadFromWindowsSystemApplications()
    {
        var items = new List<BusinessSoftwareCatalogItem>();
        var windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var systemDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var systemAppsDirectory = Path.Combine(windowsDirectory, "SystemApps");

        var folders = new (string path, bool recursive)[]
        {
            (windowsDirectory, false),
            (systemDirectory, false),
            (systemAppsDirectory, true)
        };

        foreach (var folder in folders)
        foreach (var executablePath in EnumerateExecutableFiles(folder.path, folder.recursive))
        {
            var processName = Path.GetFileNameWithoutExtension(executablePath).Trim();
            if (string.IsNullOrWhiteSpace(processName))
                continue;

            if (IsInstallerOrUninstallProcess(processName))
                continue;

            var displayName = processName;
            items.Add(new BusinessSoftwareCatalogItem(displayName, processName));
        }

        return items;
    }

    /// <summary>
    ///     Reads software candidates from running processes as a fallback.
    /// </summary>
    /// <returns>List of running-process candidates.</returns>
    private static List<BusinessSoftwareCatalogItem> ReadFromRunningProcesses()
    {
        try
        {
            return Process.GetProcesses()
                .Select(process => process.ProcessName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(name => new BusinessSoftwareCatalogItem(name, name))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Enumerates executable files from a directory with robust access error handling.
    /// </summary>
    /// <param name="rootPath">Root path to scan.</param>
    /// <param name="recursive">True to include subdirectories; otherwise, only top-level files.</param>
    /// <returns>Executable file paths.</returns>
    private static IEnumerable<string> EnumerateExecutableFiles(string rootPath, bool recursive)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            yield break;

        if (!Directory.Exists(rootPath))
            yield break;

        if (!recursive)
        {
            IEnumerable<string> topFiles;
            try
            {
                topFiles = Directory.EnumerateFiles(rootPath, "*.exe", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                yield break;
            }

            foreach (var file in topFiles)
                yield return file;

            yield break;
        }

        var queue = new Queue<string>();
        queue.Enqueue(rootPath);

        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(currentPath, "*.exe", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
                yield return file;

            IEnumerable<string> directories;
            try
            {
                directories = Directory.EnumerateDirectories(currentPath, "*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                continue;
            }

            foreach (var directory in directories)
                queue.Enqueue(directory);
        }
    }

    /// <summary>
    ///     Extracts a process name candidate from a command-like registry value.
    /// </summary>
    /// <param name="rawCommand">Raw command or executable path.</param>
    /// <returns>Process name candidate or empty string.</returns>
    private static string TryExtractProcessName(string? rawCommand)
    {
        if (string.IsNullOrWhiteSpace(rawCommand))
            return string.Empty;

        var command = rawCommand.Trim().Trim('"');
        var commaIndex = command.IndexOf(',');
        if (commaIndex >= 0)
            command = command[..commaIndex];

        var executablePath = ExtractExecutablePath(command);
        if (string.IsNullOrWhiteSpace(executablePath))
            return string.Empty;

        executablePath = Environment.ExpandEnvironmentVariables(executablePath);
        var processName = Path.GetFileNameWithoutExtension(executablePath).Trim();
        if (string.IsNullOrWhiteSpace(processName))
            return string.Empty;

        return IsInstallerOrUninstallProcess(processName) ? string.Empty : processName;
    }

    /// <summary>
    ///     Extracts an executable path from a command line.
    /// </summary>
    /// <param name="command">Command line value.</param>
    /// <returns>Executable path candidate.</returns>
    private static string ExtractExecutablePath(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return string.Empty;

        var trimmed = command.Trim();
        if (trimmed.StartsWith('"'))
        {
            var closingQuote = trimmed.IndexOf('"', 1);
            return closingQuote > 1 ? trimmed[1..closingQuote] : trimmed.Trim('"');
        }

        var spaceIndex = trimmed.IndexOf(' ');
        return spaceIndex > 0 ? trimmed[..spaceIndex] : trimmed;
    }

    /// <summary>
    ///     Tries to infer a process name from install location.
    /// </summary>
    /// <param name="installLocation">Install folder path.</param>
    /// <param name="displayName">Display name used for ranking candidates.</param>
    /// <returns>Process name candidate or empty string.</returns>
    private static string TryResolveFromInstallLocation(string? installLocation, string displayName)
    {
        if (string.IsNullOrWhiteSpace(installLocation))
            return string.Empty;

        try
        {
            if (!Directory.Exists(installLocation))
                return string.Empty;

            var candidates = Directory.EnumerateFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly)
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Where(name => !IsInstallerOrUninstallProcess(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (candidates.Count == 0)
                return string.Empty;

            return candidates
                .OrderByDescending(name => ComputeSimilarityScore(displayName, name))
                .ThenBy(name => name.Length)
                .ThenBy(name => name, StringComparer.OrdinalIgnoreCase)
                .First();
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    ///     Computes a simple similarity score between display name and candidate process name.
    /// </summary>
    /// <param name="displayName">Display name.</param>
    /// <param name="processName">Process name candidate.</param>
    /// <returns>Similarity score.</returns>
    private static int ComputeSimilarityScore(string displayName, string processName)
    {
        var d = NormalizeForComparison(displayName);
        var p = NormalizeForComparison(processName);
        if (string.IsNullOrWhiteSpace(d) || string.IsNullOrWhiteSpace(p))
            return 0;

        var score = 0;
        if (d.Contains(p, StringComparison.OrdinalIgnoreCase))
            score += 4;
        if (p.Contains(d, StringComparison.OrdinalIgnoreCase))
            score += 2;
        if (d.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            score += 2;
        if (p.StartsWith(d, StringComparison.OrdinalIgnoreCase))
            score += 1;
        score -= Math.Abs(d.Length - p.Length) / 4;
        return score;
    }

    /// <summary>
    ///     Builds a last-resort process name from the display name.
    /// </summary>
    /// <param name="displayName">Software display name.</param>
    /// <returns>Fallback process name candidate.</returns>
    private static string BuildFallbackProcessName(string displayName)
    {
        var normalized = NormalizeForComparison(displayName);
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
    }

    /// <summary>
    ///     Normalizes a value for loose name comparison.
    /// </summary>
    /// <param name="value">Input value.</param>
    /// <returns>Normalized alphanumeric value.</returns>
    private static string NormalizeForComparison(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var chars = value.Where(char.IsLetterOrDigit).ToArray();
        return new string(chars);
    }

    /// <summary>
    ///     Determines whether a process name likely corresponds to setup or uninstall tooling.
    /// </summary>
    /// <param name="processName">Process name to evaluate.</param>
    /// <returns>True when it looks like setup/uninstall tooling.</returns>
    private static bool IsInstallerOrUninstallProcess(string processName)
    {
        var name = processName.ToLowerInvariant();
        return name.Contains("unins") ||
               name.Contains("uninstall") ||
               name.Contains("setup") ||
               name.Contains("installer") ||
               name.Contains("install") ||
               name.Contains("msiexec") ||
               name.Contains("update");
    }
}