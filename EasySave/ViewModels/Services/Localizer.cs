using System.Globalization;
using System.IO;
using Tlumach;
using Tlumach.Avalonia;
using Tlumach.Base;

namespace EasySave.ViewModels.Services;

/// <summary>
///     Provides a single, application-wide Tlumach <see cref="TranslationManager" /> and factory helpers
///     for creating <see cref="TranslationUnit" /> instances tied to that manager.
/// </summary>
public static class Localizer
{
    private static readonly TranslationManager _manager;

    static Localizer()
    {
        ResxParser.Use();

        var translationsDir = Path.Combine(AppContext.BaseDirectory, "Views", "Resources");
        var defaultFile = Path.Combine(translationsDir, "UserInterface.resx");

        var config = new TranslationConfiguration(
            null,
            defaultFile,
            "en-US",
            TextFormat.DotNet
        );

        _manager = new TranslationManager(config);
        _manager.LoadFromDisk = true;
        _manager.TranslationsDirectory = translationsDir;
    }

    /// <summary>
    ///     Gets the shared <see cref="TranslationManager" /> instance.
    /// </summary>
    public static TranslationManager Manager => _manager;

    /// <summary>
    ///     Creates a new <see cref="TranslationUnit" /> bound to the shared manager for the given key.
    /// </summary>
    /// <param name="key">The resource key to translate.</param>
    /// <param name="hasPlaceholders">Whether the entry contains .NET-style placeholders.</param>
    /// <returns>A reactive translation unit.</returns>
    public static Tlumach.Avalonia.TranslationUnit CreateUnit(string key, bool hasPlaceholders = false)
        => new Tlumach.Avalonia.TranslationUnit(_manager, _manager.DefaultConfiguration!, key, hasPlaceholders);

    /// <summary>
    ///     Changes the active locale for all translation units.
    /// </summary>
    /// <param name="cultureName">BCP-47 culture name, e.g. "fr-FR".</param>
    public static void SetCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return;

        try
        {
            _manager.CurrentCulture = new CultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            // If the culture name is invalid, keep the current culture.
        }
    }
}
