using Microsoft.Extensions.Configuration;

namespace EasySave;

public class ApplicationConfiguration
{
    private static ApplicationConfiguration? _instance;
    private static readonly object _lock = new();

    public static ApplicationConfiguration Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("ApplicationConfiguration has not been loaded.");
            return _instance;
        }
    }

    public string LogPath { get; set; } = "";
    public string JobConfigPath { get; set; } = "";
    public string Localization { get; set; } = "";
    
    public ApplicationConfiguration() { }

    public static void Load(string configFile = "appsettings.json")
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .Build();

                _instance = configuration.Get<ApplicationConfiguration>()!;
            }
        }
    }
}