namespace EasySave.View.Console;

/// <summary>
/// Default console adapter using <see cref="System.Console"/>.
/// </summary>
internal sealed class SystemConsole : IConsole
{
    public void Clear() => System.Console.Clear();

    public void Write(string value) => System.Console.Write(value);

    public void WriteLine(string value) => System.Console.WriteLine(value);

    public string? ReadLine() => System.Console.ReadLine();

    public ConsoleKeyInfo ReadKey(bool intercept) => System.Console.ReadKey(intercept);
}
