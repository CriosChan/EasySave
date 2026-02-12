using EasySave.Data.Configuration;

namespace EasySaveTest;

/// <summary>
///     Verifies configuration can be loaded without global state.
/// </summary>
public class AppConfigInitTest
{
    /// <summary>
    ///     Ensures Load returns a configuration instance.
    /// </summary>
    [Test]
    public void AppConfigLoad_ReturnsInstance()
    {
        var instance = ApplicationConfiguration.Load();
        Assert.That(instance, Is.Not.Null);
    }
}
