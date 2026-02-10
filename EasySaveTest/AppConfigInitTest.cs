using EasySave.Infrastructure.Configuration;

namespace EasySaveTest;

/// <summary>
///     Verifies behavior before configuration is loaded.
/// </summary>
public class AppConfigInitTest
{
    /// <summary>
    ///     Ensures accessing Instance before Load throws an exception.
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