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
    /// Construit le prompter avec ses dependances.
    /// </summary>
    /// <param name="console">Console cible.</param>
    /// <param name="paths">Service de chemins.</param>
    public ConsolePrompter(IConsole console, IPathService paths)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    /// <summary>
    /// Lit une valeur non vide.
    /// </summary>
    /// <param name="prompt">Message invite.</param>
    /// <returns>Valeur non vide.</returns>
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
    /// Lit un chemin de dossier existant.
    /// </summary>
    /// <param name="prompt">Message invite.</param>
    /// <param name="notFoundMessage">Message d'erreur si introuvable.</param>
    /// <returns>Chemin normalise.</returns>
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
    /// Lit le type de sauvegarde via un choix utilisateur.
    /// </summary>
    /// <param name="prompt">Message invite.</param>
    /// <param name="options">Options affichees.</param>
    /// <param name="invalidInputMessage">Message en cas d'erreur.</param>
    /// <returns>Type de sauvegarde selectionne.</returns>
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
    /// Met en pause l'interface jusqu'a une touche.
    /// </summary>
    /// <param name="pressAnyKeyMessage">Message affiche.</param>
    public void Pause(string pressAnyKeyMessage)
    {
        _console.WriteLine(string.Empty);
        _console.WriteLine(pressAnyKeyMessage);
        _console.ReadKey(intercept: true);
    }
}
