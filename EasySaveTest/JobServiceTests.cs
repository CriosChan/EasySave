using EasySave.Core.Models;
using EasySave.Models.Backup;

namespace EasySaveTest;

public class JobServiceTests
{

    [Test]
    public void GetAll_ReturnsNotNull()
    {
        var service = new JobService();

        var result = service.GetAll();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void AddJob_WithValidJob_ReturnsSuccess()
    {
        var service = new JobService();
        var job = new BackupJob("TestJobValid", "C:\\Source", "C:\\Target", BackupType.Complete);

        var (ok, error) = service.AddJob(job);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.True);
            Assert.That(error, Is.Empty);
        });
        
        // Cleanup
        service.RemoveJob(job.Id.ToString());
    }

    [Test]
    public void AddJob_AssignsId_WhenJobHasNoId()
    {
        var service = new JobService();
        var job = new BackupJob("TestJobAssignId", "C:\\Source", "C:\\Target", BackupType.Complete);

        service.AddJob(job);

        Assert.That(job.Id, Is.GreaterThan(0));
        
        // Cleanup
        service.RemoveJob(job.Id.ToString());
    }

    [Test]
    public void AddJob_WithNullJob_ThrowsArgumentNullException()
    {
        var service = new JobService();

        Assert.Throws<ArgumentNullException>(() => service.AddJob(null!));
    }

    [Test]
    public void AddJob_WithExistingId_ThrowsInvalidOperationException()
    {
        var service = new JobService();
        var job = new BackupJob(5, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);

        Assert.Throws<InvalidOperationException>(() => service.AddJob(job));
    }

    [Test]
    public void AddJob_PersistsJob_CanBeRetrieved()
    {
        var service = new JobService();
        var job = new BackupJob("TestJobPersist", "C:\\Source", "C:\\Target", BackupType.Complete);

        service.AddJob(job);
        var result = service.GetAll();
        var addedJob = result.FirstOrDefault(j => j.Name == "TestJobPersist");

        Assert.Multiple(() =>
        {
            Assert.That(addedJob, Is.Not.Null);
            Assert.That(addedJob!.Name, Is.EqualTo("TestJobPersist"));
        });

        // Cleanup
        service.RemoveJob(job.Id.ToString());
    }

    [Test]
    public void RemoveJob_WithExistingId_ReturnsTrue()
    {
        var service = new JobService();
        var job = new BackupJob("TestJobRemoveById", "C:\\Source", "C:\\Target", BackupType.Complete);
        service.AddJob(job);

        var result = service.RemoveJob(job.Id.ToString());

        Assert.That(result, Is.True);
        
        // Verify it's actually removed
        var allJobs = service.GetAll();
        Assert.That(allJobs.Any(j => j.Id == job.Id), Is.False);
    }

    [Test]
    public void RemoveJob_WithExistingName_ReturnsTrue()
    {
        var service = new JobService();
        var job = new BackupJob("TestJobRemoveByName", "C:\\Source", "C:\\Target", BackupType.Complete);
        service.AddJob(job);

        var result = service.RemoveJob("TestJobRemoveByName");

        Assert.That(result, Is.True);
        
        // Verify it's actually removed
        var allJobs = service.GetAll();
        Assert.That(allJobs.Any(j => j.Name == "TestJobRemoveByName"), Is.False);
    }

    [Test]
    public void RemoveJob_WithNonExistingId_ReturnsFalse()
    {
        var service = new JobService();

        var result = service.RemoveJob("999");

        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveJob_WithNonExistingName_ReturnsFalse()
    {
        var service = new JobService();

        var result = service.RemoveJob("NonExistent");

        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveJob_WithEmptyString_ReturnsFalse()
    {
        var service = new JobService();

        var result = service.RemoveJob("");

        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveJob_WithWhitespace_ReturnsFalse()
    {
        var service = new JobService();

        var result = service.RemoveJob("   ");

        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveJob_RemovesJob_CannotBeRetrieved()
    {
        var service = new JobService();
        var job = new BackupJob("TestJobToRemove", "C:\\Source", "C:\\Target", BackupType.Complete);
        service.AddJob(job);

        service.RemoveJob(job.Id.ToString());
        var result = service.GetAll();

        // Verify the specific job is not in the list anymore
        Assert.That(result.Any(j => j.Id == job.Id), Is.False);
    }

    [Test]
    public void GetAll_ReturnsJobsOrderedById()
    {
        var service = new JobService();
        var job1 = new BackupJob("JobOrder1", "C:\\Source1", "C:\\Target1", BackupType.Complete);
        var job2 = new BackupJob("JobOrder2", "C:\\Source2", "C:\\Target2", BackupType.Complete);
        var job3 = new BackupJob("JobOrder3", "C:\\Source3", "C:\\Target3", BackupType.Complete);

        service.AddJob(job3);
        service.AddJob(job1);
        service.AddJob(job2);

        var result = service.GetAll();
        var ourJobs = result.Where(j => j.Name.StartsWith("JobOrder")).OrderBy(j => j.Id).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(ourJobs, Has.Count.EqualTo(3));
            Assert.That(ourJobs[0].Id, Is.LessThan(ourJobs[1].Id));
            Assert.That(ourJobs[1].Id, Is.LessThan(ourJobs[2].Id));
        });
        
        // Cleanup
        service.RemoveJob(job1.Id.ToString());
        service.RemoveJob(job2.Id.ToString());
        service.RemoveJob(job3.Id.ToString());
    }

    [Test]
    public void AddJob_AssignsSequentialIds()
    {
        var service = new JobService();
        var job1 = new BackupJob("JobSeq1", "C:\\Source1", "C:\\Target1", BackupType.Complete);
        var job2 = new BackupJob("JobSeq2", "C:\\Source2", "C:\\Target2", BackupType.Complete);

        service.AddJob(job1);
        service.AddJob(job2);

        Assert.That(job2.Id, Is.EqualTo(job1.Id + 1));
        
        // Cleanup
        service.RemoveJob(job1.Id.ToString());
        service.RemoveJob(job2.Id.ToString());
    }

    [Test]
    public void RemoveJob_IsCaseInsensitive_ForName()
    {
        var service = new JobService();
        var job = new BackupJob("TestJobCaseInsensitive", "C:\\Source", "C:\\Target", BackupType.Complete);
        service.AddJob(job);

        var result = service.RemoveJob("testjobcaseinsensitive");

        Assert.That(result, Is.True);
        
        // Verify removal
        var allJobs = service.GetAll();
        Assert.That(allJobs.Any(j => j.Name == "TestJobCaseInsensitive"), Is.False);
    }
}

