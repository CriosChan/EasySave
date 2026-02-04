using System.Text.Json;
using EasyLog;

namespace EasyLogTest;

public class Tests
{
    private AbstractLogger<FakeLogObject> _logger = new JsonLogger<FakeLogObject>("./");
    [SetUp]
    public void Setup()
    {
    }

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
        if(File.Exists(logPath))
            File.Delete(logPath);
        // Log the objet
        _logger.Log(log);
        // Check if the log file exists
        Assert.That(File.Exists(logPath), Is.EqualTo(true));
        // Get the file content
        string json = File.ReadAllText(logPath);
        List<FakeLogObject> logs = JsonSerializer.Deserialize<List<FakeLogObject>>(json) ?? new();
        Assert.That(logs.Count, Is.GreaterThan(0));

        // Check if the first one is equal to the object we gave
        Assert.That(logs[0], Is.EqualTo(log));
        
        // Change some values 
        log.Name = "Test2";
        log.FileSource = "./Test2.txt";
        log.FileTarget = "./Test2.txt";
        log.Time = DateTime.Now;
        // Log a second object
        _logger.Log(log);
        // Read again
        json = File.ReadAllText(logPath);
        logs = JsonSerializer.Deserialize<List<FakeLogObject>>(json) ?? new();
        Assert.That(logs.Count, Is.GreaterThanOrEqualTo(2));

        // And check again if the second log is correct
        Assert.That(logs[1], Is.EqualTo(log));
    }
}