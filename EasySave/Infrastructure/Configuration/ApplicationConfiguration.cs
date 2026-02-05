using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasySave.Infrastructure.Configuration;

/// <summary>
/// Loads and exposes application configuration.
/// </summary>
public class ApplicationConfiguration
{
    private static ApplicationConfiguration? _instance;
    private static readonly object _lock = new();
    private string _configFile = "appsettings.json";

    /// <summary>
    /// Loaded configuration instance.
    /// </summary>
    public static ApplicationConfiguration Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("ApplicationConfiguration has not been loaded.");
            return _instance;
        }
    }

    private string _logPath = "";
    public string LogPath
    {
        get => _logPath;
        set
        {
            if (_logPath != value)
            {
                _logPath = value;
                Save(nameof(LogPath), value);
            }
        }
    }

    private string _jobConfigPath = "";
    public string JobConfigPath
    {
        get => _jobConfigPath;
        set
        {
            if (_jobConfigPath != value)
            {
                _jobConfigPath = value;
                Save(nameof(JobConfigPath), value);
            }
        }
    }

    private string _localization = "";
    public string Localization
    {
        get => _localization;
        set
        {
            if (_localization != value)
            {
                _localization = value;
                Save(nameof(Localization), value);
            }
        }
    }

    /// <summary>
    /// Initializes an empty configuration (used by the binder).
    /// </summary>
    public ApplicationConfiguration() { }

    /// <summary>
    /// Loads configuration from a JSON file.
    /// </summary>
    /// <param name="configFile">Configuration file name.</param>
    public static void Load(string configFile = "appsettings.json")
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                var configuration = new ConfigurationBuilder()
                    // Use the executable directory so the config is found even if the app is started
                    // from a different working directory.
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .Build();

                _instance = configuration.Get<ApplicationConfiguration>()!;
                _instance._configFile = configFile;
            }
        }
    }

    private void Save(string propertyName, string value)
    {
        lock (_lock)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, _configFile);
            JsonNode? root;
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                root = JsonNode.Parse(json);
                if (root == null)
                    root = new JsonObject();
            }
            else
            {
                root = new JsonObject();
            }

            root[propertyName] = value;
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, root.ToJsonString(options));
        }
    }
}
