using EasySave.Models.Backup.Interfaces;
using EasySave.Models.State;

namespace EasySaveTest;

/// <summary>
///     Tests for the new <see cref="StateLogger"/> methods introduced by the priority-queue feature.
///     Each test uses an isolated temp directory so the singleton can be re-used safely.
/// </summary>
[NonParallelizable]
public class StateLoggerPriorityTests
{
    private string _tempDir = string.Empty;
    private BackupJobState _state = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"EasySaveStateLogger_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        StateFileSingleton.Instance.Initialize(_tempDir);
        _state = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // -----------------------------------------------------------------------
    // SetStateActiveWithQueues
    // -----------------------------------------------------------------------

    [Test]
    public void SetStateActiveWithQueues_SetsStateToActive()
    {
        StateLogger.SetStateActiveWithQueues(_state, 10, 2048, 4, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.That(updated.State, Is.EqualTo(JobRunState.Active));
    }

    [Test]
    public void SetStateActiveWithQueues_SetsTotalFilesAndSize()
    {
        StateLogger.SetStateActiveWithQueues(_state, 10, 2048, 4, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.TotalFiles, Is.EqualTo(10));
            Assert.That(updated.TotalSizeBytes, Is.EqualTo(2048));
        });
    }

    [Test]
    public void SetStateActiveWithQueues_InitialisesRemainingCounters()
    {
        StateLogger.SetStateActiveWithQueues(_state, 10, 2048, 4, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.RemainingFiles, Is.EqualTo(10));
            Assert.That(updated.RemainingSizeBytes, Is.EqualTo(2048));
        });
    }

    [Test]
    public void SetStateActiveWithQueues_SetsQueueCounters()
    {
        StateLogger.SetStateActiveWithQueues(_state, 10, 2048, 4, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.RemainingPriorityFiles, Is.EqualTo(4));
            Assert.That(updated.RemainingStandardFiles, Is.EqualTo(6));
        });
    }

    [Test]
    public void SetStateActiveWithQueues_ResetsProgressAndPaths()
    {
        StateLogger.SetStateActiveWithQueues(_state, 10, 2048, 4, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.ProgressPercent, Is.EqualTo(0));
            Assert.That(updated.CurrentSourcePath, Is.Null);
            Assert.That(updated.CurrentTargetPath, Is.Null);
            Assert.That(updated.CurrentAction, Is.EqualTo("start"));
        });
    }

    [Test]
    public void SetStateActiveWithQueues_AllPriorityNoStandard_CountersCorrect()
    {
        StateLogger.SetStateActiveWithQueues(_state, 5, 512, 5, 0);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.RemainingPriorityFiles, Is.EqualTo(5));
            Assert.That(updated.RemainingStandardFiles, Is.EqualTo(0));
        });
    }

    [Test]
    public void SetStateActiveWithQueues_AllStandardNoPriority_CountersCorrect()
    {
        StateLogger.SetStateActiveWithQueues(_state, 3, 300, 0, 3);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.RemainingPriorityFiles, Is.EqualTo(0));
            Assert.That(updated.RemainingStandardFiles, Is.EqualTo(3));
        });
    }

    // -----------------------------------------------------------------------
    // SetStateStartTransfer(state, file, isPriority)
    // -----------------------------------------------------------------------

    [Test]
    public void SetStateStartTransfer_PriorityFile_SetsLabelToPriorityFileTransfer()
    {
        var file = new FakeFile("C:\\src\\doc.pdf", "C:\\dst\\doc.pdf");

        StateLogger.SetStateStartTransfer(_state, file, isPriority: true);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.That(updated.CurrentAction, Is.EqualTo("priority file transfer"));
    }

    [Test]
    public void SetStateStartTransfer_StandardFile_SetsLabelToStandardFileTransfer()
    {
        var file = new FakeFile("C:\\src\\note.txt", "C:\\dst\\note.txt");

        StateLogger.SetStateStartTransfer(_state, file, isPriority: false);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.That(updated.CurrentAction, Is.EqualTo("standard file transfer"));
    }

    [Test]
    public void SetStateStartTransfer_PriorityFile_SetsStateToActive()
    {
        var file = new FakeFile("C:\\src\\doc.pdf", "C:\\dst\\doc.pdf");

        StateLogger.SetStateStartTransfer(_state, file, isPriority: true);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.That(updated.State, Is.EqualTo(JobRunState.Active));
    }

    [Test]
    public void SetStateStartTransfer_SetsSourceAndTargetPaths()
    {
        var file = new FakeFile("C:\\src\\image.png", "C:\\dst\\image.png");

        StateLogger.SetStateStartTransfer(_state, file, isPriority: false);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.CurrentSourcePath, Is.Not.Null);
            Assert.That(updated.CurrentTargetPath, Is.Not.Null);
        });
    }

    // -----------------------------------------------------------------------
    // SetStateEndTransferWithQueues
    // -----------------------------------------------------------------------

    [Test]
    public void SetStateEndTransferWithQueues_UpdatesRemainingFiles()
    {
        StateLogger.SetStateEndTransferWithQueues(_state, 10, 0, 1000, 100, 10.0, 3, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        // filesCount - (processedIndex + 1) = 10 - 1 = 9
        Assert.That(updated.RemainingFiles, Is.EqualTo(9));
    }

    [Test]
    public void SetStateEndTransferWithQueues_UpdatesRemainingSizeBytes()
    {
        StateLogger.SetStateEndTransferWithQueues(_state, 10, 0, 1000, 100, 10.0, 3, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.That(updated.RemainingSizeBytes, Is.EqualTo(900));
    }

    [Test]
    public void SetStateEndTransferWithQueues_UpdatesProgressPercent()
    {
        StateLogger.SetStateEndTransferWithQueues(_state, 10, 0, 1000, 100, 10.0, 3, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.That(updated.ProgressPercent, Is.EqualTo(10.0));
    }

    [Test]
    public void SetStateEndTransferWithQueues_UpdatesQueueCounters()
    {
        StateLogger.SetStateEndTransferWithQueues(_state, 10, 0, 1000, 100, 10.0, 3, 6);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.RemainingPriorityFiles, Is.EqualTo(3));
            Assert.That(updated.RemainingStandardFiles, Is.EqualTo(6));
        });
    }

    [Test]
    public void SetStateEndTransferWithQueues_LastFile_RemainingFilesIsZero()
    {
        // Last file: processedIndex = filesCount - 1
        StateLogger.SetStateEndTransferWithQueues(_state, 5, 4, 500, 500, 100.0, 0, 0);

        var updated = StateFileSingleton.Instance.GetOrCreate(99901, "PriorityTest");
        Assert.Multiple(() =>
        {
            Assert.That(updated.RemainingFiles, Is.EqualTo(0));
            Assert.That(updated.RemainingPriorityFiles, Is.EqualTo(0));
            Assert.That(updated.RemainingStandardFiles, Is.EqualTo(0));
            Assert.That(updated.ProgressPercent, Is.EqualTo(100.0));
        });
    }

    // -----------------------------------------------------------------------
    // Fake helpers
    // -----------------------------------------------------------------------

    private sealed class FakeFile : IFile
    {
        public FakeFile(string source, string target)
        {
            SourceFile = source;
            TargetFile = target;
        }

        public string SourceFile { get; }
        public string TargetFile { get; }
        public string BackupName => "PriorityTest";

        public void Copy() { }
        public long GetSize() => 0;
    }
}

