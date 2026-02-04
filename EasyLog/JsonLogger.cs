using System.Text.Json;

namespace EasyLog;

public class JsonLogger<T>(string logDirectory) : AbstractLogger<T>(logDirectory, "json")
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = false };

    protected override string Serialize(T log)
    {
        return JsonSerializer.Serialize(log, _options);
    }
    
    public override void Log(T content)
    {
        WriteLogFile(content);
    }
}