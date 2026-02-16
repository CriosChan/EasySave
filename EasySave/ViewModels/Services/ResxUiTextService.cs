using EasySave.Views.Resources;

namespace EasySave.ViewModels.Services;

/// <summary>
///     Resource-based implementation of <see cref="IUiTextService" />.
/// </summary>
public sealed class ResxUiTextService : IUiTextService
{
    /// <summary>
    ///     Gets a localized string and falls back to a default value when missing.
    /// </summary>
    /// <param name="resourceKey">Resource key to resolve.</param>
    /// <param name="fallback">Fallback text.</param>
    /// <returns>Localized text or fallback value.</returns>
    public string Get(string resourceKey, string fallback)
    {
        return UserInterface.ResourceManager.GetString(resourceKey, UserInterface.Culture) ?? fallback;
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