using System.Globalization;
using EasySave.Translation;

namespace EasySave.ViewModels.Services;

/// <summary>
///     Tlumach-based implementation of <see cref="IUiLocalizationService" />.
/// </summary>
public sealed class TlumachUiLocalizationService : IUiLocalizationService
{
    /// <summary>
    ///     Raised after the active culture changes.
    /// </summary>
    public event EventHandler? CultureChanged;

    /// <summary>
    ///     Applies a culture code to UI translations.
    /// </summary>
    /// <param name="cultureName">Culture code (for example: fr-FR).</param>
    public void Apply(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return;

        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            if (string.Equals(Strings.TranslationManager.CurrentCulture.Name, culture.Name,
                    StringComparison.OrdinalIgnoreCase))
                return;

            Strings.TranslationManager.CurrentCulture = culture;
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (CultureNotFoundException)
        {
            // Ignore invalid values and keep current culture.
        }
    }
}