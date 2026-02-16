using EasySave.Core.Models;

namespace EasySaveTest;

public class LogEntryTests
{
    [Test]
    public void Constructor_SetsDefaultTimestamp()
    {
        var entry = new LogEntry();

        Assert.That(entry.Timestamp, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void BackupName_CanBeSet()
    {
        var entry = new LogEntry { BackupName = "TestBackup" };

        Assert.That(entry.BackupName, Is.EqualTo("TestBackup"));
    }

    [Test]
    public void SourcePath_CanBeSet()
    {
        var entry = new LogEntry { SourcePath = "C:\\Source\\File.txt" };

        Assert.That(entry.SourcePath, Is.EqualTo("C:\\Source\\File.txt"));
    }

    [Test]
    public void TargetPath_CanBeSet()
    {
        var entry = new LogEntry { TargetPath = "C:\\Target\\File.txt" };

        Assert.That(entry.TargetPath, Is.EqualTo("C:\\Target\\File.txt"));
    }

    [Test]
    public void FileSizeBytes_CanBeSet()
    {
        var entry = new LogEntry { FileSizeBytes = 1024 };

        Assert.That(entry.FileSizeBytes, Is.EqualTo(1024));
    }

    [Test]
    public void TransferTimeMs_CanBeSet()
    {
        var entry = new LogEntry { TransferTimeMs = 500 };

        Assert.That(entry.TransferTimeMs, Is.EqualTo(500));
    }

    [Test]
    public void CryptingTimeMs_DefaultsToZero()
    {
        var entry = new LogEntry();

        Assert.That(entry.CryptingTimeMs, Is.EqualTo(0));
    }

    [Test]
    public void CryptingTimeMs_CanBeSet()
    {
        var entry = new LogEntry { CryptingTimeMs = 250 };

        Assert.That(entry.CryptingTimeMs, Is.EqualTo(250));
    }

    [Test]
    public void ErrorMessage_CanBeNull()
    {
        var entry = new LogEntry { ErrorMessage = null };

        Assert.That(entry.ErrorMessage, Is.Null);
    }

    [Test]
    public void ErrorMessage_CanBeSet()
    {
        var entry = new LogEntry { ErrorMessage = "Error occurred" };

        Assert.That(entry.ErrorMessage, Is.EqualTo("Error occurred"));
    }

    [Test]
    public void AllProperties_CanBeSetTogether()
    {
        var now = DateTime.Now;
        var entry = new LogEntry
        {
            Timestamp = now,
            BackupName = "TestBackup",
            SourcePath = "C:\\Source\\File.txt",
            TargetPath = "C:\\Target\\File.txt",
            FileSizeBytes = 2048,
            TransferTimeMs = 300,
            CryptingTimeMs = 100,
            ErrorMessage = "Test error"
        };

        Assert.Multiple(() =>
        {
            Assert.That(entry.Timestamp, Is.EqualTo(now));
            Assert.That(entry.BackupName, Is.EqualTo("TestBackup"));
            Assert.That(entry.SourcePath, Is.EqualTo("C:\\Source\\File.txt"));
            Assert.That(entry.TargetPath, Is.EqualTo("C:\\Target\\File.txt"));
            Assert.That(entry.FileSizeBytes, Is.EqualTo(2048));
            Assert.That(entry.TransferTimeMs, Is.EqualTo(300));
            Assert.That(entry.CryptingTimeMs, Is.EqualTo(100));
            Assert.That(entry.ErrorMessage, Is.EqualTo("Test error"));
        });
    }
}

