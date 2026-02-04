namespace EasyLog;

public abstract class AbstractLogger<T>(string logDirectory, string extension)
{
 
    protected void WriteLogFile(T log)
    {
        DateTime now = DateTime.Now;
        // Make us able to use the same name when writting the file.
        var logFilePath = Path.Join(logDirectory, now.ToString("yyyy-MM-dd") + "." + extension);
        // Write in the log file.
        File.AppendAllText(logFilePath, Serialize(log) + Environment.NewLine);
    }

    protected abstract string Serialize(T logs);
    public abstract void Log(T content);
}