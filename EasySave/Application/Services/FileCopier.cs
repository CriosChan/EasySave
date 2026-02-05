using System.Diagnostics;
using EasySave.Application.Abstractions;

namespace EasySave.Application.Services;

/// <summary>
/// Copie des fichiers avec mesure du temps et preservation des timestamps.
/// </summary>
public sealed class FileCopier : IFileCopier
{
    /// <summary>
    /// Copie un fichier et renvoie la duree de transfert en millisecondes.
    /// </summary>
    /// <param name="sourceFile">Chemin source.</param>
    /// <param name="targetFile">Chemin cible.</param>
    /// <returns>Duree de copie en ms.</returns>
    public long Copy(string sourceFile, string targetFile)
    {
        const int BufferSize = 1024 * 1024; // 1 MiB
        FileInfo fi = new FileInfo(sourceFile);
        Stopwatch sw = Stopwatch.StartNew();

        using (FileStream source = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan))
        using (FileStream target = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.SequentialScan))
        {
            source.CopyTo(target, BufferSize);
        }

        sw.Stop();

        // Preserve source timestamps.
        File.SetLastWriteTimeUtc(targetFile, fi.LastWriteTimeUtc);

        return sw.ElapsedMilliseconds;
    }
}
