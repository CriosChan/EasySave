using EasySave.Models.Utils;

namespace EasySaveTest;

public class PathServiceTests
{
    [Test]
    public void TryNormalizeExistingDirectory_WithExistingDirectory_ReturnsTrue()
    {
        var tempDir = Path.GetTempPath();

        var result = PathService.TryNormalizeExistingDirectory(tempDir, out var normalized);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(normalized, Is.Not.Empty);
        });
    }

    [Test]
    public void TryNormalizeExistingDirectory_WithNonExistingDirectory_ReturnsFalse()
    {
        var nonExistingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var result = PathService.TryNormalizeExistingDirectory(nonExistingPath, out var normalized);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryNormalizeExistingDirectory_WithEmptyString_ReturnsFalse()
    {
        var result = PathService.TryNormalizeExistingDirectory("", out var normalized);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryNormalizeExistingDirectory_WithWhitespace_ReturnsFalse()
    {
        var result = PathService.TryNormalizeExistingDirectory("   ", out var normalized);

        Assert.That(result, Is.False);
    }

    [Test]
    public void ToFullUncLikePath_WithValidPath_ReturnsAbsolutePath()
    {
        var relativePath = ".";

        var result = PathService.ToFullUncLikePath(relativePath);

        Assert.That(Path.IsPathRooted(result), Is.True);
    }

    [Test]
    public void ToFullUncLikePath_WithEmptyString_ReturnsEmptyString()
    {
        var result = PathService.ToFullUncLikePath("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToFullUncLikePath_WithWhitespace_ReturnsWhitespace()
    {
        var result = PathService.ToFullUncLikePath("   ");

        Assert.That(result, Is.EqualTo("   "));
    }

    [Test]
    public void ToFullUncLikePath_WithAbsolutePath_ReturnsPath()
    {
        var absolutePath = Path.GetTempPath();

        var result = PathService.ToFullUncLikePath(absolutePath);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetRelativePath_WithValidPaths_ReturnsRelativePath()
    {
        var basePath = "C:\\Base";
        var fullPath = "C:\\Base\\Sub\\File.txt";

        var result = PathService.GetRelativePath(basePath, fullPath);

        Assert.That(result, Does.Contain("Sub"));
    }

    [Test]
    public void GetRelativePath_WithSamePath_ReturnsDot()
    {
        var basePath = "C:\\Base";
        var fullPath = "C:\\Base";

        var result = PathService.GetRelativePath(basePath, fullPath);

        Assert.That(result, Is.EqualTo("."));
    }

    [Test]
    public void GetRelativePath_WithDifferentRoots_ReturnsSomePath()
    {
        var basePath = "C:\\Base";
        var fullPath = "D:\\Other\\File.txt";

        var result = PathService.GetRelativePath(basePath, fullPath);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void ToFullUncLikePath_WithQuotedPath_RemovesQuotes()
    {
        var quotedPath = "\"" + Path.GetTempPath() + "\"";

        var result = PathService.ToFullUncLikePath(quotedPath);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Not.Contain("\""));
            Assert.That(Path.IsPathRooted(result), Is.True);
        });
    }

    [Test]
    public void ToFullUncLikePath_WithSingleQuotedPath_RemovesQuotes()
    {
        var quotedPath = "'" + Path.GetTempPath() + "'";

        var result = PathService.ToFullUncLikePath(quotedPath);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Not.Contain("'"));
            Assert.That(Path.IsPathRooted(result), Is.True);
        });
    }
}

