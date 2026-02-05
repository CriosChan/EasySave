using System.Text.Json;

namespace EasyLog;

/// <summary>
/// Logger that serializes entries to JSON.
/// </summary>
public class JsonLogger<T>(string logDirectory) : AbstractLogger<T>(logDirectory, "json")
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = false };

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
    /// This method overrides the base class implementation and calls the
    /// <see cref="AbstractLogger{T}.WriteLogFile"/> method to handle the actual logging.
    /// The content is passed to the <see cref="AbstractLogger{T}.WriteLogFile"/> method for processing.
    /// </remarks>
    public override void Log(T content)
    {
        WriteLogFile(content);
    }
}
