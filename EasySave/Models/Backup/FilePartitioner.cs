using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Partitions a flat list of files into a priority queue and a standard queue
///     based on a configurable set of priority extensions.
/// </summary>
public static class FilePartitioner
{
    /// <summary>
    ///     Splits <paramref name="files" /> into two queues according to the provided
    ///     <paramref name="priorityExtensions" />.
    ///     Extensions are normalised before matching: trimmed, lowercased, and prefixed with a dot
    ///     if not already present.
    /// </summary>
    /// <param name="files">Full list of files returned by the backup type selector.</param>
    /// <param name="priorityExtensions">
    ///     Collection of extensions that identify priority files (e.g. <c>".pdf"</c>, <c>"pdf"</c>,
    ///     <c>" .PDF "</c> — all normalised to <c>".pdf"</c>).
    /// </param>
    /// <returns>
    ///     A named tuple where <c>PriorityQueue</c> contains files whose extension matches a
    ///     priority extension, and <c>StandardQueue</c> contains the remaining files.
    ///     Both queues preserve the original relative order of the input list.
    /// </returns>
    public static (Queue<IFile> PriorityQueue, Queue<IFile> StandardQueue) Partition(
        IEnumerable<IFile> files,
        IEnumerable<string> priorityExtensions)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(priorityExtensions);

        // Build a normalised hash-set for O(1) look-ups
        var normalised = new HashSet<string>(
            priorityExtensions.Select(NormaliseExtension),
            StringComparer.OrdinalIgnoreCase);

        var priorityQueue = new Queue<IFile>();
        var standardQueue = new Queue<IFile>();

        foreach (var file in files)
        {
            var ext = NormaliseExtension(Path.GetExtension(file.SourceFile));
            if (normalised.Contains(ext))
                priorityQueue.Enqueue(file);
            else
                standardQueue.Enqueue(file);
        }

        return (priorityQueue, standardQueue);
    }

    /// <summary>
    ///     Normalises an extension string: trims surrounding whitespace, converts to lower-case,
    ///     and ensures a leading dot is present.
    /// </summary>
    /// <param name="extension">Raw extension value (e.g. <c>"PDF"</c>, <c>".pdf"</c>, <c>" pdf "</c>).</param>
    /// <returns>Normalised extension, e.g. <c>".pdf"</c>. Returns an empty string when the input is empty.</returns>
    public static string NormaliseExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        var trimmed = extension.Trim().ToLowerInvariant();
        return trimmed.StartsWith('.') ? trimmed : '.' + trimmed;
    }
}

