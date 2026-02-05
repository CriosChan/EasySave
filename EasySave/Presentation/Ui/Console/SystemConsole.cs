namespace EasySave.Presentation.Ui.Console;

/// <summary>
/// Default console adapter using <see cref="System.Console"/>.
/// </summary>
internal sealed class SystemConsole : IConsole
{
    /// <summary>
    /// Efface l'ecran via la console systeme.
    /// </summary>
    public void Clear() => System.Console.Clear();

    /// <summary>
    /// Ecrit du texte sans saut de ligne.
    /// </summary>
    /// <param name="value">Texte a ecrire.</param>
    public void Write(string value) => System.Console.Write(value);

    /// <summary>
    /// Ecrit du texte avec saut de ligne.
    /// </summary>
    /// <param name="value">Texte a ecrire.</param>
    public void WriteLine(string value) => System.Console.WriteLine(value);

    /// <summary>
    /// Lit une ligne depuis l'entree standard.
    /// </summary>
    /// <returns>Ligne lue ou null.</returns>
    public string? ReadLine() => System.Console.ReadLine();

    /// <summary>
    /// Lit une touche clavier.
    /// </summary>
    /// <param name="intercept">Indique si la touche est masquee.</param>
    /// <returns>Infos de touche.</returns>
    public ConsoleKeyInfo ReadKey(bool intercept) => System.Console.ReadKey(intercept);
}
