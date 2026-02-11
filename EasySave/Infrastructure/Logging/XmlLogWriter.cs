using EasyLog;
using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.Logging;

/// <summary>
///     XML log writer adapter.
/// </summary>
public sealed class XmlLogWriter<T> : ILogWriter<T>
{
    private readonly AbstractLogger<T> _logger;

    public XmlLogWriter(string logDirectory)
    {
        _logger = new XmlLogger<T>(logDirectory);
    }

    public XmlLogWriter(AbstractLogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Log(T entry)
    {
        _logger.Log(entry);
    }
}
