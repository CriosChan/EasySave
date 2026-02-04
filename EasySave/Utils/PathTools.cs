namespace EasySave.Utils;

public static class PathTools
{
    /// <summary>
    /// Removes a single pair of wrapping quotes ("...") or ('...') if present.
    /// This prevents common user input patterns from breaking path resolution.
    /// </summary>
    private static string StripWrappingQuotes(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        string trimmed = path.Trim();
        if (trimmed.Length >= 2)
        {
            char first = trimmed[0];
            char last = trimmed[^1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                return trimmed[1..^1].Trim();
        }

        return trimmed;
    }

    /// <summary>
    /// Returns a cleaned path value intended for filesystem operations.
    /// - trims whitespace
    /// - strips wrapping quotes
    /// - expands environment variables (Windows/Linux)
    /// </summary>
    public static string NormalizeUserPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        string cleaned = StripWrappingQuotes(path);
        try
        {
            cleaned = Environment.ExpandEnvironmentVariables(cleaned);
        }
        catch
        {
            // Best-effort: keep the cleaned string.
        }

        return cleaned;
    }

    /// <summary>
    /// Tries to normalize a user-provided path and validates that it points to an existing directory.
    /// </summary>
    /// <param name="rawPath">User input path (may contain quotes, env vars, etc.).</param>
    /// <param name="normalizedPath">Normalized path ready to be used with <see cref="Directory"/> APIs.</param>
    /// <returns><c>true</c> if the directory exists; otherwise <c>false</c>.</returns>
    public static bool TryNormalizeExistingDirectory(string rawPath, out string normalizedPath)
    {
        normalizedPath = NormalizeUserPath(rawPath);
        if (string.IsNullOrWhiteSpace(normalizedPath))
            return false;

        try
        {
            return Directory.Exists(normalizedPath);
        }
        catch
        {
            return false;
        }
    }

    public static string ToFullUncLikePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        // Clean common user input patterns first.
        string cleaned = NormalizeUserPath(path);

        // Keep UNC paths as-is (after quote stripping).
        if (OperatingSystem.IsWindows() && cleaned.StartsWith("\\\\"))
            return cleaned;

        try
        {
            string full = Path.GetFullPath(cleaned);
            if (OperatingSystem.IsWindows())
                full = full.Replace('/', '\\');

            return full;
        }
        catch
        {
            // If the path is invalid, return the cleaned input as-is.
            return cleaned;
        }
    }

    public static string GetRelativePath(string basePath, string fullPath)
    {
        try
        {
            string b = NormalizeUserPath(basePath);
            string f = NormalizeUserPath(fullPath);
            return Path.GetRelativePath(b, f);
        }
        catch
        {
            // Fallback: if relative path fails, return file name.
            try { return Path.GetFileName(NormalizeUserPath(fullPath)); }
            catch { return fullPath; }
        }
    }
}
