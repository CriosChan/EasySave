using EasySave.Data.Configuration;

namespace EasySaveTest;

public class DataPathResolverTests
{
    [Test]
    public void ResolveDirectory_WithEmptyPath_UsesDefaultSubfolder()
    {
        var result = DataPathResolver.ResolveDirectory("", "config");

        Assert.That(result, Does.Contain("config"));
    }

    [Test]
    public void ResolveDirectory_WithNullPath_UsesDefaultSubfolder()
    {
        var result = DataPathResolver.ResolveDirectory(null!, "log");

        Assert.That(result, Does.Contain("log"));
    }

    [Test]
    public void ResolveDirectory_WithWhitespacePath_UsesDefaultSubfolder()
    {
        var result = DataPathResolver.ResolveDirectory("   ", "config");

        Assert.That(result, Does.Contain("config"));
    }

    [Test]
    public void ResolveDirectory_WithRelativePath_ReturnsAbsolutePath()
    {
        var result = DataPathResolver.ResolveDirectory("/config", "default");

        Assert.That(Path.IsPathRooted(result), Is.True);
    }

    [Test]
    public void ResolveDirectory_WithRelativePathStartingWithSlash_StripsSlash()
    {
        var result = DataPathResolver.ResolveDirectory("/myconfig", "default");

        Assert.That(result, Does.Contain("myconfig"));
    }

    [Test]
    public void ResolveDirectory_WithDifferentPaths_ReturnsDifferentResults()
    {
        var result1 = DataPathResolver.ResolveDirectory("/config", "default");
        var result2 = DataPathResolver.ResolveDirectory("/log", "default");

        Assert.That(result1, Is.Not.EqualTo(result2));
    }

    [Test]
    public void ResolveDirectory_ReturnsNonEmptyPath()
    {
        var result = DataPathResolver.ResolveDirectory("/test", "default");

        Assert.That(result, Is.Not.Empty);
    }
}

