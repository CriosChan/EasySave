using System.Diagnostics;
using EasySave.Application.Abstractions;

namespace EasySave.Application.Services;

/// <summary>
/// Copies files while measuring time and preserving timestamps.
/// </summary>
public sealed class FileCopier : IFileCopier
{
    /// <summary>
    /// Copies a file and returns the transfer duration in milliseconds.
    /// </summary>
    /// <param name="sourceFile">Source path.</param>
    /// <param name="targetFile">Target path.</param>
    /// <returns>Copy duration in ms.</returns>
    public long Copy(string sourceFile, string targetFile)
    {
        const int bufferSize = 1024 * 1024; // 1 MiB
        FileInfo fi = new FileInfo(sourceFile);
        Stopwatch sw = Stopwatch.StartNew();

        using (FileStream source = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
        using (FileStream target = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
        {
            source.CopyTo(target, bufferSize);
        }

        sw.Stop();

        // Preserve source timestamps.
        File.SetLastWriteTimeUtc(targetFile, fi.LastWriteTimeUtc);

        return sw.ElapsedMilliseconds;
    }
}
