using EasySave.View.Console;

namespace EasySave.View;

public static class ListWidget
{
    public static void ShowList(List<Option> options)
    {
        ShowList(options, new SystemConsole());
    }

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
    private static void WriteMenu(IConsole console, List<Option> options, Option selectedOption)
    {
        console.Clear();
        console.WriteLine(Ressources.UserInterface.Menu_Header);
        foreach (Option option in options)
        {
            console.Write(option == selectedOption ? "> " : "  ");
            console.WriteLine(option.Description);
        }
    }
}