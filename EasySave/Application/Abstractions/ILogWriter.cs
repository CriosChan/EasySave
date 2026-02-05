namespace EasySave.Application.Abstractions;

/// <summary>
/// Minimal contract for log writing.
/// </summary>
public interface ILogWriter<in T>
{
    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <param name="entry">Entry to record.</param>
    void Log(T entry);
}
