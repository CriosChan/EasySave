using EasyLog;
using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.Logging;

/// <summary>
/// JSON log adapter based on EasyLog.
/// </summary>
public sealed class JsonLogWriter<T> : ILogWriter<T>
{
    private readonly AbstractLogger<T> _logger;

    /// <summary>
    /// Builds a JSON writer from a log directory.
    /// </summary>
    /// <param name="logDirectory">Destination log directory.</param>
    public JsonLogWriter(string logDirectory)
        : this(new JsonLogger<T>(logDirectory))
    {
    }

    /// <summary>
    /// Builds a JSON writer from an EasyLog logger.
    /// </summary>
    /// <param name="logger">Concrete logger.</param>
    public JsonLogWriter(AbstractLogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <param name="entry">Entry to record.</param>
    public void Log(T entry) => _logger.Log(entry);
}
