using EasyLog;
using EasySave.Core.Contracts;

namespace EasySave.Data.Logging;

/// <summary>
///     JSON log writer adapter.
/// </summary>
public sealed class JsonLogWriter<T> : ILogWriter<T>
{
    private readonly AbstractLogger<T> _logger;

    public JsonLogWriter(string logDirectory)
    {
        _logger = new JsonLogger<T>(logDirectory);
    }

    public JsonLogWriter(AbstractLogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Log(T entry)
    {
        _logger.Log(entry);
    }
}
