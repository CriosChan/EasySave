using EasySave.Presentation.Resources;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
/// Component for displaying keyboard-navigable menus.
/// </summary>
public static class ListWidget
{
    /// <summary>
    /// Displays a menu in a new system console.
    /// </summary>
    /// <param name="options">Options to display.</param>
    public static void ShowList(List<Option> options)
    {
        ShowList(options, new SystemConsole());
    }

    /// <summary>
    /// Displays a menu in a provided console.
    /// </summary>
    /// <param name="options">Options to display.</param>
    /// <param name="console">Target console.</param>
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
    /// Writes the menu and highlights the selected option.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="options">Options to display.</param>
    /// <param name="selectedOption">Active option.</param>
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
