using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace EasySave.Data.Configuration
{
    /// <summary>
    ///     Loads and exposes application configuration (read/write).
    /// </summary>
    public sealed class ApplicationConfiguration
    {
        // Singleton instance
        private static ApplicationConfiguration _instance;
        private static readonly object _lock = new();

        // Properties
        public string LogPath { get; set; } = "./log";
        public string JobConfigPath { get; set; } = "./config";
        public string Localization { get; set; } = "";

        public string LogType
        {
            get;
            set
            {
                field = value;
                Save();
            }
        } = "json";

        public string[] BusinessSoftwareProcessNames
        {
            get;
            set
            {
                field = value
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                Save(); // Automatically save when BusinessSoftwareProcessNames is set
            }
        } = Array.Empty<string>();
        public string LogType { get; init; } = "json"; // Default log type format
        public List<string> ExtensionToCrypt { get; init; } = [];

        // Property to hold the configuration file path but not serialized
        [JsonIgnore] // Ensure to ignore this property
        public string ConfigFile { get; set; } = "appsettings.json";

        /// <summary>
        ///     Private constructor to create the singleton object.
        /// </summary>
        private ApplicationConfiguration() {}

        /// <summary>
        ///     Loads configuration from a JSON file and returns the singleton instance.
        /// </summary>
        /// <param name="configFile">Configuration file name.</param>
        /// <returns>Loaded configuration.</returns>
        public static ApplicationConfiguration Load(string configFile = "appsettings.json")
        {
            // Double-checked locking for thread safety
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var configuration = new ConfigurationBuilder()
                            .SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile(configFile, optional: false, reloadOnChange: true)
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
            }
            return _instance;
        }

        /// <summary>
        ///     Saves the current configuration to the specified JSON file.
        /// </summary>
        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ConfigFile), json);
        }
    }
}
