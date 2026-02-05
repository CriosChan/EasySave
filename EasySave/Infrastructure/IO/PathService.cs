using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.IO;

/// <summary>
/// Implementation des operations de normalisation et conversion de chemins.
/// </summary>
public sealed class PathService : IPathService
{
    /// <summary>
    /// Normalise un chemin et verifie l'existence du dossier.
    /// </summary>
    /// <param name="rawPath">Chemin brut.</param>
    /// <param name="normalizedPath">Chemin normalise.</param>
    /// <returns>Vrai si le dossier existe.</returns>
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
    /// Convertit un chemin en chemin absolu en respectant les UNC.
    /// </summary>
    /// <param name="path">Chemin a convertir.</param>
    /// <returns>Chemin absolu.</returns>
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
    /// Calcule un chemin relatif entre deux chemins.
    /// </summary>
    /// <param name="basePath">Chemin de base.</param>
    /// <param name="fullPath">Chemin complet.</param>
    /// <returns>Chemin relatif.</returns>
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
    /// Nettoie un chemin saisi par l'utilisateur (espaces, quotes, variables env).
    /// </summary>
    /// <param name="path">Chemin brut.</param>
    /// <returns>Chemin nettoye.</returns>
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
    /// Supprime une paire de guillemets d'encadrement si presente.
    /// </summary>
    /// <param name="path">Chemin brut.</param>
    /// <returns>Chemin sans guillemets d'encadrement.</returns>
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

