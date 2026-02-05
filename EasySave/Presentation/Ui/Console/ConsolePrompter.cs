using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Presentation.Resources;

namespace EasySave.Presentation.Ui.Console;

/// <summary>
/// Handles basic user input loops (non-empty strings, directory validation, choice selection).
/// </summary>
internal sealed class ConsolePrompter
{
    private readonly IConsole _console;
    private readonly IPathService _paths;

    /// <summary>
    /// Builds the prompter with its dependencies.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="paths">Path service.</param>
    public ConsolePrompter(IConsole console, IPathService paths)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    /// <summary>
    /// Reads a non-empty value.
    /// </summary>
    /// <param name="prompt">Prompt message.</param>
    /// <returns>Non-empty value.</returns>
    public string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            _console.WriteLine(prompt);
            string? raw = _console.ReadLine();
            raw = (raw ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(raw))
                return raw;

            _console.WriteLine(Resources.UserInterface.Common_InvalidInput);
        }
    }

    /// <summary>
    /// Reads an existing directory path.
    /// </summary>
    /// <param name="prompt">Prompt message.</param>
    /// <param name="notFoundMessage">Error message if not found.</param>
    /// <returns>Normalized path.</returns>
    public string ReadExistingDirectory(string prompt, string notFoundMessage)
    {
        while (true)
        {
            string raw = ReadNonEmpty(prompt);
            if (_paths.TryNormalizeExistingDirectory(raw, out string normalized))
                return normalized;

            _console.WriteLine(notFoundMessage);
        }
    }

    /// <summary>
    /// Reads the backup type from a user choice.
    /// </summary>
    /// <param name="prompt">Prompt message.</param>
    /// <param name="options">Displayed options.</param>
    /// <param name="invalidInputMessage">Message for invalid input.</param>
    /// <returns>Selected backup type.</returns>
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

    /// <summary>
    /// Pauses the UI until a key is pressed.
    /// </summary>
    /// <param name="pressAnyKeyMessage">Displayed message.</param>
    public void Pause(string pressAnyKeyMessage)
    {
        _console.WriteLine(string.Empty);
        _console.WriteLine(pressAnyKeyMessage);
        _console.ReadKey(intercept: true);
    }
}
