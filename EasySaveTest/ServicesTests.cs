using EasySave.Services;
using EasySave.Core.Models;
using EasySave.Platform.IO;
using EasySave.Data.Logging;
using EasySave.Data.Persistence;

namespace EasySaveTest;

/// <summary>
///     Tests for the backup job repository.
/// </summary>
public class JobRepositoryTests
{
    private JobRepository _repo = null!;
    private string _tempDir = null!;

    /// <summary>
    ///     Prepares a temporary folder and a job repository.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _repo = new JobRepository(_tempDir);
    }

    /// <summary>
    ///     Cleans up temporary resources.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        try
        {
            Directory.Delete(_tempDir, true);
        }
        catch (Exception ex)
        {
            // Best-effort cleanup: log failure but do not fail the test.
            TestContext.WriteLine($"Failed to delete temp directory '{_tempDir}': {ex}");
        }
    }

    /// <summary>
    ///     Verifies loading an empty repository returns an empty list.
    /// </summary>
    [Test]
    public void Load_EmptyRepository_ReturnsEmptyList()
    {
        var jobs = _repo.GetAll();
        Assert.That(jobs, Is.Not.Null);
        Assert.That(jobs.Count, Is.EqualTo(0));
    }

    /// <summary>
    ///     Verifies saving persists jobs to the file.
    /// </summary>
    [Test]
    public void Save_PersistsJobsToFile()
    {
        var jobs = new List<BackupJob>
        {
            new(1, "job1", "/src1", "/dst1", BackupType.Complete)
        };

        _repo.SaveAll(jobs);

        var loaded = _repo.GetAll();
        Assert.That(loaded.Count, Is.EqualTo(1));
        Assert.That(loaded[0].Name, Is.EqualTo("job1"));
    }
}

/// <summary>
///     Tests for the backup job service (business rules).
/// </summary>
public class JobServiceTests
{
    private JobRepository _repo = null!;
    private JobService _service = null!;
    private string _tempDir = null!;

    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _repo = new JobRepository(_tempDir);
        _service = new JobService(_repo);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            Directory.Delete(_tempDir, true);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Failed to delete temp directory '{_tempDir}': {ex}");
        }
    }

    [Test]
    public void AddJob_AssignsId_WhenSlotAvailable()
    {
        var job = new BackupJob("newJob", "/src", "/dst", BackupType.Complete);

        var (ok, _) = _service.AddJob(job);

        Assert.That(ok, Is.True);
        Assert.That(job.Id, Is.EqualTo(1));
    }

    [Test]
    public void AddJob_AssignsNextAvailableId()
    {
        SaveJobs(
            new BackupJob(1, "j1", "/src", "/dst", BackupType.Complete),
            new BackupJob(2, "j2", "/src", "/dst", BackupType.Complete)
        );

        var job = new BackupJob("j3", "/src", "/dst", BackupType.Complete);

        var (ok, _) = _service.AddJob(job);

        Assert.That(ok, Is.True);
        Assert.That(job.Id, Is.EqualTo(3));
    }

    [Test]
    public void AddJob_ReturnsError_WhenMaxJobsReached()
    {
        SaveJobs(
            new BackupJob(1, "j1", "/src", "/dst", BackupType.Complete),
            new BackupJob(2, "j2", "/src", "/dst", BackupType.Complete),
            new BackupJob(3, "j3", "/src", "/dst", BackupType.Complete),
            new BackupJob(4, "j4", "/src", "/dst", BackupType.Complete),
            new BackupJob(5, "j5", "/src", "/dst", BackupType.Complete)
        );

        var job = new BackupJob("j6", "/src", "/dst", BackupType.Complete);

        var (ok, err) = _service.AddJob(job);

        Assert.That(ok, Is.False);
        Assert.That(err, Is.EqualTo("Error.MaxJobs"));
    }

    [Test]
    public void RemoveJob_ById_RemovesJob()
    {
        SaveJobs(
            new BackupJob(1, "job1", "/src", "/dst", BackupType.Complete),
            new BackupJob(2, "job2", "/src", "/dst", BackupType.Complete)
        );

        var removed = _service.RemoveJob("1");

        Assert.That(removed, Is.True);
        Assert.That(_repo.GetAll().Count, Is.EqualTo(1));
        Assert.That(_repo.GetAll().Single().Id, Is.EqualTo(2));
    }

    [Test]
    public void RemoveJob_ByName_RemovesJob()
    {
        SaveJobs(
            new BackupJob(1, "backup_one", "/src", "/dst", BackupType.Complete),
            new BackupJob(2, "backup_two", "/src", "/dst", BackupType.Complete)
        );

        var removed = _service.RemoveJob("backup_one");

        Assert.That(removed, Is.True);
        Assert.That(_repo.GetAll().Count, Is.EqualTo(1));
        Assert.That(_repo.GetAll().Single().Name, Is.EqualTo("backup_two"));
    }

    [Test]
    public void RemoveJob_NonExistent_ReturnsFalse()
    {
        SaveJobs(new BackupJob(1, "job1", "/src", "/dst", BackupType.Complete));

        var removed = _service.RemoveJob("999");

        Assert.That(removed, Is.False);
        Assert.That(_repo.GetAll().Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveJob_ByName_CaseInsensitive()
    {
        SaveJobs(new BackupJob(1, "MyBackup", "/src", "/dst", BackupType.Complete));

        var removed = _service.RemoveJob("mybackup");

        Assert.That(removed, Is.True);
    }

    private void SaveJobs(params BackupJob[] jobs)
    {
        _repo.SaveAll(jobs.ToList());
    }
}

/// <summary>
///     Tests for the state file service.
/// </summary>
public class StateFileServiceTests
{
    private StateFileService _stateService = null!;
    private string _tempDir = null!;

    /// <summary>
    ///     Prepares a temporary folder and the state service.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _stateService = new StateFileService(_tempDir);
    }

    /// <summary>
    ///     Cleans up temporary resources.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        try
        {
            Directory.Delete(_tempDir, true);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Failed to delete temp directory '{_tempDir}': {ex}");
        }
    }

    /// <summary>
    ///     Verifies initialization creates the state file.
    /// </summary>
    [Test]
    public void Initialize_CreatesStateFile()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob(1, "job1", "/src", "/dst", BackupType.Complete),
            new BackupJob(2, "job2", "/src", "/dst", BackupType.Complete)
        };

        _stateService.Initialize(jobs);

        var statePath = Path.Combine(_tempDir, "state.json");
        Assert.That(File.Exists(statePath), Is.True);
    }

    /// <summary>
    ///     Verifies jobs are initialized in the Inactive state.
    /// </summary>
    [Test]
    public void Initialize_SetsJobsToInactive()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob(1, "job1", "/src", "/dst", BackupType.Complete)
        };

        _stateService.Initialize(jobs);

        var state = _stateService.GetOrCreate(jobs[0]);
        Assert.That(state.State, Is.EqualTo(JobRunState.Inactive));
        Assert.That(state.BackupName, Is.EqualTo("job1"));
    }

    /// <summary>
    ///     Verifies GetOrCreate returns the existing state.
    /// </summary>
    [Test]
    public void GetOrCreate_ReturnsExisting_WhenStateExists()
    {
        var job = new BackupJob(1, "job1", "/src", "/dst", BackupType.Complete);
        _stateService.Initialize(new List<BackupJob> { job });

        var state1 = _stateService.GetOrCreate(job);
        var state2 = _stateService.GetOrCreate(job);

        Assert.That(state1.JobId, Is.EqualTo(state2.JobId));
    }

    /// <summary>
    ///     Verifies GetOrCreate creates a state when missing.
    /// </summary>
    [Test]
    public void GetOrCreate_CreatesNewState_WhenMissing()
    {
        var job = new BackupJob(99, "newJob", "/src", "/dst", BackupType.Complete);

        var state = _stateService.GetOrCreate(job);

        Assert.That(state.JobId, Is.EqualTo(99));
        Assert.That(state.BackupName, Is.EqualTo("newJob"));
    }

    /// <summary>
    ///     Verifies state updates and file writes.
    /// </summary>
    [Test]
    public void Update_ModifiesState_AndWritesFile()
    {
        var job = new BackupJob(1, "job1", "/src", "/dst", BackupType.Complete);
        _stateService.Initialize(new List<BackupJob> { job });

        var state = _stateService.GetOrCreate(job);
        state.State = JobRunState.Active;
        state.ProgressPercent = 50.0;
        _stateService.Update(state);

        var updated = _stateService.GetOrCreate(job);
        Assert.That(updated.State, Is.EqualTo(JobRunState.Active));
        Assert.That(updated.ProgressPercent, Is.EqualTo(50.0));
    }

    /// <summary>
    ///     Verifies states are ordered by ID.
    /// </summary>
    [Test]
    public void Update_OrdersStatesByJobId()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob(3, "job3", "/src", "/dst", BackupType.Complete),
            new BackupJob(1, "job1", "/src", "/dst", BackupType.Complete),
            new BackupJob(2, "job2", "/src", "/dst", BackupType.Complete)
        };
        _stateService.Initialize(jobs);

        var state1 = _stateService.GetOrCreate(jobs[0]);
        state1.State = JobRunState.Active;
        _stateService.Update(state1);

        var loaded = JsonFile.ReadOrDefault(Path.Combine(_tempDir, "state.json"), new List<BackupJobState>());
        var ids = loaded.Select(s => s.JobId).ToList();
        var sorted = ids.OrderBy(id => id).ToList();
        Assert.That(ids, Is.EqualTo(sorted));
    }
}

/// <summary>
///     Tests for the backup execution service.
/// </summary>
public class BackupServiceTests
{
    private BackupService _backupService = null!;
    private string _logDir = null!;
    private string _sourceDir = null!;
    private StateFileService _stateService = null!;
    private string _targetDir = null!;
    private string _tempDir = null!;

    /// <summary>
    ///     Prepares temporary folders and the backup service.
    /// </summary>
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

        var paths = new PathService();
        var logWriter = new JsonLogWriter<LogEntry>(_logDir);
        var fileSelector = new BackupFileSelector(paths);
        var directoryPreparer = new BackupDirectoryPreparer(logWriter, paths);
        var fileCopier = new FileCopier();

        _backupService =
            new BackupService(
                logWriter,
                _stateService,
                paths,
                fileSelector,
                directoryPreparer,
                fileCopier,
                new JobValidator(paths),
                new NullProgressReporter());
    }

    /// <summary>
    ///     Cleans up temporary resources.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        try
        {
            Directory.Delete(_tempDir, true);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Failed to delete temp directory '{_tempDir}': {ex}");
        }
    }

    /// <summary>
    ///     Verifies files are copied in a complete backup.
    /// </summary>
    [Test]
    public void RunJob_CopiesFiles_InCompleteBackup()
    {
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "content2");

        var job = new BackupJob(1, "completeBackup", _sourceDir, _targetDir, BackupType.Complete);

        _backupService.RunJob(job);

        Assert.That(File.Exists(Path.Combine(_targetDir, "file1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(_targetDir, "file2.txt")), Is.True);
    }

    /// <summary>
    ///     Verifies subdirectories are created in the target.
    /// </summary>
    [Test]
    public void RunJob_CreatesSubdirectories()
    {
        var subDir = Path.Combine(_sourceDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "nested.txt"), "content");

        var job = new BackupJob(1, "backupWithDirs", _sourceDir, _targetDir, BackupType.Complete);

        _backupService.RunJob(job);

        var targetNested = Path.Combine(_targetDir, "subdir", "nested.txt");
        Assert.That(File.Exists(targetNested), Is.True);
    }

    /// <summary>
    ///     Verifies an empty directory backup completes correctly.
    /// </summary>
    [Test]
    public void RunJob_HandlesEmptyDirectory()
    {
        var job = new BackupJob(1, "emptyBackup", _sourceDir, _targetDir, BackupType.Complete);

        _backupService.RunJob(job);

        var state = _stateService.GetOrCreate(job);
        Assert.That(state.State, Is.EqualTo(JobRunState.Completed));
    }

    /// <summary>
    ///     Verifies transfers are properly logged.
    /// </summary>
    [Test]
    [Category("GHABlacklist")]
    public void RunJob_LogsFileTransfers()
    {
        File.WriteAllText(Path.Combine(_sourceDir, "file.txt"), "test");

        var day = DateTime.Now.ToString("yyyy-MM-dd");

        var job = new BackupJob(1, "loggedBackup", _sourceDir, _targetDir, BackupType.Complete);

        _backupService.RunJob(job);

        var logPath = Path.Combine(_logDir, day + ".json");
        var logs = JsonFile.ReadOrDefault(logPath, new List<LogEntry>());

        Assert.That(logs.Count, Is.GreaterThan(0));
        Assert.That(logs[0].BackupName, Is.EqualTo("loggedBackup"));
        Assert.That(logs[0].FileSizeBytes, Is.GreaterThanOrEqualTo(0));
    }

    /// <summary>
    ///     Verifies failure when the source is missing.
    /// </summary>
    [Test]
    public void RunJob_FailsWhenSourceMissing()
    {
        var job = new BackupJob(1, "missingSourceBackup", "/nonexistent/path", _targetDir, BackupType.Complete);

        _backupService.RunJob(job);

        var state = _stateService.GetOrCreate(job);
        Assert.That(state.State, Is.EqualTo(JobRunState.Failed));
    }

    /// <summary>
    ///     Verifies file timestamps are preserved.
    /// </summary>
    [Test]
    public void RunJob_PreservesFileTimestamps()
    {
        var sourceFile = Path.Combine(_sourceDir, "timestamped.txt");
        File.WriteAllText(sourceFile, "content");
        var originalTime = new DateTime(2020, 1, 1, 12, 0, 0);
        File.SetLastWriteTimeUtc(sourceFile, originalTime);

        var job = new BackupJob(1, "timestampBackup", _sourceDir, _targetDir, BackupType.Complete);

        _backupService.RunJob(job);

        var targetFile = Path.Combine(_targetDir, "timestamped.txt");
        var targetTime = File.GetLastWriteTimeUtc(targetFile);

        Assert.That(targetTime, Is.EqualTo(originalTime).Within(TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    ///     Verifies sequential execution of multiple jobs.
    /// </summary>
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
            new BackupJob(2, "job2", sourceDir2, targetDir2, BackupType.Complete),
            new BackupJob(1, "job1", sourceDir1, targetDir1, BackupType.Complete)
        };

        _backupService.RunJobsSequential(jobs);

        Assert.That(File.Exists(Path.Combine(targetDir1, "file1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(targetDir2, "file2.txt")), Is.True);
    }

    /// <summary>
    ///     Verifies differential backup behavior.
    /// </summary>
    [Test]
    public void RunJob_DifferentialBackup_OnlyUpdatesDifferentFiles()
    {
        // Create initial complete backup
        var file1 = Path.Combine(_sourceDir, "file1.txt");
        var file2 = Path.Combine(_sourceDir, "file2.txt");
        File.WriteAllText(file1, "content1");
        File.WriteAllText(file2, "content2");

        var job = new BackupJob(1, "diffBackup", _sourceDir, _targetDir, BackupType.Differential);

        _backupService.RunJob(job);

        // Both files should be copied in first differential (treated as complete for empty target)
        Assert.That(File.Exists(Path.Combine(_targetDir, "file1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(_targetDir, "file2.txt")), Is.True);
    }
}
