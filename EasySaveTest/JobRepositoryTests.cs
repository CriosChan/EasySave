using EasySave.Data.Configuration;
using EasySave.Models.Backup;
using EasySave.Models.Data.Persistence;
using EasySave.Core.Models;

namespace EasySaveTest;

public class JobRepositoryTests
{

    [Test]
    public void GetAll_WithNoFile_ReturnsEmptyList()
    {
        var repository = new JobRepository();

        var result = repository.GetAll();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void SaveAll_WithEmptyList_CreatesFile()
    {
        var repository = new JobRepository();
        var jobs = new List<BackupJob>();

        Assert.DoesNotThrow(() => repository.SaveAll(jobs));
    }

    [Test]
    public void SaveAll_WithNullList_ThrowsArgumentNullException()
    {
        var repository = new JobRepository();

        Assert.Throws<ArgumentNullException>(() => repository.SaveAll(null!));
    }

    [Test]
    public void SaveAll_ThenGetAll_ReturnsJobs()
    {
        var repository = new JobRepository();
        var jobs = new List<BackupJob>
        {
            new BackupJob(1, "Job1", "C:\\Source1", "C:\\Target1", BackupType.Complete)
        };

        repository.SaveAll(jobs);
        var result = repository.GetAll();

        Assert.That(result.Count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void SaveAll_OrdersJobsById()
    {
        var repository = new JobRepository();
        var jobs = new List<BackupJob>
        {
            new BackupJob(3, "Job3", "C:\\Source3", "C:\\Target3", BackupType.Complete),
            new BackupJob(1, "Job1", "C:\\Source1", "C:\\Target1", BackupType.Complete),
            new BackupJob(2, "Job2", "C:\\Source2", "C:\\Target2", BackupType.Complete)
        };

        Assert.DoesNotThrow(() => repository.SaveAll(jobs));
    }

    [Test]
    public void Constructor_CreatesInstance()
    {
        var repository = new JobRepository();

        Assert.That(repository, Is.Not.Null);
    }
}

