using EasySave.Models;
using EasySave.Utils;

namespace EasySave.View.Console;

/// <summary>
/// Handles basic user input loops (non-empty strings, directory validation, choice selection).
/// </summary>
internal sealed class ConsolePrompter
{
    private readonly IConsole _console;

    public ConsolePrompter(IConsole console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            _console.WriteLine(prompt);
            string? raw = _console.ReadLine();
            raw = (raw ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(raw))
                return raw;

            _console.WriteLine(Ressources.UserInterface.Common_InvalidInput);
        }
    }

    public string ReadExistingDirectory(string prompt, string notFoundMessage)
    {
        while (true)
        {
            string raw = ReadNonEmpty(prompt);
            if (PathTools.TryNormalizeExistingDirectory(raw, out string normalized))
                return normalized;

            _console.WriteLine(notFoundMessage);
        }
    }

    public BackupType ReadBackupType(string prompt, string options, string invalidInputMessage)
    {
        while (true)
        {
            _console.WriteLine(prompt);
            _console.WriteLine(options);
            string? raw = _console.ReadLine();
            raw = (raw ?? string.Empty).Trim();

            if (raw == "1")
                return BackupType.Complete;
            if (raw == "2")
                return BackupType.Differential;

            _console.WriteLine(invalidInputMessage);
        }
    }

    public void Pause(string pressAnyKeyMessage)
    {
        _console.WriteLine(string.Empty);
        _console.WriteLine(pressAnyKeyMessage);
        _console.ReadKey(intercept: true);
    }
}
