namespace EasyLog;

public abstract class AbstractLogger<T>(string logDirectory, string extension)
{
    /// <summary>
    /// Gets the directory where log files are written.
    /// </summary>
    protected string LogDirectory { get; } = logDirectory;

    /// <summary>
    /// Gets the log file extension (without dot).
    /// </summary>
    protected string Extension { get; } = extension;

    /// <summary>
    /// Builds the daily log file path for a given date.
    /// </summary>
    protected string GetLogFilePath(DateTime date)
        => Path.Join(LogDirectory, date.ToString("yyyy-MM-dd") + "." + Extension);

 
    /// <summary>
    /// Writes a log entry of type <typeparamref name="T"/> to a log file.
    /// </summary>
    /// <param name="log">The log entry to write to the file.</param>
    /// <remarks>
    /// This method generates a log file name based on the current date,
    /// ensuring that logs for each day are written to the same file.
    /// The logs are appended to the file, and the serialized format is defined by the 
    /// <see cref="Serialize(T)"/> method.
    /// </remarks>
    protected void WriteLogFile(T log)
    {
        DateTime now = DateTime.Now;
        // Ensure the directory exists before reading/writing.
        Directory.CreateDirectory(LogDirectory);
        // Make us able to use the same name when writting the file.
        var logFilePath = GetLogFilePath(now);
        // Write in the log file.
        File.AppendAllText(logFilePath, Serialize(log) + Environment.NewLine);
    }

    /// <summary>
    /// Serializes logs of type <typeparamref name="T"/> into a string.
    /// </summary>
    /// <param name="logs">The logs to serialize.</param>
    /// <returns>A string representing the serialized logs.</returns>
    /// <remarks>
    /// This method is abstract and must be implemented in a derived class.
    /// The implementation should define the specific serialization logic.
    /// </remarks>
    protected abstract string Serialize(T logs);

    /// <summary>
    /// Logs the specified content of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="content">The content to log.</param>
    /// <remarks>
    /// This method is abstract and must be implemented in a derived class.
    /// The implementation should define how the content is processed and recorded.
    /// </remarks>
    public abstract void Log(T content);
}