using Microsoft.Extensions.Configuration;

namespace EasySave.Infrastructure.Configuration;

/// <summary>
/// Charge et expose la configuration applicative.
/// </summary>
public class ApplicationConfiguration
{
    private static ApplicationConfiguration? _instance;
    private static readonly object _lock = new();

    /// <summary>
    /// Instance chargee de la configuration.
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

    public string LogPath { get; set; } = "";
    public string JobConfigPath { get; set; } = "";
    public string Localization { get; set; } = "";
    
    /// <summary>
    /// Initialise une configuration vide (utilise par le binder).
    /// </summary>
    public ApplicationConfiguration() { }

    /// <summary>
    /// Charge la configuration depuis un fichier JSON.
    /// </summary>
    /// <param name="configFile">Nom du fichier de configuration.</param>
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
            }
        }
    }
}
