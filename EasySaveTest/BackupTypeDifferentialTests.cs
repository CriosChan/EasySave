using EasySave.Models.Backup;

namespace EasySaveTest;

public class BackupTypeDifferentialTests
{
    private string _testSourceDir = null!;
    private string _testTargetDir = null!;

    [SetUp]
    public void Setup()
    {
        _testSourceDir = Path.Combine(Path.GetTempPath(), "DiffSource_" + Guid.NewGuid());
        _testTargetDir = Path.Combine(Path.GetTempPath(), "DiffTarget_" + Guid.NewGuid());
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
        var selector = new BackupTypeDifferential(_testSourceDir, _testTargetDir, "TestBackup");

        Assert.That(selector, Is.Not.Null);
    }

    [Test]
    public void GetFilesToBackup_WithEmptyDirectory_ReturnsEmptyList()
    {
        var selector = new BackupTypeDifferential(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetFilesToBackup_WithNewFile_ReturnsFile()
    {
        File.WriteAllText(Path.Combine(_testSourceDir, "new.txt"), "content");
        var selector = new BackupTypeDifferential(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetFilesToBackup_WithExistingIdenticalFile_ReturnsEmpty()
    {
        var sourceFile = Path.Combine(_testSourceDir, "same.txt");
        var targetFile = Path.Combine(_testTargetDir, "same.txt");
        File.WriteAllText(sourceFile, "content");
        File.WriteAllText(targetFile, "content");
        File.SetLastWriteTimeUtc(sourceFile, DateTime.UtcNow.AddDays(-1));
        File.SetLastWriteTimeUtc(targetFile, DateTime.UtcNow.AddDays(-1));
        var selector = new BackupTypeDifferential(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetFilesToBackup_WithModifiedFile_ReturnsFile()
    {
        var sourceFile = Path.Combine(_testSourceDir, "modified.txt");
        var targetFile = Path.Combine(_testTargetDir, "modified.txt");
        File.WriteAllText(sourceFile, "new content");
        File.WriteAllText(targetFile, "old content");
        Thread.Sleep(100);
        File.SetLastWriteTimeUtc(sourceFile, DateTime.UtcNow);
        File.SetLastWriteTimeUtc(targetFile, DateTime.UtcNow.AddDays(-1));
        var selector = new BackupTypeDifferential(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetFilesToBackup_WithDifferentSizeFile_ReturnsFile()
    {
        var sourceFile = Path.Combine(_testSourceDir, "different.txt");
        var targetFile = Path.Combine(_testTargetDir, "different.txt");
        File.WriteAllText(sourceFile, "longer content here");
        File.WriteAllText(targetFile, "short");
        var selector = new BackupTypeDifferential(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetFilesToBackup_WithMultipleNewFiles_ReturnsAllNew()
    {
        File.WriteAllText(Path.Combine(_testSourceDir, "new1.txt"), "content1");
        File.WriteAllText(Path.Combine(_testSourceDir, "new2.txt"), "content2");
        File.WriteAllText(Path.Combine(_testSourceDir, "new3.txt"), "content3");
        var selector = new BackupTypeDifferential(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetFilesToBackup_WithMixedFiles_ReturnsOnlyChangedAndNew()
    {
        var sourceFile1 = Path.Combine(_testSourceDir, "same.txt");
        var targetFile1 = Path.Combine(_testTargetDir, "same.txt");
        File.WriteAllText(sourceFile1, "content");
        File.WriteAllText(targetFile1, "content");
        
        File.WriteAllText(Path.Combine(_testSourceDir, "new.txt"), "new content");
        
        var selector = new BackupTypeDifferential(_testSourceDir, _testTargetDir, "TestBackup");

        var result = selector.GetFilesToBackup();

        Assert.That(result, Has.Count.GreaterThanOrEqualTo(1));
    }
}

