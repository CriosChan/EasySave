using System.Text.Json;
using EasySave.Controller;
using EasySave.Models;
using EasySave.Services;
using EasySave.Utils;

namespace EasySaveTest;

[TestFixture]
public class CommandControllerTests
{
    private string _rootDir = null!;
    private string _configDir = null!;
    private string _logDir = null!;

    private JobRepository _repo = null!;
    private StateFileService _state = null!;
    private BackupService _backup = null!;

    [SetUp]
    public void Setup()
    {
        _rootDir = Path.Combine(Path.GetTempPath(), "EasySave_CommandControllerTests_" + Path.GetRandomFileName());
        _configDir = Path.Combine(_rootDir, "config");
        _logDir = Path.Combine(_rootDir, "log");

        Directory.CreateDirectory(_configDir);
        Directory.CreateDirectory(_logDir);

        _repo = new JobRepository(_configDir);
        _state = new StateFileService(_configDir);
        _backup = new BackupService(_logDir, _state);
    }

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

    private void SaveJobs(params BackupJob[] jobs) => _repo.Save(jobs.ToList());

    private (int exitCode, string output) RunWithOutput(params string[] args)
    {
        TextWriter originalOut = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);

        try
        {
            int code = CommandController.Run(args, _repo, _backup, _state);
            return (code, sw.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private List<BackupJobState> ReadStateFile()
    {
        string statePath = Path.Combine(_configDir, "state.json");
        string json = File.ReadAllText(statePath);
        return JsonSerializer.Deserialize<List<BackupJobState>>(json, JsonFile.Options) ?? new List<BackupJobState>();
    }

    private List<LogEntry> ReadAllLogEntries()
    {
        if (!Directory.Exists(_logDir))
            return new List<LogEntry>();

        var entries = new List<LogEntry>();
        foreach (string file in Directory.GetFiles(_logDir, "*.json"))
        {
            foreach (string line in File.ReadAllLines(file))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(line);
                    if (entry != null)
                        entries.Add(entry);
                }
                catch
                {
                    // Ignore malformed lines so we can still assert on what we did parse.
                }
            }
        }

        return entries;
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int idx = 0;

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

    private static BackupJob MakeJob(int id, string name, string sourceDir, string targetDir, BackupType type = BackupType.Complete)
        => new() { Id = id, Name = name, SourceDirectory = sourceDir, TargetDirectory = targetDir, Type = type };

    [Test]
    public void Run_NoArgs_PrintsUsageAndReturns1()
    {
        var (code, output) = RunWithOutput(Array.Empty<string>());

        Assert.That(code, Is.EqualTo(1));
        Assert.That(output, Does.Contain("Usage:"));
        Assert.That(output, Does.Contain("EasySave.exe 1-3"));
        Assert.That(output, Does.Contain("EasySave.exe 1;3"));
        Assert.That(output, Does.Contain("EasySave.exe 2"));
    }

    [Test]
    public void Run_InvalidArgs_PrintsUsageAndReturns1()
    {
        var (code, output) = RunWithOutput("abc");

        Assert.That(code, Is.EqualTo(1));
        Assert.That(output, Does.Contain("Usage:"));
    }

    [Test]
    public void Run_NoJobsConfigured_Returns1_AndDoesNotCreateStateFile()
    {
        // No jobs.json saved -> repository.Load() returns an empty list
        var (code, output) = RunWithOutput("1");

        Assert.That(code, Is.EqualTo(1));
        Assert.That(output, Does.Contain("No backup job configured."));
        Assert.That(File.Exists(Path.Combine(_configDir, "state.json")), Is.False);
    }

    [Test]
    public void Run_SingleId_RunsJob_UpdatesState_AndWritesLogs()
    {
        string src = Path.Combine(_rootDir, "src1");
        string dst = Path.Combine(_rootDir, "dst1");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(dst);

        SaveJobs(MakeJob(1, "job1", src, dst));

        var (code, output) = RunWithOutput("1");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(output, Does.Contain("Running job 1 - job1..."));

        List<BackupJobState> states = ReadStateFile();
        BackupJobState s1 = states.Single(s => s.JobId == 1);
        Assert.That(s1.State, Is.EqualTo(JobRunState.Completed));

        List<LogEntry> logs = ReadAllLogEntries();
        Assert.That(logs.Count(e => e.BackupName == "job1"), Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void Run_Range_RunsAllJobsInIncreasingOrder()
    {
        string src1 = Path.Combine(_rootDir, "src1");
        string dst1 = Path.Combine(_rootDir, "dst1");
        string src2 = Path.Combine(_rootDir, "src2");
        string dst2 = Path.Combine(_rootDir, "dst2");
        string src3 = Path.Combine(_rootDir, "src3");
        string dst3 = Path.Combine(_rootDir, "dst3");

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

        int i1 = output.IndexOf("Running job 1 - job1...", StringComparison.Ordinal);
        int i2 = output.IndexOf("Running job 2 - job2...", StringComparison.Ordinal);
        int i3 = output.IndexOf("Running job 3 - job3...", StringComparison.Ordinal);

        Assert.That(i1, Is.GreaterThanOrEqualTo(0));
        Assert.That(i2, Is.GreaterThanOrEqualTo(0));
        Assert.That(i3, Is.GreaterThanOrEqualTo(0));
        Assert.That(i1, Is.LessThan(i2));
        Assert.That(i2, Is.LessThan(i3));

        List<BackupJobState> states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Completed));
        Assert.That(states.Single(s => s.JobId == 2).State, Is.EqualTo(JobRunState.Completed));
        Assert.That(states.Single(s => s.JobId == 3).State, Is.EqualTo(JobRunState.Completed));
    }

    [Test]
    public void Run_SemicolonList_RunsOnlySpecifiedJobs()
    {
        string src1 = Path.Combine(_rootDir, "src1");
        string dst1 = Path.Combine(_rootDir, "dst1");
        string src2 = Path.Combine(_rootDir, "src2");
        string dst2 = Path.Combine(_rootDir, "dst2");
        string src3 = Path.Combine(_rootDir, "src3");
        string dst3 = Path.Combine(_rootDir, "dst3");

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

        List<BackupJobState> states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Completed));
        Assert.That(states.Single(s => s.JobId == 2).State, Is.EqualTo(JobRunState.Inactive));
        Assert.That(states.Single(s => s.JobId == 3).State, Is.EqualTo(JobRunState.Completed));
    }

    [Test]
    public void Run_DeduplicatesIds_AndDoesNotRunSameJobTwice()
    {
        string src1 = Path.Combine(_rootDir, "src1");
        string dst1 = Path.Combine(_rootDir, "dst1");
        string src2 = Path.Combine(_rootDir, "src2");
        string dst2 = Path.Combine(_rootDir, "dst2");

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

        List<BackupJobState> states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Completed));
        Assert.That(states.Single(s => s.JobId == 2).State, Is.EqualTo(JobRunState.Completed));
    }

    [Test]
    public void Run_UnknownJobId_PrintsNotFound_AndKeepsStateInactive()
    {
        string src1 = Path.Combine(_rootDir, "src1");
        string dst1 = Path.Combine(_rootDir, "dst1");
        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(dst1);

        SaveJobs(MakeJob(1, "job1", src1, dst1));

        var (code, output) = RunWithOutput("99");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(output, Does.Contain("Job 99 not found."));
        Assert.That(output, Does.Not.Contain("Running job 99"));

        List<BackupJobState> states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Inactive));
    }

    [Test]
    public void Run_SkipsJob_WhenSourceDirectoryIsMissing()
    {
        // Source does not exist; target does.
        string src = Path.Combine(_rootDir, "missing_src");
        string dst = Path.Combine(_rootDir, "dst1");
        Directory.CreateDirectory(dst);

        SaveJobs(MakeJob(1, "job1", src, dst));

        var (code, output) = RunWithOutput("1");

        Assert.That(code, Is.EqualTo(0));
        Assert.That(output, Does.Contain("Job 1 skipped: source directory not found."));
        Assert.That(output, Does.Not.Contain("Running job 1 - job1..."));

        // No BackupService.RunJob() -> no log files created.
        Assert.That(Directory.GetFiles(_logDir, "*.json").Length, Is.EqualTo(0));

        // State is initialized but job was not executed -> still Inactive.
        List<BackupJobState> states = ReadStateFile();
        Assert.That(states.Single(s => s.JobId == 1).State, Is.EqualTo(JobRunState.Inactive));
    }

    [Test]
    public void Run_SupportsSplitArgumentsLikeShellWouldProvide()
    {
        // The controller joins args together, so passing ["1", ";", "3"] should work.
        string src1 = Path.Combine(_rootDir, "src1");
        string dst1 = Path.Combine(_rootDir, "dst1");
        string src3 = Path.Combine(_rootDir, "src3");
        string dst3 = Path.Combine(_rootDir, "dst3");
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
