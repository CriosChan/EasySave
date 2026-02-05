namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat de normalisation et conversion des chemins.
/// </summary>
public interface IPathService
{
    /// <summary>
    /// Normalise un chemin et verifie l'existence du dossier.
    /// </summary>
    /// <param name="rawPath">Chemin brut saisi par l'utilisateur.</param>
    /// <param name="normalizedPath">Chemin normalise.</param>
    /// <returns>Vrai si le dossier existe.</returns>
    bool TryNormalizeExistingDirectory(string rawPath, out string normalizedPath);

    /// <summary>
    /// Convertit un chemin en chemin absolu (UNC-like sur Windows si applicable).
    /// </summary>
    /// <param name="path">Chemin a convertir.</param>
    /// <returns>Chemin converti.</returns>
    string ToFullUncLikePath(string path);

    /// <summary>
    /// Calcule un chemin relatif entre une base et un chemin complet.
    /// </summary>
    /// <param name="basePath">Chemin de base.</param>
    /// <param name="fullPath">Chemin complet.</param>
    /// <returns>Chemin relatif.</returns>
    string GetRelativePath(string basePath, string fullPath);
}
