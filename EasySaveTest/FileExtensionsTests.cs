using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

public class FileExtensionsTests
{
    [Test]
    public void GetAllSize_WithEmptyList_ReturnsZero()
    {
        var files = new List<IFile>();

        var result = files.GetAllSize();

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetAllSize_WithSingleFile_ReturnsFileSize()
    {
        var files = new List<IFile>
        {
            new FakeFile(1024)
        };

        var result = files.GetAllSize();

        Assert.That(result, Is.EqualTo(1024));
    }

    [Test]
    public void GetAllSize_WithMultipleFiles_ReturnsSumOfSizes()
    {
        var files = new List<IFile>
        {
            new FakeFile(1024),
            new FakeFile(2048),
            new FakeFile(512)
        };

        var result = files.GetAllSize();

        Assert.That(result, Is.EqualTo(3584));
    }

    [Test]
    public void GetAllSize_WithZeroSizeFiles_ReturnsZero()
    {
        var files = new List<IFile>
        {
            new FakeFile(0),
            new FakeFile(0)
        };

        var result = files.GetAllSize();

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetAllSize_WithLargeFiles_ReturnsCorrectSum()
    {
        var files = new List<IFile>
        {
            new FakeFile(1073741824), // 1 GB
            new FakeFile(2147483648)  // 2 GB
        };

        var result = files.GetAllSize();

        Assert.That(result, Is.EqualTo(3221225472));
    }

    private class FakeFile : IFile
    {
        private readonly long _size;

        public FakeFile(long size)
        {
            _size = size;
        }

        public string SourceFile => "C:\\Source\\File.txt";
        public string TargetFile => "C:\\Target\\File.txt";
        public string BackupName => "TestBackup";

        public void Copy()
        {
            // No-op for testing
        }

        public long GetSize()
        {
            return _size;
        }
    }
}

