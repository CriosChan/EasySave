using EasySave.Data.Configuration;

namespace EasySaveTest;

/// <summary>
///     Tests for application configuration loading.
/// </summary>
public class AppConfigTest
{
    private ApplicationConfiguration _appSettings = null!;

    /// <summary>
    ///     Loads configuration once for the rest of the tests.
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        _appSettings = ApplicationConfiguration.Load();
    }

    /// <summary>
    ///     Verifies that the loaded configuration is not null.
    /// </summary>
    [Test]
    public void ConfigIsNotNull()
    {
        Assert.That(_appSettings, Is.Not.Null);
    }

    /// <summary>
    ///     Verifies the configured job config path.
    /// </summary>
    [Test]
    public void ConfigHasJobConfigPath()
    {
        Assert.That(_appSettings.JobConfigPath, Is.EqualTo("./config"));
    }

    /// <summary>
    ///     Verifies the configured culture.
    /// </summary>
    [Test]
    public void ConfigHasLocalization()
    {
        Assert.That(_appSettings.Localization, Is.EqualTo("fr-FR"));
    }

    /// <summary>
    ///     Verifies the configured log path.
    /// </summary>
    [Test]
    public void ConfigHasLogPath()
    {
        Assert.That(_appSettings.LogPath, Is.EqualTo("./log"));
    }

    /// <summary>
    ///     Verifies the configured business software process names list.
    /// </summary>
    [Test]
    public void ConfigHasBusinessSoftwareProcessNames()
    {
        Assert.That(_appSettings.BusinessSoftwareProcessNames, Is.Not.Null);
        Assert.That(_appSettings.BusinessSoftwareProcessNames.Length, Is.EqualTo(0));
    }

    /// <summary>
    ///     Verifies the legacy configured business software process name.
    /// </summary>
    [Test]
    public void ConfigHasLegacyBusinessSoftwareProcessName()
    {
        Assert.That(_appSettings.BusinessSoftwareProcessName, Is.EqualTo(string.Empty));
    }
}
