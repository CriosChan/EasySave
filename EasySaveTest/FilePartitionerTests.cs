using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

/// <summary>
///     Unit tests for <see cref="FilePartitioner" />.
/// </summary>
public class FilePartitionerTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static FakeFile File(string sourcePath) => new(sourcePath);

    // ── NormaliseExtension ─────────────────────────────────────────────────────

    [Test]
    public void NormaliseExtension_WithLeadingDot_ReturnsLowercase()
    {
        var result = FilePartitioner.NormaliseExtension(".PDF");

        Assert.That(result, Is.EqualTo(".pdf"));
    }

    [Test]
    public void NormaliseExtension_WithoutLeadingDot_PrefixesDot()
    {
        var result = FilePartitioner.NormaliseExtension("pdf");

        Assert.That(result, Is.EqualTo(".pdf"));
    }

    [Test]
    public void NormaliseExtension_WithSurroundingSpaces_TrimsAndNormalises()
    {
        var result = FilePartitioner.NormaliseExtension("  .PDF  ");

        Assert.That(result, Is.EqualTo(".pdf"));
    }

    [Test]
    public void NormaliseExtension_WithSpacesAndNoDot_TrimsAndPrefixesDot()
    {
        var result = FilePartitioner.NormaliseExtension("  PDF  ");

        Assert.That(result, Is.EqualTo(".pdf"));
    }

    [Test]
    public void NormaliseExtension_WithEmptyString_ReturnsEmpty()
    {
        var result = FilePartitioner.NormaliseExtension(string.Empty);

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void NormaliseExtension_WithWhitespaceOnly_ReturnsEmpty()
    {
        var result = FilePartitioner.NormaliseExtension("   ");

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    // ── Partition – empty inputs ──────────────────────────────────────────────

    [Test]
    public void Partition_WithEmptyFileList_ReturnsTwoEmptyQueues()
    {
        var (pq, sq) = FilePartitioner.Partition([], [".pdf"]);

        Assert.Multiple(() =>
        {
            Assert.That(pq, Is.Empty);
            Assert.That(sq, Is.Empty);
        });
    }

    [Test]
    public void Partition_WithNoPriorityExtensions_AllFilesGoToStandard()
    {
        var files = new List<IFile>
        {
            File("C:\\src\\a.txt"),
            File("C:\\src\\b.pdf"),
            File("C:\\src\\c.docx")
        };

        var (pq, sq) = FilePartitioner.Partition(files, []);

        Assert.Multiple(() =>
        {
            Assert.That(pq, Is.Empty);
            Assert.That(sq, Has.Count.EqualTo(3));
        });
    }

    // ── Partition – correct routing ───────────────────────────────────────────

    [Test]
    public void Partition_WithMatchingExtension_RoutesPriorityFilesCorrectly()
    {
        var files = new List<IFile>
        {
            File("C:\\src\\report.pdf"),
            File("C:\\src\\notes.txt"),
            File("C:\\src\\data.csv")
        };

        var (pq, sq) = FilePartitioner.Partition(files, [".pdf"]);

        Assert.Multiple(() =>
        {
            Assert.That(pq, Has.Count.EqualTo(1));
            Assert.That(pq.Peek().SourceFile, Does.EndWith("report.pdf"));
            Assert.That(sq, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Partition_AllFilesPriority_StandardQueueIsEmpty()
    {
        var files = new List<IFile>
        {
            File("C:\\src\\a.pdf"),
            File("C:\\src\\b.pdf")
        };

        var (pq, sq) = FilePartitioner.Partition(files, [".pdf"]);

        Assert.Multiple(() =>
        {
            Assert.That(pq, Has.Count.EqualTo(2));
            Assert.That(sq, Is.Empty);
        });
    }

    [Test]
    public void Partition_NoFilesMatchPriority_PriorityQueueIsEmpty()
    {
        var files = new List<IFile>
        {
            File("C:\\src\\a.txt"),
            File("C:\\src\\b.docx")
        };

        var (pq, sq) = FilePartitioner.Partition(files, [".pdf"]);

        Assert.Multiple(() =>
        {
            Assert.That(pq, Is.Empty);
            Assert.That(sq, Has.Count.EqualTo(2));
        });
    }

    // ── Partition – extension normalisation during matching ───────────────────

    [Test]
    public void Partition_PriorityExtensionWithoutDot_StillMatchesFiles()
    {
        var files = new List<IFile>
        {
            File("C:\\src\\doc.pdf"),
            File("C:\\src\\img.png")
        };

        // Extension provided without leading dot
        var (pq, sq) = FilePartitioner.Partition(files, ["pdf"]);

        Assert.Multiple(() =>
        {
            Assert.That(pq, Has.Count.EqualTo(1));
            Assert.That(sq, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Partition_PriorityExtensionUppercase_MatchesCaseInsensitively()
    {
        var files = new List<IFile>
        {
            File("C:\\src\\doc.pdf"),
            File("C:\\src\\img.png")
        };

        var (pq, sq) = FilePartitioner.Partition(files, [".PDF"]);

        Assert.Multiple(() =>
        {
            Assert.That(pq, Has.Count.EqualTo(1));
            Assert.That(sq, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Partition_PriorityExtensionWithSpaces_MatchesAfterNormalisation()
    {
        var files = new List<IFile>
        {
            File("C:\\src\\doc.pdf"),
            File("C:\\src\\img.png")
        };

        var (pq, sq) = FilePartitioner.Partition(files, ["  .PDF  "]);

        Assert.Multiple(() =>
        {
            Assert.That(pq, Has.Count.EqualTo(1));
            Assert.That(sq, Has.Count.EqualTo(1));
        });
    }

    // ── Partition – ordering preserved ───────────────────────────────────────

    [Test]
    public void Partition_PreservesRelativeOrderInBothQueues()
    {
        var files = new List<IFile>
        {
            File("C:\\src\\a.pdf"),
            File("C:\\src\\b.txt"),
            File("C:\\src\\c.pdf"),
            File("C:\\src\\d.txt")
        };

        var (pq, sq) = FilePartitioner.Partition(files, [".pdf"]);

        // Priority: a.pdf, c.pdf (in that order)
        Assert.That(pq.Dequeue().SourceFile, Does.EndWith("a.pdf"));
        Assert.That(pq.Dequeue().SourceFile, Does.EndWith("c.pdf"));

        // Standard: b.txt, d.txt (in that order)
        Assert.That(sq.Dequeue().SourceFile, Does.EndWith("b.txt"));
        Assert.That(sq.Dequeue().SourceFile, Does.EndWith("d.txt"));
    }

    // ── Partition – null guards ───────────────────────────────────────────────

    [Test]
    public void Partition_NullFiles_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            FilePartitioner.Partition(null!, [".pdf"]));
    }

    [Test]
    public void Partition_NullExtensions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            FilePartitioner.Partition([], null!));
    }

    // ── Internal fake ─────────────────────────────────────────────────────────

    private sealed class FakeFile : IFile
    {
        public FakeFile(string sourcePath)
        {
            SourceFile = sourcePath;
            TargetFile = sourcePath.Replace("src", "dst");
        }

        public string SourceFile { get; }
        public string TargetFile { get; }

        public void Copy() { /* no-op */ }

        public long GetSize() => 0;
    }
}

