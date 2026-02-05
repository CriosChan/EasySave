namespace EasySave.Utils;

/// <summary>
/// Resolves application data directories (config, logs, etc.) from configuration values.
/// </summary>
/// <remarks>
/// The specification explicitly discourages hardcoded temp locations.
/// This helper resolves paths under an OS-appropriate application data folder by default.
///
/// In the provided appsettings.json, paths are expressed as "/config" and "/log".
/// Those values are treated as application subfolders (not filesystem root folders)
/// to keep the application usable on both Windows and Linux without requiring elevated rights.
/// </remarks>
public static class DataPathResolver
{
    private const string AppFolderName = "EasySave";

    public static string ResolveDirectory(string configuredPath, string defaultSubfolder)
    {
        string baseDir = GetBaseDataDirectory();

        string raw = (configuredPath ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
            raw = defaultSubfolder;

        // If a truly absolute path is provided (drive letter or UNC on Windows), respect it.
        if (IsSafeAbsolute(raw))
            return raw;

        // Otherwise treat it as a sub-path within the application's data directory.
        string cleaned = raw.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        
        if (string.IsNullOrWhiteSpace(cleaned) || cleaned.All(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar))
            cleaned = defaultSubfolder;

        return Path.Combine(baseDir, cleaned);
    }

    private static string GetBaseDataDirectory()
    {
        // Prefer per-user folders (writeable without elevated rights), then fall back.
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(local))
            return Path.Combine(local, AppFolderName);

        string user = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrWhiteSpace(user))
            return Path.Combine(user, AppFolderName);

        string common = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (!string.IsNullOrWhiteSpace(common))
            return Path.Combine(common, AppFolderName);

        // Last resort: executable directory.
        return Path.Combine(AppContext.BaseDirectory, AppFolderName);
    }

    private static bool IsSafeAbsolute(string path)
    {
        if (!Path.IsPathRooted(path))
            return false;

        if (OperatingSystem.IsWindows())
        {
            // Accept UNC paths.
            if (path.StartsWith("\\\\"))
                return true;

            // Accept drive-rooted paths (e.g., C:\...), but NOT paths like C:folder
            if (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' &&
                (path[2] == '\\' || path[2] == '/'))
                return true;

            return false;
        }

        // On non-Windows systems, we deliberately avoid writing to filesystem root by default.
        return false;
    }
}
