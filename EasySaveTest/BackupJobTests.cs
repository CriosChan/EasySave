﻿using EasySave.Core.Models;
using EasySave.Models.Backup;

namespace EasySaveTest;

public class BackupJobTests
{
    [Test]
    public void ConstructorWithValidParameters_CreatesInstance()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);

        Assert.Multiple(() =>
        {
            Assert.That(job.Id, Is.EqualTo(1));
            Assert.That(job.Name, Is.EqualTo("TestJob"));
            Assert.That(job.SourceDirectory, Is.EqualTo("C:\\Source"));
            Assert.That(job.TargetDirectory, Is.EqualTo("C:\\Target"));
            Assert.That(job.Type, Is.EqualTo(BackupType.Complete));
        });
    }

    [Test]
    public void ConstructorWithoutId_SetsIdToZero()
    {
        var job = new BackupJob("TestJob", "C:\\Source", "C:\\Target", BackupType.Differential);

        Assert.That(job.Id, Is.EqualTo(0));
    }

    [Test]
    public void ConstructorWithNegativeId_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BackupJob(-1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete));
    }

    [Test]
    public void ConstructorWithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new BackupJob(1, "", "C:\\Source", "C:\\Target", BackupType.Complete));
    }

    [Test]
    public void ConstructorWithWhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new BackupJob(1, "   ", "C:\\Source", "C:\\Target", BackupType.Complete));
    }

    [Test]
    public void ConstructorWithNullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new BackupJob(1, null!, "C:\\Source", "C:\\Target", BackupType.Complete));
    }

    [Test]
    public void ConstructorWithEmptySourceDirectory_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new BackupJob(1, "TestJob", "", "C:\\Target", BackupType.Complete));
    }

    [Test]
    public void ConstructorWithEmptyTargetDirectory_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new BackupJob(1, "TestJob", "C:\\Source", "", BackupType.Complete));
    }

    [Test]
    public void ConstructorWithInvalidBackupType_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", (BackupType)999));
    }

    [Test]
    public void ConstructorTrimsName_WhenNameHasWhitespace()
    {
        var job = new BackupJob(1, "  TestJob  ", "C:\\Source", "C:\\Target", BackupType.Complete);

        Assert.That(job.Name, Is.EqualTo("TestJob"));
    }

    [Test]
    public void ConstructorTrimsSourceDirectory_WhenHasWhitespace()
    {
        var job = new BackupJob(1, "TestJob", "  C:\\Source  ", "C:\\Target", BackupType.Complete);

        Assert.That(job.SourceDirectory, Is.EqualTo("C:\\Source"));
    }

    [Test]
    public void ConstructorTrimsTargetDirectory_WhenHasWhitespace()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "  C:\\Target  ", BackupType.Complete);

        Assert.That(job.TargetDirectory, Is.EqualTo("C:\\Target"));
    }

    [Test]
    public void Files_InitiallyEmpty()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);

        Assert.That(job.Files, Is.Empty);
    }

    [Test]
    public void CurrentFileIndex_InitiallyZero()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);

        Assert.That(job.CurrentFileIndex, Is.EqualTo(0));
    }

    [Test]
    public void CurrentProgress_InitiallyZero()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);

        Assert.That(job.CurrentProgress, Is.EqualTo(0));
    }

    [Test]
    public void IdCanBeModified_AfterConstruction()
    {
        var job = new BackupJob(1, "TestJob", "C:\\Source", "C:\\Target", BackupType.Complete);
        job.Id = 5;

        Assert.That(job.Id, Is.EqualTo(5));
    }

    [Test]
    public void StartBackup_WithDisconnectedDrive_FailsGracefully()
    {
        // Simule un disque déconnecté (Z: est généralement non utilisé)
        var job = new BackupJob(1, "TestJob", "Z:\\NonExistent\\Source", "Z:\\NonExistent\\Target", BackupType.Complete);

        // Capture la sortie console pour vérifier que l'erreur est loggée
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // StartBackup ne devrait pas lancer d'exception, mais devrait échouer proprement
        Assert.DoesNotThrow(() => job.StartBackup());

        var output = consoleOutput.ToString();
        
        // Vérifie que l'erreur est loggée dans la console
        Assert.That(output, Does.Contain("[ERROR]"));
    }

    [Test]
    public void StartBackup_WithNonExistentSourceDirectory_FailsGracefully()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var job = new BackupJob(1, "TestJob", Path.Combine(tempDir, "Source"), Path.Combine(tempDir, "Target"), BackupType.Complete);

        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        Assert.DoesNotThrow(() => job.StartBackup());

        var output = consoleOutput.ToString();
        Assert.That(output, Does.Contain("[ERROR]"));
    }
}

