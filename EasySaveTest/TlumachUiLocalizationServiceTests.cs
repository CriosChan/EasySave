using System.Globalization;
using EasySave.Translation;
using EasySave.ViewModels.Services;

namespace EasySaveTest;

[NonParallelizable]
public class TlumachUiLocalizationServiceTests
{
    private CultureInfo? _originalCulture;

    [SetUp]
    public void Setup()
    {
        _originalCulture = Strings.TranslationManager.CurrentCulture;
    }

    [TearDown]
    public void TearDown()
    {
        if (_originalCulture != null)
            Strings.TranslationManager.CurrentCulture = _originalCulture;
    }

    [Test]
    public void Apply_WithFrenchCulture_SetsCurrentCulture()
    {
        var service = new TlumachUiLocalizationService();

        service.Apply("fr-FR");

        Assert.That(Strings.TranslationManager.CurrentCulture.Name, Is.EqualTo("fr-FR"));
    }

    [Test]
    public void Apply_WithEnglishCulture_SetsCurrentCulture()
    {
        var service = new TlumachUiLocalizationService();

        service.Apply("en-US");

        Assert.That(Strings.TranslationManager.CurrentCulture.Name, Is.EqualTo("en-US"));
    }

    [Test]
    public void Apply_WithInvalidCulture_DoesNotThrow()
    {
        var service = new TlumachUiLocalizationService();

        Assert.DoesNotThrow(() => service.Apply("invalid-culture"));
    }

    [Test]
    public void Apply_WithNullOrWhitespace_DoesNotThrow()
    {
        var service = new TlumachUiLocalizationService();

        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => service.Apply(null));
            Assert.DoesNotThrow(() => service.Apply(string.Empty));
            Assert.DoesNotThrow(() => service.Apply("   "));
        });
    }

    [Test]
    public void Apply_WithCultureChange_RaisesCultureChangedEvent()
    {
        var service = new TlumachUiLocalizationService();
        var raised = false;
        service.CultureChanged += (_, _) => raised = true;

        var targetCulture = string.Equals(Strings.TranslationManager.CurrentCulture.Name, "fr-FR",
            StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "fr-FR";

        service.Apply(targetCulture);

        Assert.That(raised, Is.True);
    }
}
