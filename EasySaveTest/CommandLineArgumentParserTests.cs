using EasySave.Cli;

namespace EasySaveTest;

public class CommandLineArgumentParserTests
{
    [Test]
    public void Parse_WithSingleNumber_ReturnsSingleId()
    {
        var result = CommandLineArgumentParser.Parse("5");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(5));
        });
    }

    [Test]
    public void Parse_WithRange_ReturnsAllIdsInRange()
    {
        var result = CommandLineArgumentParser.Parse("1-3");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Does.Contain(1));
            Assert.That(result, Does.Contain(2));
            Assert.That(result, Does.Contain(3));
        });
    }

    [Test]
    public void Parse_WithReversedRange_ReturnsAllIdsInCorrectOrder()
    {
        var result = CommandLineArgumentParser.Parse("3-1");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Does.Contain(1));
            Assert.That(result, Does.Contain(2));
            Assert.That(result, Does.Contain(3));
        });
    }

    [Test]
    public void Parse_WithSemicolonSeparated_ReturnsMultipleIds()
    {
        var result = CommandLineArgumentParser.Parse("1;3;5");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Does.Contain(1));
            Assert.That(result, Does.Contain(3));
            Assert.That(result, Does.Contain(5));
        });
    }

    [Test]
    public void Parse_WithSpaces_IgnoresSpaces()
    {
        var result = CommandLineArgumentParser.Parse("1 - 3");

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void Parse_WithDuplicates_RemovesDuplicates()
    {
        var result = CommandLineArgumentParser.Parse("1;1;2;2;3");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result.Distinct().Count(), Is.EqualTo(3));
        });
    }

    [Test]
    public void Parse_WithInvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => CommandLineArgumentParser.Parse("1-2-3"));
    }

    [Test]
    public void Parse_WithNonNumeric_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => CommandLineArgumentParser.Parse("abc"));
    }

    [Test]
    public void Parse_WithZeroInRange_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => CommandLineArgumentParser.Parse("0-3"));
    }

    [Test]
    public void Parse_WithNegativeNumber_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => CommandLineArgumentParser.Parse("-1"));
    }

    [Test]
    public void Parse_WithLargeRange_ReturnsAllIds()
    {
        var result = CommandLineArgumentParser.Parse("1-10");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(10));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[9], Is.EqualTo(10));
        });
    }

    [Test]
    public void Parse_WithSameStartAndEnd_ReturnsSingleId()
    {
        var result = CommandLineArgumentParser.Parse("5-5");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(5));
        });
    }

    [Test]
    public void Parse_WithEmptyString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => CommandLineArgumentParser.Parse(""));
    }
}

