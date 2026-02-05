using EasySave.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace EasySaveTest;

/// <summary>
/// Tests de chargement de la configuration applicative.
/// </summary>
public class AppConfigTest
{
    private ApplicationConfiguration _appSettings = null!;

    /// <summary>
    /// Charge la configuration une seule fois pour la suite.
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        ApplicationConfiguration.Load();
        _appSettings = ApplicationConfiguration.Instance;
    }

    /// <summary>
    /// Verifie que la configuration chargee n'est pas nulle.
    /// </summary>
    [Test]
    public void ConfigIsNotNull()
        => Assert.That(_appSettings, Is.Not.Null);

    /// <summary>
    /// Verifie la presence du chemin de configuration des jobs.
    /// </summary>
    [Test]
    public void ConfigHasJobConfigPath()
        => Assert.That(_appSettings.JobConfigPath, Is.EqualTo("./config"));

    /// <summary>
    /// Verifie la culture configuree.
    /// </summary>
    [Test]
    public void ConfigHasLocalization()
        => Assert.That(_appSettings.Localization, Is.EqualTo("fr-FR"));

    /// <summary>
    /// Verifie le chemin de logs configure.
    /// </summary>
    [Test]
    public void ConfigHasLogPath()
        => Assert.That(_appSettings.LogPath, Is.EqualTo("./log"));
}


