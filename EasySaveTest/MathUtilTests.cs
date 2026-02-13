using EasySave.Models.Utils;

namespace EasySaveTest;

public class MathUtilTests
{
    [Test]
    public void Percentage_WithValidValues_ReturnsCorrectPercentage()
    {
        var result = MathUtil.Percentage(50, 100);

        Assert.That(result, Is.EqualTo(50));
    }

    [Test]
    public void Percentage_WithZeroActual_ReturnsZero()
    {
        var result = MathUtil.Percentage(0, 100);

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Percentage_WithZeroTotal_ReturnsOneHundred()
    {
        var result = MathUtil.Percentage(50, 0);

        Assert.That(result, Is.EqualTo(100));
    }

    [Test]
    public void Percentage_WithNegativeTotal_ReturnsOneHundred()
    {
        var result = MathUtil.Percentage(50, -10);

        Assert.That(result, Is.EqualTo(100));
    }

    [Test]
    public void Percentage_WhenActualExceedsTotal_ReturnsOneHundred()
    {
        var result = MathUtil.Percentage(150, 100);

        Assert.That(result, Is.EqualTo(100));
    }

    [Test]
    public void Percentage_WithEqualValues_ReturnsOneHundred()
    {
        var result = MathUtil.Percentage(100, 100);

        Assert.That(result, Is.EqualTo(100));
    }

    [Test]
    public void Percentage_WithSmallFraction_ReturnsCorrectValue()
    {
        var result = MathUtil.Percentage(25, 200);

        Assert.That(result, Is.EqualTo(12.5));
    }

    [Test]
    public void Percentage_WithLargeNumbers_ReturnsCorrectValue()
    {
        var result = MathUtil.Percentage(1000000, 10000000);

        Assert.That(result, Is.EqualTo(10));
    }

    [Test]
    public void Percentage_WithDecimalValues_ReturnsCorrectValue()
    {
        var result = MathUtil.Percentage(33.33, 100);

        Assert.That(result, Is.EqualTo(33.33).Within(0.01));
    }

    [Test]
    public void Percentage_WithVerySmallActual_ReturnsCloseToZero()
    {
        var result = MathUtil.Percentage(0.01, 1000);

        Assert.That(result, Is.EqualTo(0.001).Within(0.0001));
    }
}

