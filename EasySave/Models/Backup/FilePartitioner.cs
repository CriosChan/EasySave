using EasySave.Data.Configuration;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Partitions a flat list of files into a priority queue and a standard queue
///     based on the <see cref="ApplicationConfiguration.PriorityExtensions"/> setting.
/// </summary>
public static class FilePartitioner
{
    /// <summary>
    ///     Normalises a raw extension token so it can be compared consistently.
    ///     Rules: lowercase, trimmed, exactly one leading dot.
    /// </summary>
    /// <param name="raw">Raw extension string (e.g. "PDF", ".PDF", " pdf ").</param>
    /// <returns>Normalised extension string (e.g. ".pdf").</returns>
    public static string NormalizeExtension(string raw)
    {
        var trimmed = raw.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(trimmed))
            return string.Empty;
        return trimmed.StartsWith('.') ? trimmed : "." + trimmed;
    }

    /// <summary>
    ///     Splits <paramref name="files"/> into two ordered queues based on priority extensions.
    ///     Priority files come first; within each partition the original order is preserved.
    /// </summary>
    /// <param name="files">Full list of files produced by the backup-type selector.</param>
    /// <param name="priorityExtensions">
    ///     Raw extension tokens from configuration (case-insensitive, with or without dot).
    /// </param>
    /// <param name="priorityQueue">Files whose extension matches a priority extension.</param>
    /// <param name="standardQueue">Files whose extension does not match any priority extension.</param>
    public static void Partition(
        List<IFile> files,
        IEnumerable<string> priorityExtensions,
        out Queue<IFile> priorityQueue,
        out Queue<IFile> standardQueue)
    {
        var normalised = new HashSet<string>(
            priorityExtensions.Select(NormalizeExtension).Where(e => !string.IsNullOrEmpty(e)),
            StringComparer.Ordinal);

        priorityQueue = new Queue<IFile>();
        standardQueue = new Queue<IFile>();

        foreach (var file in files)
        {
            var ext = NormalizeExtension(Path.GetExtension(file.SourceFile));
            if (normalised.Contains(ext))
                priorityQueue.Enqueue(file);
            else
                standardQueue.Enqueue(file);
        }
    }
}

