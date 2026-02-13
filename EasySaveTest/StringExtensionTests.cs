using EasySave.Models.Utils;

namespace EasySaveTest;

public class StringExtensionTests
{
    [Test]
    public void ValidateNonEmpty_WithValidString_ReturnsTrimedString()
    {
        var result = "TestValue".ValidateNonEmpty("param");

        Assert.That(result, Is.EqualTo("TestValue"));
    }

    [Test]
    public void ValidateNonEmpty_WithWhitespace_TrimsString()
    {
        var result = "  TestValue  ".ValidateNonEmpty("param");

        Assert.That(result, Is.EqualTo("TestValue"));
    }

    [Test]
    public void ValidateNonEmpty_WithEmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => "".ValidateNonEmpty("param"));
    }

    [Test]
    public void ValidateNonEmpty_WithNull_ThrowsArgumentException()
    {
        string? value = null;
        Assert.Throws<ArgumentException>(() => value!.ValidateNonEmpty("param"));
    }

    [Test]
    public void ValidateNonEmpty_WithWhitespaceOnly_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => "   ".ValidateNonEmpty("param"));
    }

    [Test]
    public void ValidateNonEmpty_WithTabsAndSpaces_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => "\t\t  \t".ValidateNonEmpty("param"));
    }

    [Test]
    public void ValidateNonEmpty_WithNewLines_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => "\n\r\n".ValidateNonEmpty("param"));
    }

    [Test]
    public void ValidateNonEmpty_PreservesInternalWhitespace()
    {
        var result = "  Test  Value  ".ValidateNonEmpty("param");

        Assert.That(result, Is.EqualTo("Test  Value"));
    }

    [Test]
    public void ValidateNonEmpty_WithSingleCharacter_ReturnsCharacter()
    {
        var result = "A".ValidateNonEmpty("param");

        Assert.That(result, Is.EqualTo("A"));
    }

    [Test]
    public void ValidateNonEmpty_WithSpecialCharacters_ReturnsValue()
    {
        var result = "!@#$%^&*()".ValidateNonEmpty("param");

        Assert.That(result, Is.EqualTo("!@#$%^&*()"));
    }
}

