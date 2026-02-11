using System.Diagnostics;

namespace EasySave.Application.Services;

/// <summary>
///     Copies files while measuring time and preserving timestamps.
/// </summary>
public sealed class FileCopier
{
    /// <summary>
    ///     Copies a file and returns the transfer duration in milliseconds.
    /// </summary>
    /// <param name="sourceFile">Source path.</param>
    /// <param name="targetFile">Target path.</param>
    /// <returns>Copy duration in ms.</returns>
    public long Copy(string sourceFile, string targetFile)
    {
        return CopyAsync(sourceFile, targetFile, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Copies a file with cancellation support and optional pause hook.
    /// </summary>
    /// <param name="sourceFile">Source path.</param>
    /// <param name="targetFile">Target path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="waitIfPausedAsync">
    ///     Optional callback invoked between chunks to support coordinated pause behavior.
    /// </param>
    /// <returns>Copy duration in ms.</returns>
    public async Task<long> CopyAsync(
        string sourceFile,
        string targetFile,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task>? waitIfPausedAsync = null)
    {
        const int bufferSize = 1024 * 1024; // 1 MiB
        var fi = new FileInfo(sourceFile);
        var sw = Stopwatch.StartNew();

        await using (var source = new FileStream(
                         sourceFile,
                         FileMode.Open,
                         FileAccess.Read,
                         FileShare.Read,
                         bufferSize,
                         FileOptions.SequentialScan))
        await using (var target = new FileStream(
                         targetFile,
                         FileMode.Create,
                         FileAccess.Write,
                         FileShare.None,
                         bufferSize,
                         FileOptions.SequentialScan))
        {
            var buffer = new byte[bufferSize];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (waitIfPausedAsync != null)
                    await waitIfPausedAsync(cancellationToken).ConfigureAwait(false);

                var read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                    break;

                await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            }
        }

        sw.Stop();

        // Preserve source timestamps.
        File.SetLastWriteTimeUtc(targetFile, fi.LastWriteTimeUtc);

        return sw.ElapsedMilliseconds;
    }
}
