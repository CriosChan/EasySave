namespace EasySaveTest;

public class AppConfigInitTest
{
    [Test]
    public void AppConfigThrowExceptionIfInstanceCalledBeforeLoading()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var instance = EasySave.ApplicationConfiguration.Instance;
        });
    }
}