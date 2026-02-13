using EasySave.Models.Backup;

namespace EasySaveTest;

public class NormalFileTests
{
    private string _testSourceDir = null!;
    private string _testTargetDir = null!;

    [SetUp]
    public void Setup()
    {
        _testSourceDir = Path.Combine(Path.GetTempPath(), "NormalFileSource_" + Guid.NewGuid());
        _testTargetDir = Path.Combine(Path.GetTempPath(), "NormalFileTarget_" + Guid.NewGuid());
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
    public void Constructor_SetsProperties()
    {
        var sourceFile = "C:\\Source\\File.txt";
        var targetFile = "C:\\Target\\File.txt";
        var normalFile = new NormalFile(sourceFile, targetFile, "TestBackup");

        Assert.Multiple(() =>
        {
            Assert.That(normalFile.SourceFile, Is.EqualTo(sourceFile));
            Assert.That(normalFile.TargetFile, Is.EqualTo(targetFile));
            Assert.That(normalFile.BackupName, Is.EqualTo("TestBackup"));
        });
    }

    [Test]
    public void GetSize_ReturnsFileSize()
    {
        var sourceFile = Path.Combine(_testSourceDir, "test.txt");
        File.WriteAllText(sourceFile, "Test content");
        var targetFile = Path.Combine(_testTargetDir, "test.txt");
        var normalFile = new NormalFile(sourceFile, targetFile, "TestBackup");

        var size = normalFile.GetSize();

        Assert.That(size, Is.GreaterThan(0));
    }

    [Test]
    public void GetSize_WithEmptyFile_ReturnsZero()
    {
        var sourceFile = Path.Combine(_testSourceDir, "empty.txt");
        File.WriteAllText(sourceFile, "");
        var targetFile = Path.Combine(_testTargetDir, "empty.txt");
        var normalFile = new NormalFile(sourceFile, targetFile, "TestBackup");

        var size = normalFile.GetSize();

        Assert.That(size, Is.EqualTo(0));
    }

    [Test]
    public void Copy_CopiesFileToTarget()
    {
        var sourceFile = Path.Combine(_testSourceDir, "source.txt");
        var targetFile = Path.Combine(_testTargetDir, "target.txt");
        File.WriteAllText(sourceFile, "Test content");
        var normalFile = new NormalFile(sourceFile, targetFile, "TestBackup");

        normalFile.Copy();

        Assert.That(File.Exists(targetFile), Is.True);
    }

    [Test]
    public void Copy_PreservesFileContent()
    {
        var sourceFile = Path.Combine(_testSourceDir, "source.txt");
        var targetFile = Path.Combine(_testTargetDir, "target.txt");
        var content = "Test content for backup";
        File.WriteAllText(sourceFile, content);
        var normalFile = new NormalFile(sourceFile, targetFile, "TestBackup");

        normalFile.Copy();

        Assert.That(File.ReadAllText(targetFile), Is.EqualTo(content));
    }

    [Test]
    public void Copy_WithLargeFile_CopiesSuccessfully()
    {
        var sourceFile = Path.Combine(_testSourceDir, "large.txt");
        var targetFile = Path.Combine(_testTargetDir, "large.txt");
        var content = new string('A', 10000);
        File.WriteAllText(sourceFile, content);
        var normalFile = new NormalFile(sourceFile, targetFile, "TestBackup");

        normalFile.Copy();

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(targetFile), Is.True);
            Assert.That(new FileInfo(targetFile).Length, Is.EqualTo(new FileInfo(sourceFile).Length));
        });
    }

    [Test]
    public void Copy_WithNonExistentSourceFile_ThrowsException()
    {
        var sourceFile = Path.Combine(_testSourceDir, "nonexistent.txt");
        var targetFile = Path.Combine(_testTargetDir, "target.txt");
        var normalFile = new NormalFile(sourceFile, targetFile, "TestBackup");

        Assert.Throws<FileNotFoundException>(() => normalFile.Copy());
    }
}

