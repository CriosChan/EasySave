namespace EasySave.Data.Configuration;

/// <summary>
///     Defines mutable application settings persistence operations.
/// </summary>
public interface IApplicationSettingsService
{
    /// <summary>
    ///     Persists the application localization code.
    /// </summary>
    /// <param name="localization">Localization value.</param>
    void SetLocalization(string localization);

    /// <summary>
    ///     Persists the log output type.
    /// </summary>
    /// <param name="logType">Log type value.</param>
    void SetLogType(string logType);
}
