using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Infrastructure.IO;

/// <summary>
///     JSON read/write helpers with error tolerance.
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
    ///     Reads a JSON file or returns a default value if missing/invalid.
    /// </summary>
    /// <typeparam name="T">Deserialization type.</typeparam>
    /// <param name="path">File path.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <returns>Deserialized instance or default value.</returns>
    public static T ReadOrDefault<T>(string path, T defaultValue) where T : class
    {
        if (!File.Exists(path))
            return defaultValue;

        try
        {
            var json = File.ReadAllText(path);
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
                var jsonArray = "[" + string.Join(",", lines) + "]";
                return JsonSerializer.Deserialize<T>(jsonArray, Options) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    ///     Writes a JSON file atomically (tmp + rename).
    /// </summary>
    /// <typeparam name="T">Type to serialize.</typeparam>
    /// <param name="path">File path.</param>
    /// <param name="value">Value to serialize.</param>
    public static void WriteAtomic<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        var tmp = path + ".tmp";
        var json = JsonSerializer.Serialize(value, Options);
        File.WriteAllText(tmp, json);

        if (File.Exists(path))
            File.Delete(path);

        File.Move(tmp, path);
    }
}