using EasyLog;
using EasySave.Data.Configuration;

namespace EasySave.Models.Logger;

/// <summary>
///     Log writer that routes entries based on current user preferences.
/// </summary>
public sealed class ConfigurableLogWriter<T>
{
    private readonly AbstractLogger<T> _logger; // Logger instance to handle log entries

    /// <summary>
    ///     Initializes a new instance of the ConfigurableLogWriter class.
    ///     Loads the application configuration to determine the log type.
    /// </summary>
    public ConfigurableLogWriter()
    {
        var config = ApplicationConfiguration.Load(); // Load application configuration
        var logType = config.LogType; // Get the configured log type

        // Choose the logger based on the configured log type
        if (string.Equals(logType, "xml", StringComparison.OrdinalIgnoreCase))
        {
            _logger = new XmlLogger<T>(config.LogPath); // Use XML logger
            return;
        }

        // Default to JSON logger
        _logger = new JsonLogger<T>(config.LogPath);
    }

    /// <summary>
    ///     Logs an entry based on the current routing preferences.
    /// </summary>
    /// <param name="entry">The log entry to be recorded.</param>
    public void Log(T entry)
    {
        var instance = ApplicationConfiguration.Load(); // Reload config for the current operation

        // Check the routing type and log accordingly
        if (instance.RoutingType is RoutingType.Local
            or RoutingType.LocalCentral) _logger.Log(entry); // Log entry for local or local-central routing

        if (instance.RoutingType is RoutingType.Central or RoutingType.LocalCentral)
            // Send the log entry to the central server
            new Thread(() => NetworkLog.Instance.Log(entry)).Start();
    }
}
