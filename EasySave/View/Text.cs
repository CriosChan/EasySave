using System.Globalization;

namespace EasySave.View;

/// <summary>
/// Convenience access to localized UI strings.
/// </summary>
internal static class Text
{
    public static string Get(string key)
    {
        // Use the current UI culture (set at startup).
        return Ressources.UserInterface.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }
}
