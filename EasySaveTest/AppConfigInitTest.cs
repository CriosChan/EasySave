using EasySave.Infrastructure.Configuration;

namespace EasySaveTest;

/// <summary>
/// Verifie le comportement avant chargement de la configuration.
/// </summary>
public class AppConfigInitTest
{
    /// <summary>
    /// S'assure que l'acces a Instance avant Load declenche une exception.
    /// </summary>
    [Test]
    public void AppConfigThrowExceptionIfInstanceCalledBeforeLoading()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var instance = ApplicationConfiguration.Instance;
        });
    }
}
