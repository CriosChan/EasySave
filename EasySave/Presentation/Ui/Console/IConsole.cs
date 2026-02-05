namespace EasySave.Presentation.Ui.Console;

/// <summary>
/// Abstraction over the system console to keep UI components decoupled from static <see cref="System.Console"/>.
/// </summary>
public interface IConsole
{
    /// <summary>
    /// Clears the screen.
    /// </summary>
    void Clear();

    /// <summary>
    /// Writes text without a line break.
    /// </summary>
    /// <param name="value">Text to write.</param>
    void Write(string value);

    /// <summary>
    /// Writes text with a line break.
    /// </summary>
    /// <param name="value">Text to write.</param>
    void WriteLine(string value);

    /// <summary>
    /// Reads a line from standard input.
    /// </summary>
    /// <returns>Read line or null.</returns>
    string? ReadLine();

    /// <summary>
    /// Reads a keyboard key.
    /// </summary>
    /// <param name="intercept">Whether the key is hidden.</param>
    /// <returns>Key info.</returns>
    ConsoleKeyInfo ReadKey(bool intercept);
}
