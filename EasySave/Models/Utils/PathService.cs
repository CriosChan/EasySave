namespace EasySave.Models.Utils;

/// <summary>
///     Implementation of path normalization and conversion operations.
/// </summary>
public static class PathService
{
    /// <summary>
    ///     Normalizes a path and verifies the directory exists.
    /// </summary>
    /// <param name="rawPath">Raw path.</param>
    /// <param name="normalizedPath">Normalized path.</param>
    /// <returns>True if the directory exists.</returns>
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

    /// <summary>
    ///     Checks if a directory path is accessible (e.g., not a disconnected external drive).
    /// </summary>
    /// <param name="path">Path to check.</param>
    /// <param name="errorMessage">Error message if the path is not accessible.</param>
    /// <returns>True if the path is accessible.</returns>
    public static bool IsDirectoryAccessible(string path, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (string.IsNullOrWhiteSpace(path))
        {
            errorMessage = "Path is empty or null.";
            return false;
        }

        var normalized = NormalizeUserPath(path);

        try
        {
            // First check if directory exists
            if (!Directory.Exists(normalized))
            {
                errorMessage = $"Directory does not exist: {normalized}";
                return false;
            }

            // Try to access the directory to ensure it's really accessible
            // This will fail if it's a disconnected drive or inaccessible network location
            _ = Directory.GetFiles(normalized, "*", SearchOption.TopDirectoryOnly);
            
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            errorMessage = $"Access denied to directory: {normalized}";
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            errorMessage = $"Directory not found or disconnected: {normalized}";
            return false;
        }
        catch (IOException ex)
        {
            errorMessage = $"I/O error accessing directory: {normalized} - {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"Error accessing directory: {normalized} - {ex.Message}";
            return false;
        }
    }

    /// <summary>
    ///     Converts a path to an absolute path while respecting UNC paths.
    /// </summary>
    /// <param name="path">Path to convert.</param>
    /// <returns>Absolute path.</returns>
    public static string ToFullUncLikePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var cleaned = NormalizeUserPath(path);

        if (OperatingSystem.IsWindows() && cleaned.StartsWith("\\\\"))
            return cleaned;

        try
        {
            var full = Path.GetFullPath(cleaned);
            if (OperatingSystem.IsWindows())
                full = full.Replace('/', '\\');

            return full;
        }
        catch
        {
            return cleaned;
        }
    }

    /// <summary>
    ///     Computes a relative path between two paths.
    /// </summary>
    /// <param name="basePath">Base path.</param>
    /// <param name="fullPath">Full path.</param>
    /// <returns>Relative path.</returns>
    public static string GetRelativePath(string basePath, string fullPath)
    {
        try
        {
            var b = NormalizeUserPath(basePath);
            var f = NormalizeUserPath(fullPath);
            return Path.GetRelativePath(b, f);
        }
        catch
        {
            try
            {
                return Path.GetFileName(NormalizeUserPath(fullPath));
            }
            catch
            {
                return fullPath;
            }
        }
    }

    /// <summary>
    ///     Cleans a user-provided path (spaces, quotes, env vars).
    /// </summary>
    /// <param name="path">Raw path.</param>
    /// <returns>Cleaned path.</returns>
    private static string NormalizeUserPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var cleaned = StripWrappingQuotes(path);
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
    ///     Removes wrapping quotes if present.
    /// </summary>
    /// <param name="path">Raw path.</param>
    /// <returns>Path without wrapping quotes.</returns>
    private static string StripWrappingQuotes(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var trimmed = path.Trim();
        if (trimmed.Length >= 2)
        {
            var first = trimmed[0];
            var last = trimmed[^1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                return trimmed[1..^1].Trim();
        }

        return trimmed;
    }
}