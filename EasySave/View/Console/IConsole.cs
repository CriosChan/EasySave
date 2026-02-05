namespace EasySave.View.Console;

/// <summary>
/// Abstraction over the system console to keep UI components decoupled from static <see cref="System.Console"/>.
/// </summary>
public interface IConsole
{
    void Clear();
    void Write(string value);
    void WriteLine(string value);
    string? ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept);
}