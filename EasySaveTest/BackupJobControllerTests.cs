
namespace EasySaveTest;

/// <summary>
///     Unit tests for BackupJobController business-software pause notification.
/// </summary>
public class BackupJobControllerTests
{
    [Test]
    public void PausedByBusiness_DefaultValue_IsFalse()
    {
        var controller = new BackupJobController();

        Assert.That(controller.PausedByBusiness, Is.False);
    }

    [Test]
    public void NotifyBusinessSoftwarePause_SetTrue_PausedByBusinessIsTrue()
    {
        var controller = new BackupJobController();

        controller.NotifyBusinessSoftwarePause(true);

        Assert.That(controller.PausedByBusiness, Is.True);
    }

    [Test]
    public void NotifyBusinessSoftwarePause_SetFalse_PausedByBusinessIsFalse()
    {
        var controller = new BackupJobController();
        controller.NotifyBusinessSoftwarePause(true);

        controller.NotifyBusinessSoftwarePause(false);

        Assert.That(controller.PausedByBusiness, Is.False);
    }

    [Test]
    public void BusinessSoftwarePauseChanged_RaisedWhenPausedByBusinessChanges()
    {
        var controller = new BackupJobController();
        var eventCount = 0;
        controller.BusinessSoftwarePauseChanged += (_, _) => eventCount++;

        controller.NotifyBusinessSoftwarePause(true);
        controller.NotifyBusinessSoftwarePause(false);

        Assert.That(eventCount, Is.EqualTo(2));
    }

    [Test]
    public void BusinessSoftwarePauseChanged_PassesTrueValueOnPause()
    {
        var controller = new BackupJobController();
        bool? receivedValue = null;
        controller.BusinessSoftwarePauseChanged += (sender, _) =>
        {
            if (sender is BackupJobController c)
                receivedValue = c.PausedByBusiness;
        };

        controller.NotifyBusinessSoftwarePause(true);

        Assert.That(receivedValue, Is.True);
    }

    [Test]
    public void BusinessSoftwarePauseChanged_PassesFalseValueOnResume()
    {
        var controller = new BackupJobController();
        controller.NotifyBusinessSoftwarePause(true);

        bool? receivedValue = null;
        controller.BusinessSoftwarePauseChanged += (sender, _) =>
        {
            if (sender is BackupJobController c)
                receivedValue = c.PausedByBusiness;
        };

        controller.NotifyBusinessSoftwarePause(false);

        Assert.That(receivedValue, Is.False);
    }
}

