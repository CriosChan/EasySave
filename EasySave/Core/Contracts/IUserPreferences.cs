namespace EasySave.Core.Contracts;

/// <summary>
///     Read/write access to user preferences that can change at runtime.
/// </summary>
public interface IUserPreferences
{
    /// <summary>
    ///     Current log type ("json" or "xml").
    /// </summary>
    string LogType { get; }

    /// <summary>
    ///     Current localization (e.g., "fr-FR").
    /// </summary>
    string Localization { get; }

    /// <summary>
    ///     Updates the log type preference.
    /// </summary>
    /// <param name="logType">Requested log type.</param>
    void SetLogType(string logType);

    /// <summary>
    ///     Updates the localization preference.
    /// </summary>
    /// <param name="localization">Requested localization.</param>
    void SetLocalization(string localization);
}
