using EasySave.Models.Backup;

namespace EasySaveTest;

/// <summary>
///     Unit tests for business software runtime detection matching rules.
/// </summary>
public class BusinessSoftwareMonitorTests
{
    [Test]
    public void IsBusinessSoftwareRunning_WithExactConfiguredName_ReturnsTrue()
    {
        var monitor = new BusinessSoftwareMonitor(
            ["CalculatorApp"],
            processName => string.Equals(processName, "CalculatorApp", StringComparison.OrdinalIgnoreCase),
            () => Array.Empty<string>());

        var isRunning = monitor.IsBusinessSoftwareRunning();

        Assert.That(isRunning, Is.True);
    }

    [Test]
    public void IsBusinessSoftwareRunning_WithCalcConfiguredAndCalculatorAppRunning_ReturnsTrue()
    {
        var monitor = new BusinessSoftwareMonitor(
            ["calc"],
            _ => false,
            () => ["CalculatorApp"]);

        var isRunning = monitor.IsBusinessSoftwareRunning();

        Assert.That(isRunning, Is.True);
    }

    [Test]
    public void IsBusinessSoftwareRunning_WithCompositeConfiguredName_UsesTokenMatching()
    {
        var monitor = new BusinessSoftwareMonitor(
            ["WindowsCalculator"],
            _ => false,
            () => ["CalculatorApp"]);

        var isRunning = monitor.IsBusinessSoftwareRunning();

        Assert.That(isRunning, Is.True);
    }

    [Test]
    public void IsBusinessSoftwareRunning_WithNoConfiguredSoftware_ReturnsFalseWithoutProcessLookups()
    {
        var monitor = new BusinessSoftwareMonitor(
            [],
            _ => throw new InvalidOperationException("Exact lookup should not be called."),
            () => throw new InvalidOperationException("Running-process lookup should not be called."));

        var isRunning = monitor.IsBusinessSoftwareRunning();

        Assert.That(isRunning, Is.False);
    }

    [Test]
    public void IsBusinessSoftwareRunning_WithNoMatchingProcess_ReturnsFalse()
    {
        var monitor = new BusinessSoftwareMonitor(
            ["calc"],
            _ => false,
            () => ["Notepad", "Explorer"]);

        var isRunning = monitor.IsBusinessSoftwareRunning();

        Assert.That(isRunning, Is.False);
    }

    [Test]
    public void ConfiguredSoftwareNames_NormalizesAndDeduplicatesConfiguredValues()
    {
        var monitor = new BusinessSoftwareMonitor(
            [" C:\\Windows\\System32\\calc.exe ", "calculatorapp.exe", "CalculatorApp", "", "   "],
            _ => false,
            () => []);

        Assert.That(monitor.ConfiguredSoftwareNames,
            Is.EquivalentTo(new[] { "calc", "calculatorapp" }).IgnoreCase);
    }

    [Test]
    public void IsBusinessSoftwareRunning_UsesLatestConfiguredSoftwareWhenConfigurationChanges()
    {
        IReadOnlyList<string> configuredNames = ["notepad"];
        var monitor = new BusinessSoftwareMonitor(
            () => configuredNames,
            _ => false,
            () => ["CalculatorApp"]);

        Assert.That(monitor.IsBusinessSoftwareRunning(), Is.False);

        configuredNames = ["calc"];

        Assert.That(monitor.IsBusinessSoftwareRunning(), Is.True);
    }
}
