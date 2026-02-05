namespace EasySaveTest;

public class AppConfigTest
{
    private EasySave.ApplicationConfiguration _appSettings = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        EasySave.ApplicationConfiguration.Load();
        _appSettings = EasySave.ApplicationConfiguration.Instance;
    }

    [Test]
    public void ConfigIsNotNull()
        => Assert.That(_appSettings, Is.Not.Null);

    [Test]
    public void ConfigHasJobConfigPath()
        => Assert.That(_appSettings.JobConfigPath, Is.EqualTo("./config"));

    [Test]
    public void ConfigHasLocalization()
        => Assert.That(_appSettings.Localization, Is.EqualTo("fr-FR"));

    [Test]
    public void ConfigHasLogPath()
        => Assert.That(_appSettings.LogPath, Is.EqualTo("./log"));
}