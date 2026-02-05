using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.IO;

/// <summary>
/// Implementation of path normalization and conversion operations.
/// </summary>
public sealed class PathService : IPathService
{
    /// <summary>
    /// Normalizes a path and verifies the directory exists.
    /// </summary>
    /// <param name="rawPath">Raw path.</param>
    /// <param name="normalizedPath">Normalized path.</param>
    /// <returns>True if the directory exists.</returns>
    public bool TryNormalizeExistingDirectory(string rawPath, out string normalizedPath)
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
    /// Converts a path to an absolute path while respecting UNC paths.
    /// </summary>
    /// <param name="path">Path to convert.</param>
    /// <returns>Absolute path.</returns>
    public string ToFullUncLikePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        string cleaned = NormalizeUserPath(path);

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
            return cleaned;
        }
    }

    /// <summary>
    /// Computes a relative path between two paths.
    /// </summary>
    /// <param name="basePath">Base path.</param>
    /// <param name="fullPath">Full path.</param>
    /// <returns>Relative path.</returns>
    public string GetRelativePath(string basePath, string fullPath)
    {
        try
        {
            string b = NormalizeUserPath(basePath);
            string f = NormalizeUserPath(fullPath);
            return Path.GetRelativePath(b, f);
        }
        catch
        {
            try { return Path.GetFileName(NormalizeUserPath(fullPath)); }
            catch { return fullPath; }
        }
    }

    /// <summary>
    /// Cleans a user-provided path (spaces, quotes, env vars).
    /// </summary>
    /// <param name="path">Raw path.</param>
    /// <returns>Cleaned path.</returns>
    private static string NormalizeUserPath(string path)
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
    /// Removes wrapping quotes if present.
    /// </summary>
    /// <param name="path">Raw path.</param>
    /// <returns>Path without wrapping quotes.</returns>
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
}

