namespace EasySave.Presentation.Ui.Console;

/// <summary>
///     Default console adapter using <see cref="System.Console" />.
/// </summary>
internal sealed class SystemConsole : IConsole
{
    /// <summary>
    ///     Clears the screen using the system console.
    /// </summary>
    public void Clear()
    {
        System.Console.Clear();
    }

    /// <summary>
    ///     Writes text without a line break.
    /// </summary>
    /// <param name="value">Text to write.</param>
    public void Write(string value)
    {
        System.Console.Write(value);
    }

    /// <summary>
    ///     Writes text with a line break.
    /// </summary>
    /// <param name="value">Text to write.</param>
    public void WriteLine(string value)
    {
        System.Console.WriteLine(value);
    }

    /// <summary>
    ///     Reads a line from standard input.
    /// </summary>
    /// <returns>Read line or null.</returns>
    public string? ReadLine()
    {
        return System.Console.ReadLine();
    }

    /// <summary>
    ///     Reads a keyboard key.
    /// </summary>
    /// <param name="intercept">Whether the key is hidden.</param>
    /// <returns>Key info.</returns>
    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        return System.Console.ReadKey(intercept);
    }

    public void Selected()
    {
        System.Console.BackgroundColor = ConsoleColor.White;
        System.Console.ForegroundColor = ConsoleColor.Black;
    }

    public void ResetColor()
    {
        System.Console.ResetColor();
    }
}