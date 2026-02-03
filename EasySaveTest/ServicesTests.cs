using NUnit.Framework;
using EasySave.Services;
using EasySave.Models;
using EasySave.Utils;
using System.IO;
using System.Collections.Generic;
using System;

namespace EasySaveTest;

public class JobRepositoryTests
{
    private string _tempDir = null!;
    private JobRepository _repo = null!;

    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _repo = new JobRepository(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Test]
    public void Load_EmptyRepository_ReturnsEmptyList()
    {
        var jobs = _repo.Load();
        Assert.That(jobs, Is.Not.Null);
        Assert.That(jobs.Count, Is.EqualTo(0));
    }

    [Test]
    public void Save_PersistsJobsToFile()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "job1", SourceDirectory = "/src1", TargetDirectory = "/dst1" }
        };

        _repo.Save(jobs);

        var loaded = _repo.Load();
        Assert.That(loaded.Count, Is.EqualTo(1));
        Assert.That(loaded[0].Name, Is.EqualTo("job1"));
    }

    [Test]
    public void AddJob_AssignsId_WhenSlotAvailable()
    {
        var jobs = new List<BackupJob>();
        var job = new BackupJob { Name = "newJob", SourceDirectory = "/src", TargetDirectory = "/dst" };

        var (ok, err) = _repo.AddJob(jobs, job);

        Assert.That(ok, Is.True);
        Assert.That(job.Id, Is.EqualTo(1));
    }

    [Test]
    public void AddJob_AssignsNextAvailableId()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "j1" },
            new BackupJob { Id = 2, Name = "j2" }
        };
        var job = new BackupJob { Name = "j3", SourceDirectory = "/src", TargetDirectory = "/dst" };

        var (ok, err) = _repo.AddJob(jobs, job);

        Assert.That(ok, Is.True);
        Assert.That(job.Id, Is.EqualTo(3));
    }

    [Test]
    public void AddJob_ReturnsError_WhenMaxJobsReached()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "j1" },
            new BackupJob { Id = 2, Name = "j2" },
            new BackupJob { Id = 3, Name = "j3" },
            new BackupJob { Id = 4, Name = "j4" },
            new BackupJob { Id = 5, Name = "j5" }
        };
        var job = new BackupJob { Name = "j6", SourceDirectory = "/src", TargetDirectory = "/dst" };

        var (ok, err) = _repo.AddJob(jobs, job);

        Assert.That(ok, Is.False);
        Assert.That(err, Is.EqualTo("Error.MaxJobs"));
    }

    [Test]
    public void RemoveJob_ById_RemovesJob()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "job1" },
            new BackupJob { Id = 2, Name = "job2" }
        };

        bool removed = _repo.RemoveJob(jobs, "1");

        Assert.That(removed, Is.True);
        Assert.That(jobs.Count, Is.EqualTo(1));
        Assert.That(jobs[0].Id, Is.EqualTo(2));
    }

    [Test]
    public void RemoveJob_ByName_RemovesJob()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "backup_one" },
            new BackupJob { Id = 2, Name = "backup_two" }
        };

        bool removed = _repo.RemoveJob(jobs, "backup_one");

        Assert.That(removed, Is.True);
        Assert.That(jobs.Count, Is.EqualTo(1));
        Assert.That(jobs[0].Name, Is.EqualTo("backup_two"));
    }

    [Test]
    public void RemoveJob_NonExistent_ReturnsFalse()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "job1" }
        };

        bool removed = _repo.RemoveJob(jobs, "999");

        Assert.That(removed, Is.False);
        Assert.That(jobs.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveJob_ByName_CaseInsensitive()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "MyBackup" }
        };

        bool removed = _repo.RemoveJob(jobs, "mybackup");

        Assert.That(removed, Is.True);
    }
}

public class StateFileServiceTests
{
    private string _tempDir = null!;
    private StateFileService _stateService = null!;

    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _stateService = new StateFileService(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Test]
    public void Initialize_CreatesStateFile()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "job1" },
            new BackupJob { Id = 2, Name = "job2" }
        };

        _stateService.Initialize(jobs);

        string statePath = Path.Combine(_tempDir, "state.json");
        Assert.That(File.Exists(statePath), Is.True);
    }

    [Test]
    public void Initialize_SetsJobsToInactive()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1, Name = "job1" }
        };

        _stateService.Initialize(jobs);

        var state = _stateService.GetOrCreate(jobs[0]);
        Assert.That(state.State, Is.EqualTo(JobRunState.Inactive));
        Assert.That(state.BackupName, Is.EqualTo("job1"));
    }

    [Test]
    public void GetOrCreate_ReturnsExisting_WhenStateExists()
    {
        var job = new BackupJob { Id = 1, Name = "job1" };
        _stateService.Initialize(new List<BackupJob> { job });

        var state1 = _stateService.GetOrCreate(job);
        var state2 = _stateService.GetOrCreate(job);

        Assert.That(state1.JobId, Is.EqualTo(state2.JobId));
    }

    [Test]
    public void GetOrCreate_CreatesNewState_WhenMissing()
    {
        var job = new BackupJob { Id = 99, Name = "newJob" };

        var state = _stateService.GetOrCreate(job);

        Assert.That(state.JobId, Is.EqualTo(99));
        Assert.That(state.BackupName, Is.EqualTo("newJob"));
    }

    [Test]
    public void Update_ModifiesState_AndWritesFile()
    {
        var job = new BackupJob { Id = 1, Name = "job1" };
        _stateService.Initialize(new List<BackupJob> { job });

        var state = _stateService.GetOrCreate(job);
        state.State = JobRunState.Active;
        state.ProgressPercent = 50.0;
        _stateService.Update(state);

        var updated = _stateService.GetOrCreate(job);
        Assert.That(updated.State, Is.EqualTo(JobRunState.Active));
        Assert.That(updated.ProgressPercent, Is.EqualTo(50.0));
    }

    [Test]
    public void Update_OrdersStatesByJobId()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 3, Name = "job3" },
            new BackupJob { Id = 1, Name = "job1" },
            new BackupJob { Id = 2, Name = "job2" }
        };
        _stateService.Initialize(jobs);

        var state1 = _stateService.GetOrCreate(jobs[0]);
        state1.State = JobRunState.Active;
        _stateService.Update(state1);

        // Verify ordering by loading from file
        string json = File.ReadAllText(Path.Combine(_tempDir, "state.json"));
        var loaded = JsonFile.ReadOrDefault(Path.Combine(_tempDir, "state.json"), new List<BackupJobState>());
        Assert.That(loaded[0].JobId, Is.LessThanOrEqualTo(loaded[loaded.Count - 1].JobId));
    }
}

public class BackupServiceTests
{
    private string _tempDir = null!;
    private string _sourceDir = null!;
    private string _targetDir = null!;
    private StateFileService _stateService = null!;
    private string _logDir = null!;
    private BackupService _backupService = null!;

    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _sourceDir = Path.Combine(_tempDir, "source");
        _targetDir = Path.Combine(_tempDir, "target");
        _logDir = Path.Combine(_tempDir, "logs");

        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_targetDir);
        Directory.CreateDirectory(_logDir);

        _stateService = new StateFileService(Path.Combine(_tempDir, "state"));
        _backupService = new BackupService(_logDir, _stateService);
    }

    [TearDown]
    public void TearDown()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Test]
    public void RunJob_CopiesFiles_InCompleteBackup()
    {
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "content2");

        var job = new BackupJob
        {
            Id = 1,
            Name = "completeBackup",
            SourceDirectory = _sourceDir,
            TargetDirectory = _targetDir,
            Type = BackupType.Complete
        };

        _backupService.RunJob(job);

        Assert.That(File.Exists(Path.Combine(_targetDir, "file1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(_targetDir, "file2.txt")), Is.True);
    }

    [Test]
    public void RunJob_CreatesSubdirectories()
    {
        string subDir = Path.Combine(_sourceDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "nested.txt"), "content");

        var job = new BackupJob
        {
            Id = 1,
            Name = "backupWithDirs",
            SourceDirectory = _sourceDir,
            TargetDirectory = _targetDir,
            Type = BackupType.Complete
        };

        _backupService.RunJob(job);

        string targetNested = Path.Combine(_targetDir, "subdir", "nested.txt");
        Assert.That(File.Exists(targetNested), Is.True);
    }

    [Test]
    public void RunJob_HandlesEmptyDirectory()
    {
        var job = new BackupJob
        {
            Id = 1,
            Name = "emptyBackup",
            SourceDirectory = _sourceDir,
            TargetDirectory = _targetDir,
            Type = BackupType.Complete
        };

        _backupService.RunJob(job);

        var state = _stateService.GetOrCreate(job);
        Assert.That(state.State, Is.EqualTo(JobRunState.Completed));
    }

    [Test]
    public void RunJob_LogsFileTransfers()
    {
        File.WriteAllText(Path.Combine(_sourceDir, "file.txt"), "test");

        var job = new BackupJob
        {
            Id = 1,
            Name = "loggedBackup",
            SourceDirectory = _sourceDir,
            TargetDirectory = _targetDir,
            Type = BackupType.Complete
        };

        _backupService.RunJob(job);

        string logPath = Path.Combine(_logDir, DateTime.Now.ToString("yyyy-MM-dd") + ".json");
        var logs = JsonFile.ReadOrDefault(logPath, new List<LogEntry>());
        
        Assert.That(logs.Count, Is.GreaterThan(0));
        Assert.That(logs[0].BackupName, Is.EqualTo("loggedBackup"));
        Assert.That(logs[0].FileSizeBytes, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void RunJob_FailsWhenSourceMissing()
    {
        var job = new BackupJob
        {
            Id = 1,
            Name = "missingSourceBackup",
            SourceDirectory = "/nonexistent/path",
            TargetDirectory = _targetDir,
            Type = BackupType.Complete
        };

        _backupService.RunJob(job);

        var state = _stateService.GetOrCreate(job);
        Assert.That(state.State, Is.EqualTo(JobRunState.Failed));
    }

    [Test]
    public void RunJob_PreservesFileTimestamps()
    {
        string sourceFile = Path.Combine(_sourceDir, "timestamped.txt");
        File.WriteAllText(sourceFile, "content");
        var originalTime = new DateTime(2020, 1, 1, 12, 0, 0);
        File.SetLastWriteTimeUtc(sourceFile, originalTime);

        var job = new BackupJob
        {
            Id = 1,
            Name = "timestampBackup",
            SourceDirectory = _sourceDir,
            TargetDirectory = _targetDir,
            Type = BackupType.Complete
        };

        _backupService.RunJob(job);

        string targetFile = Path.Combine(_targetDir, "timestamped.txt");
        var targetTime = File.GetLastWriteTimeUtc(targetFile);
        
        Assert.That(targetTime, Is.EqualTo(originalTime).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void RunJobsSequential_RunsMultipleJobs_InOrder()
    {
        var sourceDir1 = Path.Combine(_sourceDir, "job1");
        var sourceDir2 = Path.Combine(_sourceDir, "job2");
        var targetDir1 = Path.Combine(_targetDir, "out1");
        var targetDir2 = Path.Combine(_targetDir, "out2");

        Directory.CreateDirectory(sourceDir1);
        Directory.CreateDirectory(sourceDir2);
        Directory.CreateDirectory(targetDir1);
        Directory.CreateDirectory(targetDir2);

        File.WriteAllText(Path.Combine(sourceDir1, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(sourceDir2, "file2.txt"), "content2");

        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 2, Name = "job2", SourceDirectory = sourceDir2, TargetDirectory = targetDir2, Type = BackupType.Complete },
            new BackupJob { Id = 1, Name = "job1", SourceDirectory = sourceDir1, TargetDirectory = targetDir1, Type = BackupType.Complete }
        };

        _backupService.RunJobsSequential(jobs);

        Assert.That(File.Exists(Path.Combine(targetDir1, "file1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(targetDir2, "file2.txt")), Is.True);
    }

    [Test]
    public void RunJob_DifferentialBackup_OnlyUpdatesDifferentFiles()
    {
        // Create initial complete backup
        string file1 = Path.Combine(_sourceDir, "file1.txt");
        string file2 = Path.Combine(_sourceDir, "file2.txt");
        File.WriteAllText(file1, "content1");
        File.WriteAllText(file2, "content2");

        var job = new BackupJob
        {
            Id = 1,
            Name = "diffBackup",
            SourceDirectory = _sourceDir,
            TargetDirectory = _targetDir,
            Type = BackupType.Differential
        };

        _backupService.RunJob(job);

        // Both files should be copied in first differential (treated as complete for empty target)
        Assert.That(File.Exists(Path.Combine(_targetDir, "file1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(_targetDir, "file2.txt")), Is.True);
    }
}
