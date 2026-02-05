using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Infrastructure.IO;

/// <summary>
/// Aides de lecture/ecriture JSON avec tolerance aux erreurs.
/// </summary>
public static class JsonFile
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Lit un fichier JSON ou retourne une valeur par defaut si absent/invalide.
    /// </summary>
    /// <typeparam name="T">Type de deserialization.</typeparam>
    /// <param name="path">Chemin du fichier.</param>
    /// <param name="defaultValue">Valeur par defaut.</param>
    /// <returns>Instance deserialisee ou valeur par defaut.</returns>
    public static T ReadOrDefault<T>(string path, T defaultValue) where T : class
    {
        if (!File.Exists(path))
            return defaultValue;

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, Options) ?? defaultValue;
        }
        catch
        {
            // Fallback: Try to read as JSONL format (one JSON object per line)
            try
            {
                var lines = File.ReadAllLines(path)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();

                if (lines.Count == 0)
                    return defaultValue;

                // Convert JSONL to JSON array
                string jsonArray = "[" + string.Join(",", lines) + "]";
                return JsonSerializer.Deserialize<T>(jsonArray, Options) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Ecrit un fichier JSON de maniere atomique (tmp + rename).
    /// </summary>
    /// <typeparam name="T">Type a serialiser.</typeparam>
    /// <param name="path">Chemin du fichier.</param>
    /// <param name="value">Valeur a serialiser.</param>
    public static void WriteAtomic<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        string tmp = path + ".tmp";
        string json = JsonSerializer.Serialize(value, Options);
        File.WriteAllText(tmp, json);

        if (File.Exists(path))
            File.Delete(path);

        File.Move(tmp, path);
    }
}

