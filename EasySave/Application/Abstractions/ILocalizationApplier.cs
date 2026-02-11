namespace EasySave.Application.Abstractions;

/// <summary>
///     Applies localization to the current process/UI.
/// </summary>
public interface ILocalizationApplier
{
    /// <summary>
    ///     Applies the specified culture if valid.
    /// </summary>
    /// <param name="cultureName">Culture name (e.g., "fr-FR").</param>
    void Apply(string cultureName);
}
