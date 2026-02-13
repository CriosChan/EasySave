using EasySave.Models.Utils;

namespace EasySaveTest;

public class JsonFileTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void Setup()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "JsonFileTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    public void ReadOrDefault_WithNonExistentFile_ReturnsDefaultValue()
    {
        var path = Path.Combine(_testDirectory, "nonexistent.json");
        var defaultValue = new TestData { Name = "Default", Value = 100 };

        var result = JsonFile.ReadOrDefault(path, defaultValue);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Default"));
            Assert.That(result.Value, Is.EqualTo(100));
        });
    }

    [Test]
    public void WriteAtomic_CreatesFile()
    {
        var path = Path.Combine(_testDirectory, "test.json");
        var data = new TestData { Name = "Test", Value = 42 };

        JsonFile.WriteAtomic(path, data);

        Assert.That(File.Exists(path), Is.True);
    }

    [Test]
    public void WriteAtomic_ThenRead_ReturnsCorrectData()
    {
        var path = Path.Combine(_testDirectory, "test.json");
        var data = new TestData { Name = "Test", Value = 42 };

        JsonFile.WriteAtomic(path, data);
        var result = JsonFile.ReadOrDefault(path, new TestData());

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Test"));
            Assert.That(result.Value, Is.EqualTo(42));
        });
    }

    [Test]
    public void WriteAtomic_OverwritesExistingFile()
    {
        var path = Path.Combine(_testDirectory, "test.json");
        var data1 = new TestData { Name = "First", Value = 1 };
        var data2 = new TestData { Name = "Second", Value = 2 };

        JsonFile.WriteAtomic(path, data1);
        JsonFile.WriteAtomic(path, data2);
        var result = JsonFile.ReadOrDefault(path, new TestData());

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Second"));
            Assert.That(result.Value, Is.EqualTo(2));
        });
    }

    [Test]
    public void WriteAtomic_CreatesDirectory_IfNotExists()
    {
        var subdirPath = Path.Combine(_testDirectory, "subdir", "nested");
        var path = Path.Combine(subdirPath, "test.json");
        var data = new TestData { Name = "Test", Value = 42 };

        JsonFile.WriteAtomic(path, data);

        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(subdirPath), Is.True);
            Assert.That(File.Exists(path), Is.True);
        });
    }

    [Test]
    public void ReadOrDefault_WithInvalidJson_ReturnsDefaultValue()
    {
        var path = Path.Combine(_testDirectory, "invalid.json");
        File.WriteAllText(path, "{ invalid json }");
        var defaultValue = new TestData { Name = "Default", Value = 100 };

        var result = JsonFile.ReadOrDefault(path, defaultValue);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Default"));
            Assert.That(result.Value, Is.EqualTo(100));
        });
    }

    [Test]
    public void WriteAtomic_WithList_SerializesCorrectly()
    {
        var path = Path.Combine(_testDirectory, "list.json");
        var data = new List<TestData>
        {
            new() { Name = "Item1", Value = 1 },
            new() { Name = "Item2", Value = 2 }
        };

        JsonFile.WriteAtomic(path, data);
        var result = JsonFile.ReadOrDefault(path, new List<TestData>());

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Item1"));
            Assert.That(result[1].Name, Is.EqualTo("Item2"));
        });
    }

    [Test]
    public void ReadOrDefault_WithEmptyFile_ReturnsDefaultValue()
    {
        var path = Path.Combine(_testDirectory, "empty.json");
        File.WriteAllText(path, "");
        var defaultValue = new TestData { Name = "Default", Value = 100 };

        var result = JsonFile.ReadOrDefault(path, defaultValue);

        Assert.That(result.Name, Is.EqualTo("Default"));
    }

    private class TestData
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}

