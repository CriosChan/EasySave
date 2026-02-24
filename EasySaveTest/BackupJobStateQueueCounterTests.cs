using EasySave.Models.State;

namespace EasySaveTest;

/// <summary>
///     Tests for the two new queue-counter properties added to <see cref="BackupJobState"/>.
/// </summary>
public class BackupJobStateQueueCounterTests
{
    [Test]
    public void RemainingPriorityFiles_DefaultsToZero()
    {
        var state = new BackupJobState();

        Assert.That(state.RemainingPriorityFiles, Is.EqualTo(0));
    }

    [Test]
    public void RemainingStandardFiles_DefaultsToZero()
    {
        var state = new BackupJobState();

        Assert.That(state.RemainingStandardFiles, Is.EqualTo(0));
    }

    [Test]
    public void RemainingPriorityFiles_CanBeSet()
    {
        var state = new BackupJobState { RemainingPriorityFiles = 7 };

        Assert.That(state.RemainingPriorityFiles, Is.EqualTo(7));
    }

    [Test]
    public void RemainingStandardFiles_CanBeSet()
    {
        var state = new BackupJobState { RemainingStandardFiles = 13 };

        Assert.That(state.RemainingStandardFiles, Is.EqualTo(13));
    }

    [Test]
    public void BothCounters_CanBeSetIndependently()
    {
        var state = new BackupJobState
        {
            RemainingPriorityFiles = 3,
            RemainingStandardFiles = 9
        };

        Assert.Multiple(() =>
        {
            Assert.That(state.RemainingPriorityFiles, Is.EqualTo(3));
            Assert.That(state.RemainingStandardFiles, Is.EqualTo(9));
        });
    }

    [Test]
    public void BothCounters_AreIndependentOfOtherProperties()
    {
        var state = new BackupJobState
        {
            TotalFiles = 12,
            RemainingFiles = 12,
            RemainingPriorityFiles = 4,
            RemainingStandardFiles = 8
        };

        Assert.Multiple(() =>
        {
            Assert.That(state.TotalFiles, Is.EqualTo(12));
            Assert.That(state.RemainingFiles, Is.EqualTo(12));
            Assert.That(state.RemainingPriorityFiles, Is.EqualTo(4));
            Assert.That(state.RemainingStandardFiles, Is.EqualTo(8));
        });
    }
}

