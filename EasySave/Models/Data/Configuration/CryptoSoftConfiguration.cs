using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace EasySave.Models.Data.Configuration;

/// <summary>
///     Loads and exposes application configuration (read/write).
/// </summary>
public sealed class CryptoSoftConfiguration
{
    // Singleton instance
    private static CryptoSoftConfiguration _instance;
    private static readonly object _lock = new();

    /// <summary>
    ///     Private constructor to create the singleton object.
    /// </summary>
    private CryptoSoftConfiguration()
    {
    }

    // Properties
    public string Key {
        get;
        set
        {
            field = value;
            Save();
        }
    } = "value";
    
    // Property to hold the configuration file path but not serialized
    [JsonIgnore] // Ensure to ignore this property
    public string ConfigFile { get; set; } = "Tools/appsettings.json";

    /// <summary>
    ///     Loads configuration from a JSON file and returns the singleton instance.
    /// </summary>
    /// <param name="configFile">Configuration file name.</param>
    /// <returns>Loaded configuration.</returns>
    public static CryptoSoftConfiguration Load(string configFile = "Tools/appsettings.json")
    {
        // Double-checked locking for thread safety
        if (_instance == null)
            lock (_lock)
            {
                if (_instance == null)
                {
                    // Vérifiez si le fichier de configuration existe déjà
                    string filePath = Path.Combine(AppContext.BaseDirectory, configFile);
                    if (!File.Exists(filePath))
                    {
                        // Créez le fichier avec juste {}
                        File.WriteAllText(filePath, "{}");
                    }

                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile(configFile, false, true)
                        .Build();

                    // Create or get the configuration object
                    _instance = new CryptoSoftConfiguration
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