using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

/// <summary>
///     Unit tests for ParallelJobOrchestrator.
/// </summary>
[TestFixture]
public class ParallelJobOrchestratorTests
{
    private IBackupExecutionEngine _executionEngine = null!;
    private ParallelJobOrchestrator _orchestrator = null!;

    [SetUp]
    public void SetUp()
    {
        _executionEngine = new BackupExecutionEngine();
        _orchestrator = new ParallelJobOrchestrator(_executionEngine);
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenEngineIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ParallelJobOrchestrator(null!));
    }

    [Test]
    public void ActiveJobCount_ReturnsZero_WhenNoJobsAreRunning()
    {
        Assert.That(_orchestrator.ActiveJobCount, Is.EqualTo(0));
    }

    [Test]
    public void CompletedJobCount_ReturnsZero_Initially()
    {
        Assert.That(_orchestrator.CompletedJobCount, Is.EqualTo(0));
    }

    [Test]
    public void FailedJobCount_ReturnsZero_Initially()
    {
        Assert.That(_orchestrator.FailedJobCount, Is.EqualTo(0));
    }

    [Test]
    public void PendingJobCount_ReturnsZero_Initially()
    {
        Assert.That(_orchestrator.PendingJobCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAllAsync_ThrowsArgumentNullException_WhenJobsIsNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _orchestrator.ExecuteAllAsync(null!));
    }

    [Test]
    public async Task ExecuteAllAsync_ReturnsEmptyResult_WhenJobListIsEmpty()
    {
        var result = await _orchestrator.ExecuteAllAsync(new List<BackupJob>());

        Assert.Multiple(() =>
        {
            Assert.That(result.CompletedCount, Is.EqualTo(0));
            Assert.That(result.FailedCount, Is.EqualTo(0));
            Assert.That(result.CancelledCount, Is.EqualTo(0));
            Assert.That(result.WasStoppedByBusinessSoftware, Is.False);
        });
    }

    [Test]
    public void GetJobState_ReturnsNull_WhenJobIsNotTracked()
    {
        var state = _orchestrator.GetJobState(999);
        Assert.That(state, Is.Null);
    }

    [Test]
    public void ClearStates_RemovesAllTrackedStates()
    {
        _orchestrator.ClearStates();
        Assert.That(_orchestrator.PendingJobCount, Is.EqualTo(0));
    }

    [Test]
    public void OrchestrationResult_HasCorrectProperties()
    {
        var result = new OrchestrationResult(5, 2, 1, false);

        Assert.Multiple(() =>
        {
            Assert.That(result.CompletedCount, Is.EqualTo(5));
            Assert.That(result.FailedCount, Is.EqualTo(2));
            Assert.That(result.CancelledCount, Is.EqualTo(1));
            Assert.That(result.WasStoppedByBusinessSoftware, Is.False);
        });
    }

    [Test]
    public void JobExecutionState_HasAllExpectedValues()
    {
        var states = Enum.GetValues<JobExecutionState>();

        Assert.Multiple(() =>
        {
            Assert.That(states, Contains.Item(JobExecutionState.Pending));
            Assert.That(states, Contains.Item(JobExecutionState.Active));
            Assert.That(states, Contains.Item(JobExecutionState.Completed));
            Assert.That(states, Contains.Item(JobExecutionState.Failed));
            Assert.That(states, Contains.Item(JobExecutionState.Cancelled));
            Assert.That(states, Contains.Item(JobExecutionState.StoppedByBusinessSoftware));
            Assert.That(states, Contains.Item(JobExecutionState.Skipped));
        });
    }
}

