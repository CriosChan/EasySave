using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Utils;

public static class JsonFile
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

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
            return defaultValue;
        }
    }

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
