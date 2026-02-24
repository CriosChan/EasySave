using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

public class FilePartitionerTests
{
    // ---------------------------------------------------------------------------
    // NormalizeExtension
    // ---------------------------------------------------------------------------

    [Test]
    public void NormalizeExtension_PlainExtension_ReturnsLowercaseWithDot()
    {
        var result = FilePartitioner.NormalizeExtension("pdf");

        Assert.That(result, Is.EqualTo(".pdf"));
    }

    [Test]
    public void NormalizeExtension_ExtensionWithDot_ReturnsSameNormalised()
    {
        var result = FilePartitioner.NormalizeExtension(".PDF");

        Assert.That(result, Is.EqualTo(".pdf"));
    }

    [Test]
    public void NormalizeExtension_ExtensionWithLeadingTrailingSpaces_ReturnsTrimmedWithDot()
    {
        var result = FilePartitioner.NormalizeExtension("  .TXT  ");

        Assert.That(result, Is.EqualTo(".txt"));
    }

    [Test]
    public void NormalizeExtension_ExtensionWithSpacesNoDot_ReturnsTrimmedWithDot()
    {
        var result = FilePartitioner.NormalizeExtension("  DOCX  ");

        Assert.That(result, Is.EqualTo(".docx"));
    }

    [Test]
    public void NormalizeExtension_EmptyString_ReturnsEmpty()
    {
        var result = FilePartitioner.NormalizeExtension("");

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void NormalizeExtension_WhitespaceOnly_ReturnsEmpty()
    {
        var result = FilePartitioner.NormalizeExtension("   ");

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void NormalizeExtension_AlreadyNormalisedExtension_ReturnsUnchanged()
    {
        var result = FilePartitioner.NormalizeExtension(".mp3");

        Assert.That(result, Is.EqualTo(".mp3"));
    }

    [Test]
    public void NormalizeExtension_MixedCase_ReturnsLowercase()
    {
        var result = FilePartitioner.NormalizeExtension("MpEG");

        Assert.That(result, Is.EqualTo(".mpeg"));
    }

    // ---------------------------------------------------------------------------
    // Partition — empty inputs
    // ---------------------------------------------------------------------------

    [Test]
    public void Partition_EmptyFileList_ReturnsTwoEmptyQueues()
    {
        FilePartitioner.Partition([], ["pdf"], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority, Is.Empty);
            Assert.That(standard, Is.Empty);
        });
    }

    [Test]
    public void Partition_EmptyPriorityExtensions_AllFilesGoToStandardQueue()
    {
        var files = new List<IFile>
        {
            new FakeFile("C:\\src\\a.pdf", 100),
            new FakeFile("C:\\src\\b.txt", 200)
        };

        FilePartitioner.Partition(files, [], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority, Is.Empty);
            Assert.That(standard.Count, Is.EqualTo(2));
        });
    }

    // ---------------------------------------------------------------------------
    // Partition — correct routing
    // ---------------------------------------------------------------------------

    [Test]
    public void Partition_AllFilesMatchPriority_AllGoToPriorityQueue()
    {
        var files = new List<IFile>
        {
            new FakeFile("C:\\src\\a.pdf", 100),
            new FakeFile("C:\\src\\b.pdf", 200)
        };

        FilePartitioner.Partition(files, ["pdf"], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority.Count, Is.EqualTo(2));
            Assert.That(standard, Is.Empty);
        });
    }

    [Test]
    public void Partition_NoFileMatchesPriority_AllGoToStandardQueue()
    {
        var files = new List<IFile>
        {
            new FakeFile("C:\\src\\a.docx", 100),
            new FakeFile("C:\\src\\b.txt", 200)
        };

        FilePartitioner.Partition(files, ["pdf"], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority, Is.Empty);
            Assert.That(standard.Count, Is.EqualTo(2));
        });
    }

    [Test]
    public void Partition_MixedFiles_CorrectlySeparated()
    {
        var pdf1 = new FakeFile("C:\\src\\a.pdf", 100);
        var txt1 = new FakeFile("C:\\src\\b.txt", 200);
        var pdf2 = new FakeFile("C:\\src\\c.PDF", 300); // uppercase extension
        var doc1 = new FakeFile("C:\\src\\d.docx", 400);
        var files = new List<IFile> { pdf1, txt1, pdf2, doc1 };

        FilePartitioner.Partition(files, ["pdf"], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority.Count, Is.EqualTo(2));
            Assert.That(standard.Count, Is.EqualTo(2));
        });
    }

    // ---------------------------------------------------------------------------
    // Partition — order preservation
    // ---------------------------------------------------------------------------

    [Test]
    public void Partition_PreservesInsertionOrderInPriorityQueue()
    {
        var first  = new FakeFile("C:\\src\\first.pdf",  100);
        var second = new FakeFile("C:\\src\\second.pdf", 200);
        var third  = new FakeFile("C:\\src\\third.pdf",  300);
        var files  = new List<IFile> { first, second, third };

        FilePartitioner.Partition(files, ["pdf"], out var priority, out _);

        Assert.Multiple(() =>
        {
            Assert.That(priority.Dequeue(), Is.SameAs(first));
            Assert.That(priority.Dequeue(), Is.SameAs(second));
            Assert.That(priority.Dequeue(), Is.SameAs(third));
        });
    }

    [Test]
    public void Partition_PreservesInsertionOrderInStandardQueue()
    {
        var first  = new FakeFile("C:\\src\\first.txt",  100);
        var second = new FakeFile("C:\\src\\second.txt", 200);
        var files  = new List<IFile> { first, second };

        FilePartitioner.Partition(files, ["pdf"], out _, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(standard.Dequeue(), Is.SameAs(first));
            Assert.That(standard.Dequeue(), Is.SameAs(second));
        });
    }

    // ---------------------------------------------------------------------------
    // Partition — extension normalisation during matching
    // ---------------------------------------------------------------------------

    [Test]
    public void Partition_ExtensionInConfigWithDot_StillMatchesCorrectly()
    {
        var file  = new FakeFile("C:\\src\\report.pdf", 100);

        FilePartitioner.Partition([file], [".pdf"], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority.Count, Is.EqualTo(1));
            Assert.That(standard, Is.Empty);
        });
    }

    [Test]
    public void Partition_ExtensionInConfigUpperCase_StillMatchesLowerCaseFile()
    {
        var file = new FakeFile("C:\\src\\report.pdf", 100);

        FilePartitioner.Partition([file], ["PDF"], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority.Count, Is.EqualTo(1));
            Assert.That(standard, Is.Empty);
        });
    }

    [Test]
    public void Partition_FileExtensionUpperCase_MatchesLowerCaseConfigEntry()
    {
        var file = new FakeFile("C:\\src\\report.PDF", 100);

        FilePartitioner.Partition([file], ["pdf"], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority.Count, Is.EqualTo(1));
            Assert.That(standard, Is.Empty);
        });
    }

    [Test]
    public void Partition_ExtensionInConfigWithLeadingSpaces_StillMatches()
    {
        var file = new FakeFile("C:\\src\\archive.zip", 100);

        FilePartitioner.Partition([file], ["  zip  "], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority.Count, Is.EqualTo(1));
            Assert.That(standard, Is.Empty);
        });
    }

    [Test]
    public void Partition_EmptyStringInPriorityExtensions_IsIgnored()
    {
        var file = new FakeFile("C:\\src\\report.pdf", 100);

        // Empty entry must not match anything
        FilePartitioner.Partition([file], ["", "   "], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority, Is.Empty);
            Assert.That(standard.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void Partition_MultiplePriorityExtensions_AllMatched()
    {
        var pdf  = new FakeFile("C:\\src\\a.pdf",  100);
        var docx = new FakeFile("C:\\src\\b.docx", 200);
        var txt  = new FakeFile("C:\\src\\c.txt",  300);
        var files = new List<IFile> { pdf, docx, txt };

        FilePartitioner.Partition(files, ["pdf", "docx"], out var priority, out var standard);

        Assert.Multiple(() =>
        {
            Assert.That(priority.Count,  Is.EqualTo(2));
            Assert.That(standard.Count, Is.EqualTo(1));
        });
    }

    // ---------------------------------------------------------------------------
    // Fake helpers
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Minimal <see cref="IFile"/> stub used only in partitioner tests.
    /// </summary>
    private sealed class FakeFile : IFile
    {
        private readonly long _size;

        public FakeFile(string sourcePath, long size)
        {
            SourceFile = sourcePath;
            TargetFile = sourcePath.Replace("src", "dst");
            _size = size;
        }

        public string SourceFile { get; }
        public string TargetFile { get; }
        public string BackupName => "TestBackup";

        public void Copy() { }
        public long GetSize() => _size;
    }
}

