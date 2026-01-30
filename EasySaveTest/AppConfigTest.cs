using Microsoft.Extensions.Configuration;

namespace EasySaveTest;

public class AppConfigTest
{
    private EasySave.ApplicationConfiguration _appSettings = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        _appSettings = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build()
            .Get<EasySave.ApplicationConfiguration>()!;
    }

    [Test]
    public void ConfigIsNotNull()
        => Assert.That(_appSettings, Is.Not.Null);

    [Test]
    public void ConfigHasJobConfigPath()
        => Assert.That(_appSettings.jobConfigPath, Is.EqualTo("/config"));

    [Test]
    public void ConfigHasLocalization()
        => Assert.That(_appSettings.localization, Is.EqualTo("fr-FR"));

    [Test]
    public void ConfigHasLogPath()
        => Assert.That(_appSettings.logPath, Is.EqualTo("/log"));
}