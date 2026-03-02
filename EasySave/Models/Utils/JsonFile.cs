using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace EasySave.Models.Utils;

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
        var json = JsonSerializer.Serialize(value, Options);
        var tmp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            File.WriteAllText(tmp, json);
            const int maxAttempts = 8;

            for (var attempt = 1; ; attempt++)
                try
                {
                    File.Move(tmp, path, overwrite: true); // Atomic overwrite — avoids delete+move race window
                    return;
                }
                catch (Exception ex) when (IsRetryableWriteException(ex) && attempt < maxAttempts)
                {
                    // Retry transient file-lock races (AV/indexer/parallel readers).
                    Thread.Sleep(attempt * 25);
                }
        }
        finally
        {
            try
            {
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
            catch
            {
                // Ignore cleanup failures for temp files.
            }
        }
    }

    private static bool IsRetryableWriteException(Exception exception)
    {
        return exception is IOException or UnauthorizedAccessException;
    }
}
