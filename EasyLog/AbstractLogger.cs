namespace EasyLog;

public abstract class AbstractLogger<T>(string logDirectory, string extension)
{
    private string _logFilePath = "";
    
    protected List<T> ReadLogFile()
    {
        DateTime now = DateTime.Now;
        // Make us able to use the same name when writting the file.
        _logFilePath = Path.Join(logDirectory, now.ToString("yyyy-MM-dd") + "." + extension);
        // If log file doesn't exist return Empty Array, else return Deserialized file content.
        return !File.Exists(_logFilePath) ? [] : Deserialize(File.ReadAllText(_logFilePath));
    }

    protected void WriteLogFile(List<T> logs)
    {
        // Write in the log file.
        File.WriteAllText(_logFilePath, Serialize(logs));
    }

    protected abstract List<T> Deserialize(string fileContent);
    protected abstract string Serialize(List<T> logs);
    public abstract void Log(T content);
}