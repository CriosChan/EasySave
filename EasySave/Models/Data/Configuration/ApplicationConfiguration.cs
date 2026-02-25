using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Models.Data.Configuration;
using Microsoft.Extensions.Configuration;

namespace EasySave.Data.Configuration;

/// <summary>
///     Loads and exposes application configuration (read/write).
/// </summary>
public sealed class ApplicationConfiguration
{
    // Singleton instance
    private static ApplicationConfiguration _instance;
    private static readonly object _lock = new();

    /// <summary>
    ///     Private constructor to create the singleton object.
    /// </summary>
    private ApplicationConfiguration()
    {
    }

    // Properties

    /// <summary>
    ///     Gets or sets the path for log files. Automatically saves the configuration when modified.
    ///     Default is "./log".
    /// </summary>
    public string LogPath
    {
        get;
        set
        {
            field = value; // Assign new value
            Save(); // Save the configuration
        }
    } = "./log";

    /// <summary>
    ///     Gets or sets the path for job configuration files. Automatically saves when modified.
    ///     Default is "./config".
    /// </summary>
    public string JobConfigPath
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = "./config";

    /// <summary>
    ///     Gets or sets the localization settings.
    ///     Automatically saves when modified.
    /// </summary>
    public string Localization
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = "";

    /// <summary>
    ///     Gets or sets the type of log (JSON or XML). Automatically saves when modified.
    ///     Default is "json".
    /// </summary>
    public string LogType
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = "json";

    /// <summary>
    ///     Gets or sets the names of business software processes.
    ///     Automatically saves when set and ensures unique, trimmed, and non-empty names.
    /// </summary>
    public string[] BusinessSoftwareProcessNames
    {
        get;
        set
        {
            field = value
                .Where(name => !string.IsNullOrWhiteSpace(name)) // Filter out empty names
                .Select(name => name.Trim()) // Trim whitespace
                .Distinct(StringComparer.OrdinalIgnoreCase) // Ensure uniqueness
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase) // Sort alphabetically
                .ToArray();
            Save(); // Automatically save when modified
        }
    } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets the list of file extensions to be encrypted.
    ///     Automatically saves when modified.
    /// </summary>
    public List<string> ExtensionToCrypt
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = new();

    /// <summary>
    ///     Gets or sets the list of file extensions that should be treated as priority during backup.
    ///     Automatically saves when modified.
    /// </summary>
    public List<string> PriorityExtensions
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = new();

    /// <summary>
    ///     Gets or sets the EasySaveServer's IP address. Automatically saves when modified.
    ///     Default is "127.0.0.1" (localhost).
    /// </summary>
    public string EasySaveServerIp
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = "127.0.0.1";

    /// <summary>
    ///     Gets or sets the EasySaveServer's port number. Automatically saves when modified.
    ///     Default is 5000.
    /// </summary>
    public int EasySaveServerPort
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = 5000;

    /// <summary>
    ///     Gets or sets the routing type for logs: local only, server only, or both. Automatically saves when modified.
    ///     Default is RoutingType.Local.
    /// </summary>
    public RoutingType RoutingType
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = RoutingType.Local;

    /// <summary>
    ///     Gets or sets the configuration file path. Not serialized.
    ///     Default is "appsettings.json".
    /// </summary>
    [JsonIgnore] // Ensure this property is ignored during JSON serialization
    public string ConfigFile
    {
        get;
        set
        {
            field = value;
            Save();
        }
    } = "appsettings.json";

    /// <summary>
    ///     Loads configuration from a JSON file and returns the singleton instance.
    /// </summary>
    /// <param name="configFile">Configuration file name.</param>
    /// <returns>Loaded configuration.</returns>
    public static ApplicationConfiguration Load(string configFile = "appsettings.json")
    {
        // Double-checked locking for thread safety
        if (_instance == null)
            lock (_lock)
            {
                if (_instance == null)
                {
                    var filePath = Path.Combine(AppContext.BaseDirectory, configFile);
                    if (!File.Exists(filePath))
                        // Create the file with just {}
                        File.WriteAllText(filePath, "{}");

                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile(configFile, false, true)
                        .Build();

                    // Create or get the configuration object
                    _instance = new ApplicationConfiguration
                    {
                        ConfigFile = configFile // Set the config file path
                    };

                    // Bind values from the loaded configuration
                    configuration.Bind(_instance);
                }
            }

        return _instance;
    }

    /// <summary>
    ///     Saves the current configuration to the specified JSON file.
    /// </summary>
    public void Save()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ConfigFile), json);
    }
}