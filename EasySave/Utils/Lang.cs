using System.Globalization;

namespace EasySave.Utils;

public static class Lang
{
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