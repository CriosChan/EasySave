using EasySave.Utils;
using System.Reflection;

namespace EasySaveTest;

[TestFixture]
public class DataPathResolverTest
{
    [Test]
    public void ResolveDirectory_ReturnsAbsolutePath_WhenGivenDriveRootedPath_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Test spécifique à Windows.");

        var abs = @"C:\MyFolder";
        var result = DataPathResolver.ResolveDirectory(abs, "default");
        Assert.That(result, Is.EqualTo(abs));
    }

    [Test]
    public void ResolveDirectory_ReturnsUNCPath_WhenGivenUNC_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Test spécifique à Windows.");

        var unc = @"\\server\share\folder";
        var result = DataPathResolver.ResolveDirectory(unc, "default");
        Assert.That(result, Is.EqualTo(unc));
    }

    [Test]
    public void ResolveDirectory_ReturnsBasePlusSubfolder_WhenGivenRelativePath()
    {
        var rel = "mydata";
        var expected = Path.Combine(GetExpectedBaseDir(), rel);
        var result = DataPathResolver.ResolveDirectory(rel, "default");
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ResolveDirectory_ReturnsBasePlusDefault_WhenGivenNullOrWhitespace()
    {
        var expected = Path.Combine(GetExpectedBaseDir(), "default");
        Assert.That(DataPathResolver.ResolveDirectory(null, "default"), Is.EqualTo(expected));
        Assert.That(DataPathResolver.ResolveDirectory("", "default"), Is.EqualTo(expected));
        Assert.That(DataPathResolver.ResolveDirectory("   ", "default"), Is.EqualTo(expected));
    }

    [Test]
    public void ResolveDirectory_StripsLeadingSeparators()
    {
        var rel = "/myfolder";
        var expected = Path.Combine(GetExpectedBaseDir(), "myfolder");
        var result = DataPathResolver.ResolveDirectory(rel, "default");
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ResolveDirectory_UsesDefaultSubfolder_WhenOnlySeparatorProvided()
    {
        var expected = Path.Combine(GetExpectedBaseDir(), "default");
        Assert.That(DataPathResolver.ResolveDirectory("/", "default"), Is.EqualTo(expected));
        Assert.That(DataPathResolver.ResolveDirectory("\\", "default"), Is.EqualTo(expected));
    }

    [Test]
    public void IsSafeAbsolute_ReturnsFalse_ForNonRootedPaths()
    {
        var method = typeof(DataPathResolver).GetMethod("IsSafeAbsolute", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That((bool)method.Invoke(null, new object[] { "folder" }), Is.False);
        Assert.That((bool)method.Invoke(null, new object[] { "C:folder" }), Is.False);
    }

    [Test]
    public void IsSafeAbsolute_ReturnsTrue_ForDriveRootedPath_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Test spécifique à Windows.");

        var method = typeof(DataPathResolver).GetMethod("IsSafeAbsolute", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That((bool)method.Invoke(null, new object[] { @"C:\folder" }), Is.True);
    }

    [Test]
    public void IsSafeAbsolute_ReturnsTrue_ForUNCPath_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Test spécifique à Windows.");

        var method = typeof(DataPathResolver).GetMethod("IsSafeAbsolute", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That((bool)method.Invoke(null, new object[] { @"\\server\share" }), Is.True);
    }

    [Test]
    public void IsSafeAbsolute_ReturnsFalse_ForRootedPath_OnLinux()
    {
        if (OperatingSystem.IsWindows())
            Assert.Ignore("Test spécifique à Linux/macOS.");

        var method = typeof(DataPathResolver).GetMethod("IsSafeAbsolute", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That((bool)method.Invoke(null, new object[] { "/etc" }), Is.False);
    }

    private string GetExpectedBaseDir()
    {
        // Reproduit la logique de GetBaseDataDirectory()
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(local))
            return Path.Combine(local, "EasySave");

        string user = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrWhiteSpace(user))
            return Path.Combine(user, "EasySave");

        string common = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (!string.IsNullOrWhiteSpace(common))
            return Path.Combine(common, "EasySave");

        return Path.Combine(AppContext.BaseDirectory, "EasySave");
    }
}