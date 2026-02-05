namespace EasySave.Application.Abstractions;

/// <summary>
/// Contract for path normalization and conversion.
/// </summary>
public interface IPathService
{
    /// <summary>
    /// Normalizes a path and verifies the directory exists.
    /// </summary>
    /// <param name="rawPath">Raw path provided by the user.</param>
    /// <param name="normalizedPath">Normalized path.</param>
    /// <returns>True if the directory exists.</returns>
    bool TryNormalizeExistingDirectory(string rawPath, out string normalizedPath);

    /// <summary>
    /// Converts a path to an absolute path (UNC-like on Windows when applicable).
    /// </summary>
    /// <param name="path">Path to convert.</param>
    /// <returns>Converted path.</returns>
    string ToFullUncLikePath(string path);

    /// <summary>
    /// Computes a relative path between a base and a full path.
    /// </summary>
    /// <param name="basePath">Base path.</param>
    /// <param name="fullPath">Full path.</param>
    /// <returns>Relative path.</returns>
    string GetRelativePath(string basePath, string fullPath);
}
