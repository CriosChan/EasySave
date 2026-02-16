namespace EasySave.Models.Utils;

/// <summary>
///     Defines culture application behavior for UI localization.
/// </summary>
public interface ILocalizationApplier
{
    /// <summary>
    ///     Applies a culture code to the current process.
    /// </summary>
    /// <param name="cultureName">Culture code (for example: fr-FR).</param>
    void Apply(string cultureName);
}
