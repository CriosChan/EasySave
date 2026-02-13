using System.Globalization;
using EasySave.Models.Utils;

namespace EasySaveTest;

public class LocalizationApplierTests
{
    private CultureInfo? _originalCulture;
    private CultureInfo? _originalUICulture;

    [SetUp]
    public void Setup()
    {
        _originalCulture = CultureInfo.DefaultThreadCurrentCulture;
        _originalUICulture = CultureInfo.DefaultThreadCurrentUICulture;
    }

    [TearDown]
    public void TearDown()
    {
        CultureInfo.DefaultThreadCurrentCulture = _originalCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _originalUICulture;
    }

    [Test]
    public void Apply_WithValidCulture_SetsCulture()
    {
        var applier = new LocalizationApplier();

        applier.Apply("fr-FR");

        Assert.Multiple(() =>
        {
            Assert.That(CultureInfo.DefaultThreadCurrentCulture?.Name, Is.EqualTo("fr-FR"));
            Assert.That(CultureInfo.DefaultThreadCurrentUICulture?.Name, Is.EqualTo("fr-FR"));
        });
    }

    [Test]
    public void Apply_WithEnglishCulture_SetsCulture()
    {
        var applier = new LocalizationApplier();

        applier.Apply("en-US");

        Assert.Multiple(() =>
        {
            Assert.That(CultureInfo.DefaultThreadCurrentCulture?.Name, Is.EqualTo("en-US"));
            Assert.That(CultureInfo.DefaultThreadCurrentUICulture?.Name, Is.EqualTo("en-US"));
        });
    }

    [Test]
    public void Apply_WithEmptyString_DoesNotThrow()
    {
        var applier = new LocalizationApplier();

        Assert.DoesNotThrow(() => applier.Apply(""));
    }

    [Test]
    public void Apply_WithWhitespace_DoesNotThrow()
    {
        var applier = new LocalizationApplier();

        Assert.DoesNotThrow(() => applier.Apply("   "));
    }

    [Test]
    public void Apply_WithNull_DoesNotThrow()
    {
        var applier = new LocalizationApplier();

        Assert.DoesNotThrow(() => applier.Apply(null!));
    }

    [Test]
    public void Apply_WithInvalidCulture_DoesNotThrow()
    {
        var applier = new LocalizationApplier();

        Assert.DoesNotThrow(() => applier.Apply("invalid-culture"));
    }

    [Test]
    public void Apply_WithInvalidCulture_KeepsDefaultCulture()
    {
        var applier = new LocalizationApplier();
        // Set a known culture first
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        var beforeCulture = CultureInfo.DefaultThreadCurrentCulture;

        applier.Apply("xyz-ABC");

        // Should remain en-US since xyz-ABC is invalid
        Assert.That(CultureInfo.DefaultThreadCurrentCulture?.Name, Is.EqualTo("en-US"));
    }
}

