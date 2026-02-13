namespace EasySave.Data.Configuration;

/// <summary>
///     Resolves application data directories (config, logs, etc.) from configuration values.
/// </summary>
/// <remarks>
///     The specification explicitly discourages hardcoded temp locations.
///     This helper resolves paths under an OS-appropriate application data folder by default.
///     In the provided appsettings.json, paths are expressed as "/config" and "/log".
///     Those values are treated as application subfolders (not filesystem root folders)
///     to keep the application usable on both Windows and Linux without requiring elevated rights.
/// </remarks>
public static class DataPathResolver
{
    private const string AppFolderName = "EasySave";

    /// <summary>
    ///     Resolves a data directory based on configuration and a default subfolder.
    /// </summary>
    /// <param name="configuredPath">Configured path.</param>
    /// <param name="defaultSubfolder">Default subfolder.</param>
    /// <returns>Resolved absolute path.</returns>
    public static string ResolveDirectory(string configuredPath, string defaultSubfolder)
    {
        var baseDir = GetBaseDataDirectory();

        var raw = (configuredPath ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
            raw = defaultSubfolder;

        // If a truly absolute path is provided (drive letter or UNC on Windows), respect it.
        if (IsSafeAbsolute(raw))
            return raw;

        // Otherwise treat it as a sub-path within the application's data directory.
        var cleaned = raw.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrWhiteSpace(cleaned))
            cleaned = defaultSubfolder;

        return Path.Combine(baseDir, cleaned);
    }

    /// <summary>
    ///     Returns the base folder for application data.
    /// </summary>
    private static string GetBaseDataDirectory()
    {
        // Prefer per-user folders (writeable without elevated rights), then fall back.
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(local))
            return Path.Combine(local, AppFolderName);

        var user = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrWhiteSpace(user))
            return Path.Combine(user, AppFolderName);

        var common = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (!string.IsNullOrWhiteSpace(common))
            return Path.Combine(common, AppFolderName);

        // Last resort: executable directory.
        return Path.Combine(AppContext.BaseDirectory, AppFolderName);
    }

    /// <summary>
    ///     Indicates whether an absolute path can be used safely (OS-aware).
    /// </summary>
    /// <param name="path">Path to evaluate.</param>
    /// <returns>True if the path is accepted.</returns>
    private static bool IsSafeAbsolute(string path)
    {
        if (!Path.IsPathRooted(path))
            return false;

        if (OperatingSystem.IsWindows())
        {
            // Accept UNC paths.
            if (path.StartsWith("\\\\"))
                return true;

            // Accept drive-rooted paths.
            return path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':';
        }

        // On non-Windows systems, we deliberately avoid writing to filesystem root by default.
        return false;
    }
}