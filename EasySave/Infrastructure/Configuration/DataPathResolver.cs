namespace EasySave.Infrastructure.Configuration;

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

    /// <summary>
    /// Resolue un dossier de donnees en fonction de la configuration et d'un sous-dossier par defaut.
    /// </summary>
    /// <param name="configuredPath">Chemin configure.</param>
    /// <param name="defaultSubfolder">Sous-dossier par defaut.</param>
    /// <returns>Chemin absolu resolu.</returns>
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
        if (string.IsNullOrWhiteSpace(cleaned))
            cleaned = defaultSubfolder;

        return Path.Combine(baseDir, cleaned);
    }

    /// <summary>
    /// Retourne le dossier de base pour les donnees applicatives.
    /// </summary>
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

    /// <summary>
    /// Indique si un chemin absolu peut etre utilise sans risque (selon l'OS).
    /// </summary>
    /// <param name="path">Chemin a evaluer.</param>
    /// <returns>Vrai si le chemin est accepte.</returns>
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
