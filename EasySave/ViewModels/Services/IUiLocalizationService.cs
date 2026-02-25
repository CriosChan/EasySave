namespace EasySave.ViewModels.Services;

/// <summary>
///     Defines application language selection behavior for UI translations.
/// </summary>
public interface IUiLocalizationService
{
    /// <summary>
    ///     Raised after the active culture changes.
    /// </summary>
    event EventHandler? CultureChanged;

    /// <summary>
    ///     Applies a culture code to UI translations.
    /// </summary>
    /// <param name="cultureName">Culture code (for example: fr-FR).</param>
    void Apply(string? cultureName);
}