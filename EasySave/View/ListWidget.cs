using EasySave.View.Console;

namespace EasySave.View;

public class ListWidget
{
    private string _menuTitle = "";
    private bool _close = false;

    public void ShowList(List<Option> options, string menuTitle, IConsole console)
    {
        _menuTitle = menuTitle;
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (console == null) throw new ArgumentNullException(nameof(console));
        if (options.Count == 0) throw new ArgumentException("Options list cannot be empty.", nameof(options));

        int index = 0;
        _close = false;
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
        } while (keyinfo.Key != ConsoleKey.X && _close == false);
    }

    public void Return()
    {
        _close = true;
    }

    /// <summary>
    /// Write the menu to the console.
    /// </summary>
    private void WriteMenu(IConsole console, List<Option> options, Option selectedOption)
    {
        console.Clear();
        console.WriteLine(_menuTitle);
        foreach (Option option in options)
        {
            if (option == selectedOption)
            {
                console.Selected();
            }
            console.Write(option == selectedOption ? "> " : "  ");
            console.WriteLine(option.Description);
            console.ResetColor();
        }
    }
}