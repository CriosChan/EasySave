namespace EasySave.ViewModels.Services;

/// <summary>
///     Tlumach-based implementation of <see cref="IUiTextService" />.
/// </summary>
public sealed class TlumachUiTextService : IUiTextService
{
    /// <summary>
    ///     Gets a localized string and falls back to a default value when missing.
    /// </summary>
    /// <param name="resourceKey">Resource key to resolve.</param>
    /// <param name="fallback">Fallback text.</param>
    /// <returns>Localized text or fallback value.</returns>
    public string Get(string resourceKey, string fallback)
    {
        var entry = Localizer.Manager.GetValue(resourceKey);
        return string.IsNullOrEmpty(entry.Text) ? fallback : entry.Text;
    }

    /// <summary>
    ///     Formats a localized string using provided arguments.
    /// </summary>
    /// <param name="resourceKey">Resource key to resolve.</param>
    /// <param name="fallback">Fallback text.</param>
    /// <param name="args">Formatting arguments.</param>
    /// <returns>Formatted localized text.</returns>
    public string Format(string resourceKey, string fallback, params object[] args)
    {
        return string.Format(Get(resourceKey, fallback), args);
    }
}
