using System.Globalization;
using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.Lang;

/// <summary>
///     Applies localization to the current process using CultureInfo.
/// </summary>
public sealed class LocalizationApplier : ILocalizationApplier
{
    public void Apply(string cultureName)
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
    }
}
