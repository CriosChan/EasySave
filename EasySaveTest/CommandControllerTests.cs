using System.Globalization;
using System.Text.Json;
using EasySave.Services;
using EasySave.Core.Models;
using EasySave.Platform.IO;
using EasySave.Data.Logging;
using EasySave.Data.Persistence;
using EasySave.Cli;

namespace EasySaveTest;

/// <summary>
///     Integration tests for the command-line controller.
/// </summary>
[TestFixture]
public class CommandControllerTests
{
    /// <summary>
    ///     Prepares the temporary test environment.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        _rootDir = Path.Combine(Path.GetTempPath(), "EasySave_CommandControllerTests_" + Path.GetRandomFileName());
        _configDir = Path.Combine(_rootDir, "config");
        _logDir = Path.Combine(_rootDir, "log");

        Directory.CreateDirectory(_configDir);
        Directory.CreateDirectory(_logDir);

        _repo = new JobRepository(_configDir);
        _jobService = new JobService(_repo);
        _state = new StateFileService(_configDir);
        _paths = new PathService();

        var logWriter = new JsonLogWriter<LogEntry>(_logDir);
        var fileSelector = new BackupFileSelector(_paths);
        var directoryPreparer = new BackupDirectoryPreparer(logWriter, _paths);
        var fileCopier = new FileCopier();
        _validator = new JobValidator(_paths);

        _backup = new BackupService(
            logWriter,
            _state,
            _paths,
            fileSelector,
            directoryPreparer,
            fileCopier,
            _validator,
            new NullProgressReporter());
    }

    /// <summary>
    ///     Cleans up the temporary test environment.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        try
        {
            Directory.Delete(_rootDir, true);
        }
        catch (Exception ex)
        {
            // Best-effort cleanup: log failure but do not fail the test.
            TestContext.WriteLine($"Failed to delete temp directory '{_rootDir}': {ex}");
        }
    }

    private string _rootDir = null!;
    private string _configDir = null!;
    private string _logDir = null!;

    private JobRepository _repo = null!;
    private JobService _jobService = null!;
    private StateFileService _state = null!;
    private BackupService _backup = null!;
    private PathService _paths = null!;
    private JobValidator _validator = null!;

    /// <summary>
    ///     Saves a list of jobs in the repository.
    /// </summary>
    /// <param name="jobs">Jobs to save.</param>
    private void SaveJobs(params BackupJob[] jobs)
    {
        _repo.SaveAll(jobs.ToList());
    }

    /// <summary>
    ///     Executes the controller while capturing console output.
    /// </summary>
    /// <param name="args">CLI arguments.</param>
    /// <returns>Exit code and captured output.</returns>
    private (int exitCode, string output) RunWithOutput(params string[] args)
    {
        var originalOut = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);

        try
        {
            var code = CommandController.Run(args, _jobService, _backup, _state, _validator);
            return (code, sw.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Reads and deserializes the state file.
    /// </summary>
    /// <returns>List of states.</returns>
    private List<BackupJobState> ReadStateFile()
    {
        var statePath = Path.Combine(_configDir, "state.json");
        var json = File.ReadAllText(statePath);
        return JsonSerializer.Deserialize<List<BackupJobState>>(json, JsonFile.Options) ?? new List<BackupJobState>();
    }

    /// <summary>
    ///     Reads all available JSON log entries.
    /// </summary>
    /// <returns>List of log entries.</returns>
    private List<LogEntry> ReadAllLogEntries()
    {
        if (!Directory.Exists(_logDir))
            return new List<LogEntry>();

        var entries = new List<LogEntry>();
        foreach (var file in Directory.GetFiles(_logDir, "*.json"))
        foreach (var line in File.ReadAllLines(file))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var entry = JsonSerializer.Deserialize<LogEntry>(line);
                if (entry != null)
                    entries.Add(entry);
            }
            catch
            {
                // Ignore malformed lines so we can still assert on what we did parse.
            }
        }

        return entries;
    }

    /// <summary>
    ///     Counts occurrences of a value within a text.
    /// </summary>
    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var idx = 0;

        while (true)
        {
            idx = text.IndexOf(value, idx, StringComparison.Ordinal);
            if (idx < 0)
                break;

            count++;
            idx += value.Length;
        }

        return count;
    }

    /// <summary>
    ///     Creates a backup job for tests.
    /// </summary>
    private static BackupJob MakeJob(int id, string name, string sourceDir, string targetDir,
        BackupType type = BackupType.Complete)
    {
        return new BackupJob(id, name, sourceDir, targetDir, type);
    }

    /// <summary>
    ///     Verifies usage is printed when no argument is provided.
    /// </summary>
    [Test]
    public void Run_NoArgs_PrintsUsageAndReturns1()
    {
        var (code, output) = RunWithOutput();

        Assert.That(code, Is.EqualTo(1));
        Assert.That(output, Does.Contain("Usage:"));
        Assert.That(output, Does.Contain("EasySave.exe 1-3"));
        Assert.That(output, Does.Contain("EasySave.exe 1;3"));
        Assert.That(output, Does.Contain("EasySave.exe 2"));
    }

    /// <summary>
    ///     Verifies usage is printed for invalid arguments.
    /// </summary>
    [Test]
    public void Run_InvalidArgs_PrintsUsageAndReturns1()
    {
        var (code, output) = RunWithOutput("abc");

        Assert.That(code, Is.EqualTo(1));
        Assert.That(output, Does.Contain("Usage:"));
    }

    /// <summary>
    ///     Verifies that no configured job returns 1 and does not create state.json.
    /// </summary>
    [Test]
    public void Run_NoJobsConfigured_Returns1_AndDoesNotCreateStateFile()
    {
        // No jobs.json saved -> repository.Load() returns an empty list
        var (code, output) = RunWithOutput("1");

        Assert.That(code, Is.EqualTo(1));
        Assert.That(output, Does.Contain("No backup job configured."));
        Assert.That(File.Exists(Path.Combine(_configDir, "state.json")), Is.False);
    }

    /// <summary>
    ///     Verifies running a single job updates state and writes logs.
    /// </summary>
    [Test]
    public void Run_SingleId_RunsJob_UpdatesState_AndWritesLogs()
    {
        var src = Path.Combine(_rootDir, "src1");
        var dst = Path.Combine(_rootDir, "dst1");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(dst);

        SaveJobs(MakeJob(1, "job1", src, dst));

        var (code, output) = RunWithOutput("1");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(output, Does.Contain("Running job 1 - job1..."));

        var states = ReadStateFile();
        var s1 = states.Single(s => s.JobId == 1);
        Assert.That(s1.State, Is.EqualTo(JobRunState.Completed));

        var logs = ReadAllLogEntries();
        Assert.That(logs.Count(e => e.BackupName == "job1"), Is.GreaterThanOrEqualTo(2));
    }

    /// <summary>
    ///     Verifies running a range of IDs in order.
    /// </summary>
    [Test]
    public void Run_Range_RunsAllJobsInIncreasingOrder()
    {
        var src1 = Path.Combine(_rootDir, "src1");
        var dst1 = Path.Combine(_rootDir, "dst1");
        var src2 = Path.Combine(_rootDir, "src2");
        var dst2 = Path.Combine(_rootDir, "dst2");
        var src3 = Path.Combine(_rootDir, "src3");
        var dst3 = Path.Combine(_rootDir, "dst3");

        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(dst1);
        Directory.CreateDirectory(src2);
        Directory.CreateDirectory(dst2);
        Directory.CreateDirectory(src3);
        Directory.CreateDirectory(dst3);

        SaveJobs(
            MakeJob(1, "job1", src1, dst1),
            MakeJob(2, "job2", src2, dst2),
            MakeJob(3, "job3", src3, dst3)
        );

        var (code, output) = RunWithOutput("1-3");

        Assert.That(code, Is.EqualTo(0));

        var i1 = output.IndexOf("Running job 1 - job1...", StringComparison.Ordinal);
        var i2 = output.IndexOf("Running job 2 - job2...", StringComparison.Ordinal);
        var i3 = output.IndexOf("Running job 3 - job3...", StringComparison.Ordinal);

        Assert.That(i1, Is.GreaterThanOrEqualTo(0));
        Assert.That(i2, Is.GreaterThanOrEqualTo(0));
        Assert.That(i3, Is.GreaterThanOrEqualTo(0));
        Assert.That(i1, Is.LessThan(i2));
        Assert.That(i2, Is.LessThan(i3));

        var states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Completed));
        Assert.That(states.Single(s => s.JobId == 2).State, Is.EqualTo(JobRunState.Completed));
        Assert.That(states.Single(s => s.JobId == 3).State, Is.EqualTo(JobRunState.Completed));
    }

    /// <summary>
    ///     Verifies running a list separated by semicolons.
    /// </summary>
    [Test]
    public void Run_SemicolonList_RunsOnlySpecifiedJobs()
    {
        var src1 = Path.Combine(_rootDir, "src1");
        var dst1 = Path.Combine(_rootDir, "dst1");
        var src2 = Path.Combine(_rootDir, "src2");
        var dst2 = Path.Combine(_rootDir, "dst2");
        var src3 = Path.Combine(_rootDir, "src3");
        var dst3 = Path.Combine(_rootDir, "dst3");

        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(dst1);
        Directory.CreateDirectory(src2);
        Directory.CreateDirectory(dst2);
        Directory.CreateDirectory(src3);
        Directory.CreateDirectory(dst3);

        SaveJobs(
            MakeJob(1, "job1", src1, dst1),
            MakeJob(2, "job2", src2, dst2),
            MakeJob(3, "job3", src3, dst3)
        );

        var (code, output) = RunWithOutput("1;3");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(output, Does.Contain("Running job 1 - job1..."));
        Assert.That(output, Does.Not.Contain("Running job 2 - job2..."));
        Assert.That(output, Does.Contain("Running job 3 - job3..."));

        var states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Completed));
        Assert.That(states.Single(s => s.JobId == 2).State, Is.EqualTo(JobRunState.Inactive));
        Assert.That(states.Single(s => s.JobId == 3).State, Is.EqualTo(JobRunState.Completed));
    }

    /// <summary>
    ///     Verifies deduplication of input IDs.
    /// </summary>
    [Test]
    public void Run_DeduplicatesIds_AndDoesNotRunSameJobTwice()
    {
        var src1 = Path.Combine(_rootDir, "src1");
        var dst1 = Path.Combine(_rootDir, "dst1");
        var src2 = Path.Combine(_rootDir, "src2");
        var dst2 = Path.Combine(_rootDir, "dst2");

        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(dst1);
        Directory.CreateDirectory(src2);
        Directory.CreateDirectory(dst2);

        SaveJobs(
            MakeJob(1, "job1", src1, dst1),
            MakeJob(2, "job2", src2, dst2)
        );

        var (code, output) = RunWithOutput("1;1;2");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(CountOccurrences(output, "Running job 1 - job1..."), Is.EqualTo(1));
        Assert.That(CountOccurrences(output, "Running job 2 - job2..."), Is.EqualTo(1));

        var states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Completed));
        Assert.That(states.Single(s => s.JobId == 2).State, Is.EqualTo(JobRunState.Completed));
    }

    /// <summary>
    ///     Verifies behavior for an unknown job ID.
    /// </summary>
    [Test]
    public void Run_UnknownJobId_PrintsNotFound_AndKeepsStateInactive()
    {
        var src1 = Path.Combine(_rootDir, "src1");
        var dst1 = Path.Combine(_rootDir, "dst1");
        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(dst1);

        SaveJobs(MakeJob(1, "job1", src1, dst1));

        var (code, output) = RunWithOutput("99");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(output, Does.Contain("Job 99 not found."));
        Assert.That(output, Does.Not.Contain("Running job 99"));

        var states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Inactive));
    }

    /// <summary>
    ///     Verifies a job is skipped when the source folder is missing.
    /// </summary>
    [Test]
    public void Run_SkipsJob_WhenSourceDirectoryIsMissing()
    {
        // Source does not exist; target does.
        var src = Path.Combine(_rootDir, "missing_src");
        var dst = Path.Combine(_rootDir, "dst1");
        Directory.CreateDirectory(dst);

        SaveJobs(MakeJob(1, "job1", src, dst));

        var (code, output) = RunWithOutput("1");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(output, Does.Contain("Job 1 skipped: source directory not found."));
        Assert.That(output, Does.Not.Contain("Running job 1 - job1..."));

        // No BackupService.RunJob() -> no log files created.
        Assert.That(Directory.GetFiles(_logDir, "*.json").Length, Is.EqualTo(0));

        // State is initialized but job was not executed -> still Inactive.
        var states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Inactive));
    }

    /// <summary>
    ///     Verifies support for arguments split by the shell.
    /// </summary>
    [Test]
    public void Run_SupportsSplitArgumentsLikeShellWouldProvide()
    {
        // The controller joins args together, so passing ["1", ";", "3"] should work.
        var src1 = Path.Combine(_rootDir, "src1");
        var dst1 = Path.Combine(_rootDir, "dst1");
        var src3 = Path.Combine(_rootDir, "src3");
        var dst3 = Path.Combine(_rootDir, "dst3");
        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(dst1);
        Directory.CreateDirectory(src3);
        Directory.CreateDirectory(dst3);

        SaveJobs(
            MakeJob(1, "job1", src1, dst1),
            MakeJob(3, "job3", src3, dst3)
        );

        var (code, output) = RunWithOutput("1", ";", "3");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(output, Does.Contain("Running job 1 - job1..."));
        Assert.That(output, Does.Contain("Running job 3 - job3..."));
    }
}
