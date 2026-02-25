using EasyLog;
using EasySave.Data.Configuration;
using EasySave.Models.Data.Configuration;

namespace EasySave.Models.Logger;

/// <summary>
///     Log writer that routes entries based on current user preferences.
/// </summary>
public sealed class ConfigurableLogWriter<T>
{
    private readonly AbstractLogger<T> _logger;

    public ConfigurableLogWriter()
    {
        var config = ApplicationConfiguration.Load();
        var logType = config.LogType;
        if (string.Equals(logType, "xml", StringComparison.OrdinalIgnoreCase))
        {
            _logger = new XmlLogger<T>(config.LogPath);
            return;
        }

        _logger = new JsonLogger<T>(config.LogPath);
    }

    public void Log(T entry)
    {
        ApplicationConfiguration instance = ApplicationConfiguration.Load();
        if (instance.RoutingType is RoutingType.Local or RoutingType.LocalCentral)
        {
            Console.WriteLine(entry);
            _logger.Log(entry);
        }

        if (instance.RoutingType is RoutingType.Central or RoutingType.LocalCentral)
        {
            NetworkLog.Instance.Log(entry);
        }
    }
}