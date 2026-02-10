using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace EasySave.Infrastructure.Configuration;

/// <summary>
///     Loads and exposes application configuration.
/// </summary>
public class ApplicationConfiguration
{
    private static ApplicationConfiguration? _instance;
    private static readonly object _lock = new();
    private string _configFile = "appsettings.json";

    /// <summary>
    ///     Initializes an empty configuration (used by the binder).
    /// </summary>
    public ApplicationConfiguration()
    {
    }

    /// <summary>
    ///     Loaded configuration instance.
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

    public string LogPath
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Save(nameof(LogPath), value);
            }
        }
    } = "./log";

    public string JobConfigPath
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Save(nameof(JobConfigPath), value);
            }
        }
    } = "./config";

    public string Localization
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Save(nameof(Localization), value);
            }
        }
    } = "";

    public string LogType
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Save(nameof(LogType), value);
            }
        }
    } = "json";

    /// <summary>
    ///     Loads configuration from a JSON file.
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
                    .AddJsonFile(configFile, false, true)
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