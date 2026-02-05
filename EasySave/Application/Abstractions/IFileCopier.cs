namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat de copie de fichier avec mesure de duree.
/// </summary>
public interface IFileCopier
{
    /// <summary>
    /// Copie un fichier et renvoie la duree en millisecondes.
    /// </summary>
    /// <param name="sourceFile">Chemin source.</param>
    /// <param name="targetFile">Chemin cible.</param>
    /// <returns>Duree de copie en ms.</returns>
    long Copy(string sourceFile, string targetFile);
}
