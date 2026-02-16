namespace EasySave.ViewModels.Services;

/// <summary>
///     Defines localized UI text lookup helpers.
/// </summary>
public interface IUiTextService
{
    /// <summary>
    ///     Gets a localized string and falls back to a default value when missing.
    /// </summary>
    /// <param name="resourceKey">Resource key to resolve.</param>
    /// <param name="fallback">Fallback text.</param>
    /// <returns>Localized text or fallback value.</returns>
    string Get(string resourceKey, string fallback);

    /// <summary>
    ///     Formats a localized string using provided arguments.
    /// </summary>
    /// <param name="resourceKey">Resource key to resolve.</param>
    /// <param name="fallback">Fallback text.</param>
    /// <param name="args">Formatting arguments.</param>
    /// <returns>Formatted localized text.</returns>
    string Format(string resourceKey, string fallback, params object[] args);
}