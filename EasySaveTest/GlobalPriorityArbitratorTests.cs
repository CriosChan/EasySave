using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

/// <summary>
///     Unit tests for GlobalPriorityArbitrator.
/// </summary>
[TestFixture]
public class GlobalPriorityArbitratorTests
{
    private IPriorityArbitrator _arbitrator = null!;

    [SetUp]
    public void SetUp()
    {
        _arbitrator = new GlobalPriorityArbitrator();
    }

    [Test]
    public void Initialize_SetsGlobalPriorityCount()
    {
        // Arrange
        var jobCounts = new Dictionary<int, int> { { 1, 5 }, { 2, 3 } };

        // Act
        _arbitrator.Initialize(jobCounts);

        // Assert
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(8));
    }

    [Test]
    public void Initialize_WithNullDictionary_ThrowsArgumentNullException()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => _arbitrator.Initialize(null!));
    }

    [Test]
    public void CanProcessStandardFile_ReturnsFalse_WhenPriorityFilesRemain()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 5 } });

        // Act & Assert
        Assert.That(_arbitrator.CanProcessStandardFile(1), Is.False);
    }

    [Test]
    public void CanProcessStandardFile_ReturnsTrue_WhenNoPriorityFilesRemain()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 0 } });

        // Act & Assert
        Assert.That(_arbitrator.CanProcessStandardFile(1), Is.True);
    }

    [Test]
    public void UpdateGlobalPriorityCount_DecrementsGlobalCount()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 5 } });

        // Act
        _arbitrator.UpdateGlobalPriorityCount(1, 4);

        // Assert
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(4));
    }

    [Test]
    public void UpdateGlobalPriorityCount_WithZero_AllowsStandardFiles()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 3 }, { 2, 2 } });

        // Act
        _arbitrator.UpdateGlobalPriorityCount(1, 0);

        // Assert
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.False); // Job 2 still has 2 priority files
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(2));
    }

    [Test]
    public void CanProcessStandardFile_ReturnsTrue_WhenAllPriorityFilesAreZero()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 2 }, { 2, 3 } });

        // Act
        _arbitrator.UpdateGlobalPriorityCount(1, 0);
        _arbitrator.UpdateGlobalPriorityCount(2, 0);

        // Assert
        Assert.That(_arbitrator.CanProcessStandardFile(1), Is.True);
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.True);
    }

    [Test]
    public void OnJobCompleted_RemovesJobFromTracking()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 5 }, { 2, 3 } });

        // Act
        _arbitrator.OnJobCompleted(1);

        // Assert
        // After removing job 1, global count should be just from job 2
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(3));
    }

    [Test]
    public void AntiFamine_AllowsStandardFilesAfterTimeout()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 5 } });

        // Act: Call CanProcessStandardFile multiple times with delay
        var firstCall = _arbitrator.CanProcessStandardFile(1);
        System.Threading.Thread.Sleep(11000); // Wait longer than the 10-second timeout
        var secondCall = _arbitrator.CanProcessStandardFile(1);

        // Assert
        Assert.That(firstCall, Is.False);
        Assert.That(secondCall, Is.True); // Should be allowed due to anti-famine
    }

    [Test]
    [Timeout(15000)]
    public void ThreadSafety_ConcurrentUpdates()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 100 }, { 2, 100 } });
        var task1 = Task.Run(() =>
        {
            for (int i = 100; i > 0; i--)
            {
                _arbitrator.UpdateGlobalPriorityCount(1, i - 1);
                System.Threading.Thread.Sleep(1);
            }
        });

        var task2 = Task.Run(() =>
        {
            for (int i = 100; i > 0; i--)
            {
                _arbitrator.UpdateGlobalPriorityCount(2, i - 1);
                System.Threading.Thread.Sleep(1);
            }
        });

        // Act
        Task.WaitAll(task1, task2);

        // Assert
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(0));
    }

    [Test]
    public void MultipleJobs_BlocksWhenAnyJobHasPriority()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int> { { 1, 0 }, { 2, 5 } });

        // Act & Assert
        Assert.That(_arbitrator.CanProcessStandardFile(1), Is.False); // Blocked because job 2 has 5 priority files
        Assert.That(_arbitrator.CanProcessStandardFile(2), Is.False); // Job 2 itself is blocked
    }

    [Test]
    public void GetGlobalPriorityFilesRemaining_ReturnsZero_WhenEmpty()
    {
        // Arrange
        _arbitrator.Initialize(new Dictionary<int, int>());

        // Act & Assert
        Assert.That(_arbitrator.GetGlobalPriorityFilesRemaining(), Is.EqualTo(0));
    }
}
