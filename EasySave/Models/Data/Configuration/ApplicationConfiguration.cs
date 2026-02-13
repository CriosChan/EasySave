using Microsoft.Extensions.Configuration;

namespace EasySave.Data.Configuration;

/// <summary>
///     Loads and exposes application configuration (read-only at runtime).
/// </summary>
public sealed class ApplicationConfiguration
{
    /// <summary>
    ///     Log directory configuration (can be relative).
    /// </summary>
    public string LogPath { get; init; } = "./log"; // Default path for log files

    /// <summary>
    ///     Job configuration directory (can be relative).
    /// </summary>
    public string JobConfigPath { get; init; } = "./config"; // Default path for job configuration files

    /// <summary>
    ///     Default localization (e.g., "fr-FR").
    /// </summary>
    public string Localization { get; init; } = ""; // Default localization setting

    public string LogType { get; init; } = "json"; // Default log type format

    /// <summary>
    ///     Process names of business software that block backup execution when running.
    ///     Example values: ["CalculatorApp", "notepad", "notepad.exe"].
    /// </summary>
    public string[] BusinessSoftwareProcessNames { get; init; } = [];

    /// <summary>
    ///     Loads configuration from a JSON file.
    /// </summary>
    /// <param name="configFile">Configuration file name.</param>
    /// <returns>Loaded configuration.</returns>
    public static ApplicationConfiguration Load(string configFile = "appsettings.json")
    {
        var configuration = new ConfigurationBuilder()
            // Use the executable directory so the config is found even if the app is started
            // from a different working directory.
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(configFile, false, true) // Load the specified JSON configuration file
            .Build();

        return configuration.Get<ApplicationConfiguration>() ??
               new ApplicationConfiguration(); // Return the loaded configuration or a new instance
    }
}
