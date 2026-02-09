using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.Lang;

namespace EasySave.Application.Services;

public class LoggerService
{
    /// <summary>
    /// Sets the application's log file type.
    /// </summary>
    /// <param name="logType">A string representing the log type (e.g., "json", "xml").</param>
    /// <remarks>
    /// Applies type to ApplicationConfiguration.
    /// </remarks>
    public void SetLogger(string logType)
    {
        ApplicationConfiguration cfg = ApplicationConfiguration.Instance;
        cfg.LogType = logType;
    }
}