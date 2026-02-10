using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.Lang;

namespace EasySave.Application.Services;

public class LanguageService
{
    /// <summary>
    ///     Sets the application's language for localization purposes.
    /// </summary>
    /// <param name="lang">A string representing the language code (e.g., "en-US", "fr-FR").</param>
    /// <remarks>
    ///     This method updates the application's language setting and applies the chosen localization.
    ///     It should be called before rendering any UI elements to ensure that menus and prompts
    ///     display the correct language resources.
    ///     The method utilizes the singleton instance of <see cref="ApplicationConfiguration" />
    ///     to update the localization setting. Make sure the language code provided is valid
    ///     for supported languages.
    /// </remarks>
    public void SetLanguage(string lang)
    {
        var cfg = ApplicationConfiguration.Instance;
        cfg.Localization = lang;

        // Apply localization early so menus/prompts pick the right resource.
        LangUtil.TryApplyCulture(cfg.Localization);
    }
}