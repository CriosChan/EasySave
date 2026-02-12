using EasySave.Core.Contracts;

namespace EasySave.Services;

/// <summary>
///     Log writer that routes entries based on current user preferences.
/// </summary>
public sealed class ConfigurableLogWriter<T> : ILogWriter<T>
{
    private readonly IUserPreferences _preferences;
    private readonly ILogWriter<T> _jsonWriter;
    private readonly ILogWriter<T> _xmlWriter;

    public ConfigurableLogWriter(IUserPreferences preferences, ILogWriter<T> jsonWriter, ILogWriter<T> xmlWriter)
    {
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        _jsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        _xmlWriter = xmlWriter ?? throw new ArgumentNullException(nameof(xmlWriter));
    }

    public void Log(T entry)
    {
        var logType = _preferences.LogType;
        if (string.Equals(logType, "xml", StringComparison.OrdinalIgnoreCase))
        {
            _xmlWriter.Log(entry);
            return;
        }

        _jsonWriter.Log(entry);
    }
}
