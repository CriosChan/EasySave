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
        if (System.Console.IsOutputRedirected)
            return;

        try
        {
            System.Console.Clear();
        }
        catch (IOException)
        {
            // No interactive console available (e.g., redirected output).
        }
        catch (InvalidOperationException)
        {
            // Console not available.
        }
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
        if (System.Console.IsInputRedirected)
            throw new InvalidOperationException("Interactive console is not available. Run the app in a real terminal.");

        return System.Console.ReadKey(intercept);
    }

    public void Selected()
    {
        if (System.Console.IsOutputRedirected)
            return;

        try
        {
            System.Console.BackgroundColor = ConsoleColor.White;
            System.Console.ForegroundColor = ConsoleColor.Black;
        }
        catch (IOException)
        {
            // Ignore color changes when console is unavailable.
        }
        catch (InvalidOperationException)
        {
            // Ignore color changes when console is unavailable.
        }
    }

    public void ResetColor()
    {
        if (System.Console.IsOutputRedirected)
            return;

        try
        {
            System.Console.ResetColor();
        }
        catch (IOException)
        {
            // Ignore color changes when console is unavailable.
        }
        catch (InvalidOperationException)
        {
            // Ignore color changes when console is unavailable.
        }
    }
}
