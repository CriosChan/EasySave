using System.Text.Json;

namespace EasyLog;

public class JsonLogger<T>(string logDirectory) : AbstractLogger<T>(logDirectory, "json")
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    /// <summary>
    /// Serializes the specified log entry of type <typeparamref name="T"/> into a JSON string.
    /// </summary>
    /// <param name="log">The log entry to serialize.</param>
    /// <returns>A JSON string representation of the log entry.</returns>
    /// <remarks>
    /// This method overrides a base class implementation to provide JSON serialization
    /// using the <see cref="JsonSerializer"/> class with specific serialization options.
    /// </remarks>
    protected override string Serialize(T log)
    {
        return JsonSerializer.Serialize(log, _options);
    }
    
    /// <summary>
    /// Logs the specified content of type <typeparamref name="T"/> by writing it to a log file.
    /// </summary>
    /// <param name="content">The content to log.</param>
    /// <remarks>
    /// This method writes to a daily JSON file (one file per day) located under
    /// <see cref="AbstractLogger{T}.LogDirectory"/>.
    ///
    /// The file format is a JSON array so it stays valid JSON while still being updated in real time.
    /// </remarks>
    public override void Log(T content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        Directory.CreateDirectory(LogDirectory);
        string logFilePath = GetLogFilePath(DateTime.Now);

        List<T> logs = ReadExistingLogs(logFilePath);
        logs.Add(content);
        WriteAtomic(logFilePath, logs);
    }

    private List<T> ReadExistingLogs(string logFilePath)
    {
        if (!File.Exists(logFilePath))
            return new List<T>();

        // Preferred format (v1.0+): a JSON array of objects.
        try
        {
            string json = File.ReadAllText(logFilePath);
            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
        }
        catch
        {
            // Backward compatibility: support legacy "JSON Lines" files
            // where each line is a serialized object.
            var logs = new List<T>();
            try
            {
                foreach (string line in File.ReadLines(logFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        if (JsonSerializer.Deserialize<T>(line, _options) is { } item)
                            logs.Add(item);
                    }
                    catch
                    {
                        // Skip malformed lines.
                    }
                }
            }
            catch
            {
                // If reading fails, return an empty list.
            }

            return logs;
        }
    }

    private void WriteAtomic(string logFilePath, List<T> logs)
    {
        string? dir = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        string tmp = logFilePath + ".tmp";
        string json = JsonSerializer.Serialize(logs, _options);
        File.WriteAllText(tmp, json);

        if (File.Exists(logFilePath))
            File.Delete(logFilePath);

        File.Move(tmp, logFilePath);
    }
}