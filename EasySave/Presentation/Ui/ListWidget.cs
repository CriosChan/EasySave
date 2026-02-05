using EasySave.Presentation.Resources;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
/// Composant d'affichage de menus navigables au clavier.
/// </summary>
public static class ListWidget
{
    /// <summary>
    /// Affiche un menu dans une nouvelle console systeme.
    /// </summary>
    /// <param name="options">Options a afficher.</param>
    public static void ShowList(List<Option> options)
    {
        ShowList(options, new SystemConsole());
    }

    /// <summary>
    /// Affiche un menu dans une console fournie.
    /// </summary>
    /// <param name="options">Options a afficher.</param>
    /// <param name="console">Console cible.</param>
    public static void ShowList(List<Option> options, IConsole console)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (console == null) throw new ArgumentNullException(nameof(console));
        if (options.Count == 0) throw new ArgumentException("Options list cannot be empty.", nameof(options));

        int index = 0;
        ConsoleKeyInfo keyinfo;
        do
        {
            WriteMenu(console, options, options[index]);
            keyinfo = console.ReadKey(intercept: true);
            switch (keyinfo.Key)
            {
                case ConsoleKey.DownArrow:
                    index = (index + 1) % options.Count;
                    break;
                case ConsoleKey.UpArrow:
                    index = (index - 1 + options.Count) % options.Count;
                    break;
                case ConsoleKey.Enter:
                    options[index].Selected();
                    index = 0;
                    break;
            }
        } while (keyinfo.Key != ConsoleKey.X);
    }

    /// <summary>
    /// Write the menu to the console.
    /// </summary>
    /// <summary>
    /// Ecrit le menu et met en evidence l'option selectionnee.
    /// </summary>
    /// <param name="console">Console cible.</param>
    /// <param name="options">Options a afficher.</param>
    /// <param name="selectedOption">Option active.</param>
    private static void WriteMenu(IConsole console, List<Option> options, Option selectedOption)
    {
        console.Clear();
        console.WriteLine(Resources.UserInterface.Menu_Header);
        foreach (Option option in options)
        {
            console.Write(option == selectedOption ? "> " : "  ");
            console.WriteLine(option.Description);
        }
    }
}
