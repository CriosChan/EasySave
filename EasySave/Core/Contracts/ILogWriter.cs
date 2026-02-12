namespace EasySave.Core.Contracts;

/// <summary>
///     Contract for writing log entries.
/// </summary>
/// <typeparam name="T">Log entry type.</typeparam>
public interface ILogWriter<in T>
{
    /// <summary>
    ///     Writes a log entry.
    /// </summary>
    /// <param name="entry">Entry to log.</param>
    void Log(T entry);
}
