namespace EasySave.Models.BusinessSoftware;

/// <summary>
///     Persists the configured business software process names.
/// </summary>
public interface IBusinessSoftwareSettingsService
{
    /// <summary>
    ///     Loads the configured process names used to block backups.
    /// </summary>
    /// <returns>Read-only list of configured process names.</returns>
    IReadOnlyList<string> LoadConfiguredProcessNames();

    /// <summary>
    ///     Saves the configured process names used to block backups.
    /// </summary>
    /// <param name="processNames">Process names to persist.</param>
    void SaveConfiguredProcessNames(IEnumerable<string> processNames);
}
