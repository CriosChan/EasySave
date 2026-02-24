using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

/// <summary>
///     Integration tests demonstrating the global priority arbitrator in action.
/// </summary>
[TestFixture]
public class PriorityArbitrationIntegrationTests
{
    private IPriorityArbitrator _arbitrator = null!;

    [SetUp]
    public void SetUp()
    {
        _arbitrator = new GlobalPriorityArbitrator();
    }

    [Test]
    public void Scenario_MultipleJobsWithMixedPriority()
    {
        // Setup: 
        // Job 1: 3 priority, 2 standard
        // Job 2: 2 priority, 3 standard
        // Job 3: 0 priority, 4 standard
        _arbitrator.Initialize(new Dictionary<int, int>
        {
            { 1, 3 },
            { 2, 2 },
            { 3, 0 }
        });

        // Initial state: All standard files blocked (global priority = 5)
        Assert.Multiple(() =>
        {
            Assert.That(_arbitrator.CanProcessStandardFile(1), Is.False);
            Assert.That(_arbitrator.CanProcessStandardFile(2), Is.False);
            Assert.That(_arbitrator.CanProcessStandardFile(3), Is.False); // Even though Job 3 has 0 priority
        });

        // Job 1 processes 1 priority file
        _arbitrator.UpdateGlobalPriorityCount(1, 2); // 1 processed
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(4));
        Assert.That(_arbitrator.CanProcessStandardFile(1), Is.False);

        // Job 2 processes 1 priority file
        _arbitrator.UpdateGlobalPriorityCount(2, 1); // 1 processed
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(3));

        // Job 1 processes all remaining priority files (2 more)
        _arbitrator.UpdateGlobalPriorityCount(1, 0); // All done
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(1));
        Assert.That(_arbitrator.CanProcessStandardFile(1), Is.False); // Still blocked (Job 2 has 1)

        // Job 2 processes final priority file
        _arbitrator.UpdateGlobalPriorityCount(2, 0); // All done
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(0));

        // Now ALL jobs can process standard files
        Assert.Multiple(() =>
        {
            Assert.That(_arbitrator.CanProcessStandardFile(1), Is.True);
            Assert.That(_arbitrator.CanProcessStandardFile(2), Is.True);
            Assert.That(_arbitrator.CanProcessStandardFile(3), Is.True);
        });

        // Jobs complete
        _arbitrator.OnJobCompleted(1);
        _arbitrator.OnJobCompleted(2);
        _arbitrator.OnJobCompleted(3);

        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(0));
    }

    [Test]
    public void Scenario_JobWithNoPriorityFilesStaysBlocked()
    {
        // Setup:
        // Job A: 3 priority, 5 standard
        // Job B: 0 priority, 10 standard
        _arbitrator.Initialize(new Dictionary<int, int>
        {
            { 1, 3 },
            { 2, 0 }
        });

        // Job B (with no priority files) is blocked because Job A has priority files
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.False);

        // Job A processes all priority files
        _arbitrator.UpdateGlobalPriorityCount(1, 0);

        // Now Job B can process its standard files
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.True);
    }

    [Test]
    public void Scenario_ContinuousPriorityFeedWithAntiFamine()
    {
        // Simulate continuous feeding of priority files
        _arbitrator.Initialize(new Dictionary<int, int>
        {
            { 1, 10 }
        });

        // Job 1 starts processing but keeps getting more priority files
        // Stop at i=1 (remaining=1): standard files must stay blocked while priority > 0
        for (int i = 10; i > 1; i--)
        {
            _arbitrator.UpdateGlobalPriorityCount(1, i - 1);
            Assert.That(_arbitrator.CanProcessStandardFile(1), Is.False);
            System.Threading.Thread.Sleep(100); // Simulate processing
        }

        // Process the last priority file
        _arbitrator.UpdateGlobalPriorityCount(1, 0);

        // All priority files done: standard files are now allowed
        Assert.That(_arbitrator.CanProcessStandardFile(1), Is.True); // All priorities done

        // Now it can process standard files
    }

    [Test]
    public void Scenario_PartialCompletion()
    {
        // Setup:
        // Job 1: 2 priority, 3 standard
        // Job 2: 3 priority, 2 standard  
        // Job 3: 1 priority, 4 standard
        _arbitrator.Initialize(new Dictionary<int, int>
        {
            { 1, 2 },
            { 2, 3 },
            { 3, 1 }
        });

        // Job 1 finishes (both priority and standard)
        _arbitrator.UpdateGlobalPriorityCount(1, 0);
        _arbitrator.OnJobCompleted(1);

        // Still blocked - Job 2 has 3 and Job 3 has 1 = 4 remaining
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.False);
        Assert.That(_arbitrator.CanProcessStandardFile(3), Is.False);

        // Job 3 completes priority file
        _arbitrator.UpdateGlobalPriorityCount(3, 0);

        // Still blocked - Job 2 has 3
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.False);

        // Job 2 finishes priority files
        _arbitrator.UpdateGlobalPriorityCount(2, 0);

        // Now all remaining jobs can process standard files
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.True);
    }

    [Test]
    [Timeout(15000)]
    public void Scenario_ConcurrentJobsProcessingPriorities()
    {
        // Simulate 3 jobs processing priority files concurrently
        _arbitrator.Initialize(new Dictionary<int, int>
        {
            { 1, 5 },
            { 2, 4 },
            { 3, 3 }
        });

        var tasks = new[]
        {
            Task.Run(() => ProcessJobPriorities(1, 5)),
            Task.Run(() => ProcessJobPriorities(2, 4)),
            Task.Run(() => ProcessJobPriorities(3, 3))
        };

        Task.WaitAll(tasks);

        // All priority files processed
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(0));

        // All jobs can now process standard files
        Assert.That(_arbitrator.CanProcessStandardFile(1), Is.True);
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.True);
        Assert.That(_arbitrator.CanProcessStandardFile(3), Is.True);
    }

    private void ProcessJobPriorities(int jobId, int priorityCount)
    {
        for (int i = priorityCount; i > 0; i--)
        {
            _arbitrator.UpdateGlobalPriorityCount(jobId, i - 1);
            System.Threading.Thread.Sleep(10); // Simulate processing time
        }
    }
}
