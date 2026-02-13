using EasySave.Core.Models;
using EasySave.Models.Backup;
using EasySave.ViewModels;

namespace EasySaveTest;

public class BackupJobItemViewModelTests
{
    [Test]
    public void Constructor_WithValidJob_CreatesInstance()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);

        var viewModel = new BackupJobItemViewModel(job);

        Assert.That(viewModel, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullJob_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BackupJobItemViewModel(null!));
    }

    [Test]
    public void Job_ReturnsUnderlyingJob()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);
        var viewModel = new BackupJobItemViewModel(job);

        Assert.That(viewModel.Job, Is.SameAs(job));
    }

    [Test]
    public void DisplayName_ContainsIdAndName()
    {
        var job = new BackupJob(5, "MyBackup", "C:\\Source", "C:\\Target", BackupType.Complete);
        var viewModel = new BackupJobItemViewModel(job);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.DisplayName, Does.Contain("5"));
            Assert.That(viewModel.DisplayName, Does.Contain("MyBackup"));
        });
    }

    [Test]
    public void DisplayName_HasCorrectFormat()
    {
        var job = new BackupJob(3, "TestBackup", "C:\\Source", "C:\\Target", BackupType.Complete);
        var viewModel = new BackupJobItemViewModel(job);

        Assert.That(viewModel.DisplayName, Is.EqualTo("[3] TestBackup"));
    }

    [Test]
    public void Type_ReturnsBackupTypeAsString()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);
        var viewModel = new BackupJobItemViewModel(job);

        Assert.That(viewModel.Type, Is.EqualTo("Complete"));
    }

    [Test]
    public void Type_WithDifferentialBackup_ReturnsDifferential()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Differential);
        var viewModel = new BackupJobItemViewModel(job);

        Assert.That(viewModel.Type, Is.EqualTo("Differential"));
    }

    [Test]
    public void DisplayPath_ContainsSourceAndTarget()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);
        var viewModel = new BackupJobItemViewModel(job);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.DisplayPath, Does.Contain("C:\\Source"));
            Assert.That(viewModel.DisplayPath, Does.Contain("C:\\Target"));
        });
    }

    [Test]
    public void DisplayPath_HasArrowSeparator()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);
        var viewModel = new BackupJobItemViewModel(job);

        Assert.That(viewModel.DisplayPath, Does.Contain("→"));
    }

    [Test]
    public void DisplayPath_HasCorrectFormat()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);
        var viewModel = new BackupJobItemViewModel(job);

        Assert.That(viewModel.DisplayPath, Is.EqualTo("C:\\Source → C:\\Target"));
    }
}

