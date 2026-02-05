namespace EasySave.Presentation.Ui.Console;

/// <summary>
/// Abstraction over the system console to keep UI components decoupled from static <see cref="System.Console"/>.
/// </summary>
public interface IConsole
{
    /// <summary>
    /// Efface l'ecran.
    /// </summary>
    void Clear();

    /// <summary>
    /// Ecrit du texte sans saut de ligne.
    /// </summary>
    /// <param name="value">Texte a ecrire.</param>
    void Write(string value);

    /// <summary>
    /// Ecrit du texte avec saut de ligne.
    /// </summary>
    /// <param name="value">Texte a ecrire.</param>
    void WriteLine(string value);

    /// <summary>
    /// Lit une ligne depuis l'entree standard.
    /// </summary>
    /// <returns>Ligne lue ou null.</returns>
    string? ReadLine();

    /// <summary>
    /// Lit une touche clavier.
    /// </summary>
    /// <param name="intercept">Indique si la touche est masquee.</param>
    /// <returns>Infos de touche.</returns>
    ConsoleKeyInfo ReadKey(bool intercept);
}
