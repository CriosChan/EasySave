namespace EasyLog;

/// <summary>
/// Contract for log writers.
/// </summary>
public interface ILogWriter<T>
{
    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <param name="content">The log entry to write.</param>
    void Log(T content);
}
