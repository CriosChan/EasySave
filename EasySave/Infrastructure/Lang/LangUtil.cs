using System.Globalization;

namespace EasySave.Infrastructure.Lang;

public static class LangUtil
{
    /// <summary>
    /// Applies UI/thread culture if it is valid.
    /// </summary>
    /// <param name="cultureName">Culture name (e.g., fr-FR).</param>
    public static void TryApplyCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return;

        try
        {
            CultureInfo culture = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch
        {
            // If localization is invalid, keep the default system culture.
        }
    }
}