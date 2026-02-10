using EasyLog;
using EasySave.Infrastructure.Configuration;

namespace EasySave.Application.Services;

/// <summary>
///     Log writer that resolves the concrete logger based on current configuration.
/// </summary>
public sealed class ConfigurableLogWriter<T>
{
    private readonly string _logDirectory;
    private readonly object _sync = new();
    private JsonLogger<T>? _jsonLogger;
    private XmlLogger<T>? _xmlLogger;

    public ConfigurableLogWriter(string logDirectory)
    {
        if (string.IsNullOrWhiteSpace(logDirectory))
            throw new ArgumentException("Log directory cannot be empty.", nameof(logDirectory));

        _logDirectory = logDirectory;
    }

    public void Log(T content)
    {
        ResolveLogger().Log(content);
    }

    private AbstractLogger<T> ResolveLogger()
    {
        var logType = NormalizeLogType(ApplicationConfiguration.Instance.LogType);

        lock (_sync)
        {
            if (logType == "xml")
                return _xmlLogger ??= new XmlLogger<T>(_logDirectory);

            return _jsonLogger ??= new JsonLogger<T>(_logDirectory);
        }
    }

    private static string NormalizeLogType(string? logType)
    {
        if (string.IsNullOrWhiteSpace(logType))
            return "json";

        var normalized = logType.Trim().ToLowerInvariant();
        return normalized == "xml" ? "xml" : "json";
    }
}