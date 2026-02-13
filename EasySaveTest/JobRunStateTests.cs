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
    public void AllStates_AreDefined()
    {
        var states = Enum.GetValues(typeof(JobRunState));

        Assert.That(states.Length, Is.EqualTo(4));
    }

    [Test]
    public void CanAssignInactive()
    {
        JobRunState state = JobRunState.Inactive;

        Assert.That(state, Is.EqualTo(JobRunState.Inactive));
    }

    [Test]
    public void CanAssignActive()
    {
        JobRunState state = JobRunState.Active;

        Assert.That(state, Is.EqualTo(JobRunState.Active));
    }

    [Test]
    public void CanAssignCompleted()
    {
        JobRunState state = JobRunState.Completed;

        Assert.That(state, Is.EqualTo(JobRunState.Completed));
    }

    [Test]
    public void CanAssignFailed()
    {
        JobRunState state = JobRunState.Failed;

        Assert.That(state, Is.EqualTo(JobRunState.Failed));
    }
}

