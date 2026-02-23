using System.Globalization;
using EasySave.Translations;

namespace EasySave.Models.Utils;

/// <summary>
///     Applies localization to the current process using CultureInfo.
/// </summary>
public static class LocalizationApplier
{
    public static void Apply(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return;

        try
        {
            var culture = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch
        {
            // If localization is invalid, keep the default system culture.
        }

        try { Strings.SetCulture(cultureName); }
        catch { /* Strings may not be available in non-Avalonia contexts (e.g., tests). */ }
    }
}