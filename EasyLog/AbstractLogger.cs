namespace EasyLog;

/// <summary>
///     Abstract base for typed log writers.
/// </summary>
public abstract class AbstractLogger<T>(string logDirectory, string extension)
{
    /// <summary>
    ///     Writes a log entry of type <typeparamref name="T" /> to a log file.
    /// </summary>
    /// <param name="log">The log entry to write to the file.</param>
    /// <remarks>
    ///     This method generates a log file name based on the current date,
    ///     ensuring that logs for each day are written to the same file.
    ///     The logs are appended to the file, and the serialized format is defined by the
    ///     <see cref="Serialize(T)" /> method.
    /// </remarks>
    protected void WriteLogFile(T log)
    {
        var now = DateTime.Now;
        // Ensure the directory exists before reading/writing.
        Directory.CreateDirectory(logDirectory);
        // Make us able to use the same name when writting the file.
        var logFilePath = Path.Join(logDirectory, now.ToString("yyyy-MM-dd") + "." + extension);
        // Write in the log file.
        File.AppendAllText(logFilePath, Serialize(log) + Environment.NewLine);
    }

    /// <summary>
    ///     Serializes logs of type <typeparamref name="T" /> into a string.
    /// </summary>
    /// <param name="logs">The logs to serialize.</param>
    /// <returns>A string representing the serialized logs.</returns>
    /// <remarks>
    ///     This method is abstract and must be implemented in a derived class.
    ///     The implementation should define the specific serialization logic.
    /// </remarks>
    protected abstract string Serialize(T logs);

    /// <summary>
    ///     Logs the specified content of type <typeparamref name="T" /> by writing it to a log file.
    /// </summary>
    /// <param name="content">The content to log.</param>
    /// <remarks>
    ///     This method overrides the base class implementation and calls the
    ///     <see cref="AbstractLogger{T}.WriteLogFile" /> method to handle the actual logging.
    ///     The content is passed to the <see cref="AbstractLogger{T}.WriteLogFile" /> method for processing.
    /// </remarks>
    public void Log(T content)
    {
        WriteLogFile(content);
    }
}