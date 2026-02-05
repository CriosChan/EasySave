using System.Text.Json;
using EasyLog;

namespace EasyLogTest;

/// <summary>
/// Unit tests for JSON log creation.
/// </summary>
public class Tests
{
    private AbstractLogger<FakeLogObject> _logger = new JsonLogger<FakeLogObject>("./");
    /// <summary>
    /// Shared test setup (empty for now).
    /// </summary>
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Verifies that logging creates a file and contains entries.
    /// </summary>
    [Test]
    public void TestLogCreation()
    {
        // Fake log object
        FakeLogObject log = new FakeLogObject
        {
            Name = "Test1",
            FileSize = 999,
            FileSource = "./Test1.txt",
            FileTarget = "./Test1.txt",
            Time = DateTime.Now
        };

        string logPath = Path.Join("./", DateTime.Now.ToString("yyyy-MM-dd") + ".json");
        // If the file already exists remove it.
        if (File.Exists(logPath))
            File.Delete(logPath);
        // Log the object
        _logger.Log(log);
        // Check if the log file exists
        Assert.That(File.Exists(logPath), Is.EqualTo(true));
        // Get the file content
        IEnumerable<string> lines = File.ReadLines(logPath);
        Assert.That(lines.Count(), Is.Not.EqualTo(0));
        // Deserialize it
        FakeLogObject readedLog = JsonSerializer.Deserialize<FakeLogObject>(lines.ElementAt(0));
        // Check if the first one is equal to the object we gave
        Assert.That(readedLog, Is.EqualTo(log));

        // Change some values 
        log.Name = "Test2";
        log.FileSource = "./Test2.txt";
        log.FileTarget = "./Test2.txt";
        log.Time = DateTime.Now;
        // Log a second object
        _logger.Log(log);
        // Read again
        lines = File.ReadLines(logPath);
        // And check again if the second log is correct
        readedLog = JsonSerializer.Deserialize<FakeLogObject>(lines.ElementAt(1));
        Assert.That(readedLog, Is.EqualTo(log));
    }
}
