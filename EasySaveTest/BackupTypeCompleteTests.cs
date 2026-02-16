using EasySave.Models.Backup;
using EasySave.Core.Models;

namespace EasySaveTest;

public class BackupTypeCompleteTests
{
    private string _testSourceDir = null!;
    private string _testTargetDir = null!;

    [SetUp]
    public void Setup()
    {
        _testSourceDir = Path.Combine(Path.GetTempPath(), "Source_" + Guid.NewGuid());
        _testTargetDir = Path.Combine(Path.GetTempPath(), "Target_" + Guid.NewGuid());
        Directory.CreateDirectory(_testSourceDir);
        Directory.CreateDirectory(_testTargetDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testSourceDir))
            Directory.Delete(_testSourceDir, true);
        if (Directory.Exists(_testTargetDir))
            Directory.Delete(_testTargetDir, true);
    }

    [Test]
    public void Constructor_CreatesInstance()
    {
        var selector = new BackupTypeComplete(_testSourceDir, _testTargetDir, "TestBackup");

        Assert.That(selector, Is.Not.Null);
    }

    [Test]
    public void GetFilesToBackup_WithEmptyDirectory_ReturnsEmptyList()
    {
        var selector = new BackupTypeComplete(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetFilesToBackup_WithSingleFile_ReturnsOneFile()
    {
        File.WriteAllText(Path.Combine(_testSourceDir, "test.txt"), "content");
        var selector = new BackupTypeComplete(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetFilesToBackup_WithMultipleFiles_ReturnsAllFiles()
    {
        File.WriteAllText(Path.Combine(_testSourceDir, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(_testSourceDir, "file2.txt"), "content2");
        File.WriteAllText(Path.Combine(_testSourceDir, "file3.txt"), "content3");
        var selector = new BackupTypeComplete(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetFilesToBackup_WithSubdirectories_ReturnsAllFiles()
    {
        var subDir = Path.Combine(_testSourceDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(_testSourceDir, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(subDir, "file2.txt"), "content2");
        var selector = new BackupTypeComplete(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetFilesToBackup_ReturnsNormalFileInstances()
    {
        File.WriteAllText(Path.Combine(_testSourceDir, "test.txt"), "content");
        var selector = new BackupTypeComplete(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result[0], Is.TypeOf<NormalFile>());
    }

    [Test]
    public void GetFilesToBackup_FilesHaveCorrectTargetPath()
    {
        File.WriteAllText(Path.Combine(_testSourceDir, "test.txt"), "content");
        var selector = new BackupTypeComplete(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result[0].TargetFile, Does.Contain(_testTargetDir));
    }
}

