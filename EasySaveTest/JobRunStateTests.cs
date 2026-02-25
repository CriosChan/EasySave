using EasySave.Models.State;

namespace EasySaveTest;

public class JobRunStateTests
{
    [Test]
    public void Inactive_HasCorrectValue()
    {
        Assert.That((int)JobRunState.Inactive, Is.EqualTo(0));
    }

    [Test]
    public void Active_HasCorrectValue()
    {
        Assert.That((int)JobRunState.Active, Is.EqualTo(1));
    }

    [Test]
    public void Completed_HasCorrectValue()
    {
        Assert.That((int)JobRunState.Completed, Is.EqualTo(2));
    }

    [Test]
    public void Failed_HasCorrectValue()
    {
        Assert.That((int)JobRunState.Failed, Is.EqualTo(3));
    }

    [Test]
    public void Paused_HasCorrectValue()
    {
        Assert.That((int)JobRunState.Paused, Is.EqualTo(4));
    }

    [Test]
    public void Stopped_HasCorrectValue()
    {
        Assert.That((int)JobRunState.Stopped, Is.EqualTo(5));
    }

    [Test]
    public void WaitingPriority_HasCorrectValue()
    {
        Assert.That((int)JobRunState.WaitingPriority, Is.EqualTo(6));
    }

    [Test]
    public void WaitingLargeFile_HasCorrectValue()
    {
        Assert.That((int)JobRunState.WaitingLargeFile, Is.EqualTo(7));
    }

    [Test]
    public void PausedBusinessSoftware_HasCorrectValue()
    {
        Assert.That((int)JobRunState.PausedBusinessSoftware, Is.EqualTo(8));
    }

    [Test]
    public void AllStates_AreDefined()
    {
        var states = Enum.GetValues(typeof(JobRunState));

        Assert.That(states.Length, Is.EqualTo(9));
    }

    [Test]
    public void CanAssignInactive()
    {
        var state = JobRunState.Inactive;

        Assert.That(state, Is.EqualTo(JobRunState.Inactive));
    }

    [Test]
    public void CanAssignActive()
    {
        var state = JobRunState.Active;

        Assert.That(state, Is.EqualTo(JobRunState.Active));
    }

    [Test]
    public void CanAssignCompleted()
    {
        var state = JobRunState.Completed;

        Assert.That(state, Is.EqualTo(JobRunState.Completed));
    }

    [Test]
    public void CanAssignFailed()
    {
        var state = JobRunState.Failed;

        Assert.That(state, Is.EqualTo(JobRunState.Failed));
    }

    [Test]
    public void CanAssignPaused()
    {
        var state = JobRunState.Paused;

        Assert.That(state, Is.EqualTo(JobRunState.Paused));
    }

    [Test]
    public void CanAssignStopped()
    {
        var state = JobRunState.Stopped;

        Assert.That(state, Is.EqualTo(JobRunState.Stopped));
    }

    [Test]
    public void CanAssignWaitingPriority()
    {
        var state = JobRunState.WaitingPriority;

        Assert.That(state, Is.EqualTo(JobRunState.WaitingPriority));
    }

    [Test]
    public void CanAssignWaitingLargeFile()
    {
        var state = JobRunState.WaitingLargeFile;

        Assert.That(state, Is.EqualTo(JobRunState.WaitingLargeFile));
    }

    [Test]
    public void CanAssignPausedBusinessSoftware()
    {
        var state = JobRunState.PausedBusinessSoftware;

        Assert.That(state, Is.EqualTo(JobRunState.PausedBusinessSoftware));
    }
}