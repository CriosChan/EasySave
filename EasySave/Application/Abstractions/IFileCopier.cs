namespace EasySave.Application.Abstractions;

/// <summary>
/// Contract for file copying with duration measurement.
/// </summary>
public interface IFileCopier
{
    /// <summary>
    /// Copies a file and returns the duration in milliseconds.
    /// </summary>
    /// <param name="sourceFile">Source path.</param>
    /// <param name="targetFile">Target path.</param>
    /// <returns>Copy duration in ms.</returns>
    long Copy(string sourceFile, string targetFile);
}
