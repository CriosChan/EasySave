using System.Text.Json;

namespace EasyLog;

public class JsonLogger<T>(string logDirectory) : AbstractLogger<T>(logDirectory, "json")
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    protected override List<T> Deserialize(string fileContent)
    { 
        return JsonSerializer.Deserialize<List<T>>(fileContent) ?? [];
    }

    protected override string Serialize(List<T> logs)
    {
        return JsonSerializer.Serialize(logs, _options);
    }
    
    public override void Log(T content)
    {
        List<T> logs = ReadLogFile();
        logs.Add(content);
        WriteLogFile(logs);
    }
}