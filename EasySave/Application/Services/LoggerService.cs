using EasySave.Infrastructure.Configuration;

namespace EasySave.Application.Services;

public class LoggerService
{
    /// <summary>
    ///     Sets the application's log file type.
    /// </summary>
    /// <param name="logType">A string representing the log type (e.g., "json", "xml").</param>
    /// <remarks>
    ///     Applies type to ApplicationConfiguration.
    /// </remarks>
    public void SetLogger(string logType)
    {
        var cfg = ApplicationConfiguration.Instance;
        cfg.LogType = logType;
    }
}