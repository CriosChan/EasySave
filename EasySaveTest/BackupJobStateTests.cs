using EasySave.Models.State;

namespace EasySaveTest;

public class BackupJobStateTests
{
    [Test]
    public void JobId_CanBeSet()
    {
        var state = new BackupJobState { JobId = 5 };

        Assert.That(state.JobId, Is.EqualTo(5));
    }

    [Test]
    public void BackupName_DefaultsToEmpty()
    {
        var state = new BackupJobState();

        Assert.That(state.BackupName, Is.EqualTo(""));
    }

    [Test]
    public void BackupName_CanBeSet()
    {
        var state = new BackupJobState { BackupName = "TestBackup" };

        Assert.That(state.BackupName, Is.EqualTo("TestBackup"));
    }

    [Test]
    public void LastActionTimestamp_CanBeSet()
    {
        var now = DateTime.Now;
        var state = new BackupJobState { LastActionTimestamp = now };

        Assert.That(state.LastActionTimestamp, Is.EqualTo(now));
    }

    [Test]
    public void State_DefaultsToInactive()
    {
        var state = new BackupJobState();

        Assert.That(state.State, Is.EqualTo(JobRunState.Inactive));
    }

    [Test]
    public void State_CanBeSet()
    {
        var state = new BackupJobState { State = JobRunState.Active };

        Assert.That(state.State, Is.EqualTo(JobRunState.Active));
    }

    [Test]
    public void TotalFiles_CanBeSet()
    {
        var state = new BackupJobState { TotalFiles = 100 };

        Assert.That(state.TotalFiles, Is.EqualTo(100));
    }

    [Test]
    public void TotalSizeBytes_CanBeSet()
    {
        var state = new BackupJobState { TotalSizeBytes = 1024000 };

        Assert.That(state.TotalSizeBytes, Is.EqualTo(1024000));
    }

    [Test]
    public void ProgressPercent_CanBeSet()
    {
        var state = new BackupJobState { ProgressPercent = 45.5 };

        Assert.That(state.ProgressPercent, Is.EqualTo(45.5));
    }

    [Test]
    public void RemainingFiles_CanBeSet()
    {
        var state = new BackupJobState { RemainingFiles = 50 };

        Assert.That(state.RemainingFiles, Is.EqualTo(50));
    }

    [Test]
    public void RemainingSizeBytes_CanBeSet()
    {
        var state = new BackupJobState { RemainingSizeBytes = 512000 };

        Assert.That(state.RemainingSizeBytes, Is.EqualTo(512000));
    }

    [Test]
    public void CurrentSourcePath_CanBeNull()
    {
        var state = new BackupJobState { CurrentSourcePath = null };

        Assert.That(state.CurrentSourcePath, Is.Null);
    }

    [Test]
    public void CurrentSourcePath_CanBeSet()
    {
        var state = new BackupJobState { CurrentSourcePath = "C:\\Source\\File.txt" };

        Assert.That(state.CurrentSourcePath, Is.EqualTo("C:\\Source\\File.txt"));
    }

    [Test]
    public void CurrentTargetPath_CanBeNull()
    {
        var state = new BackupJobState { CurrentTargetPath = null };

        Assert.That(state.CurrentTargetPath, Is.Null);
    }

    [Test]
    public void CurrentTargetPath_CanBeSet()
    {
        var state = new BackupJobState { CurrentTargetPath = "C:\\Target\\File.txt" };

        Assert.That(state.CurrentTargetPath, Is.EqualTo("C:\\Target\\File.txt"));
    }

    [Test]
    public void CurrentAction_CanBeNull()
    {
        var state = new BackupJobState { CurrentAction = null };

        Assert.That(state.CurrentAction, Is.Null);
    }

    [Test]
    public void CurrentAction_CanBeSet()
    {
        var state = new BackupJobState { CurrentAction = "file_transfer" };

        Assert.That(state.CurrentAction, Is.EqualTo("file_transfer"));
    }

    [Test]
    public void AllProperties_CanBeSetTogether()
    {
        var now = DateTime.Now;
        var state = new BackupJobState
        {
            JobId = 1,
            BackupName = "TestBackup",
            LastActionTimestamp = now,
            State = JobRunState.Active,
            TotalFiles = 100,
            TotalSizeBytes = 2048000,
            ProgressPercent = 50.0,
            RemainingFiles = 50,
            RemainingSizeBytes = 1024000,
            CurrentSourcePath = "C:\\Source\\File.txt",
            CurrentTargetPath = "C:\\Target\\File.txt",
            CurrentAction = "file_transfer"
        };

        Assert.Multiple(() =>
        {
            Assert.That(state.JobId, Is.EqualTo(1));
            Assert.That(state.BackupName, Is.EqualTo("TestBackup"));
            Assert.That(state.LastActionTimestamp, Is.EqualTo(now));
            Assert.That(state.State, Is.EqualTo(JobRunState.Active));
            Assert.That(state.TotalFiles, Is.EqualTo(100));
            Assert.That(state.TotalSizeBytes, Is.EqualTo(2048000));
            Assert.That(state.ProgressPercent, Is.EqualTo(50.0));
            Assert.That(state.RemainingFiles, Is.EqualTo(50));
            Assert.That(state.RemainingSizeBytes, Is.EqualTo(1024000));
            Assert.That(state.CurrentSourcePath, Is.EqualTo("C:\\Source\\File.txt"));
            Assert.That(state.CurrentTargetPath, Is.EqualTo("C:\\Target\\File.txt"));
            Assert.That(state.CurrentAction, Is.EqualTo("file_transfer"));
        });
    }
}

