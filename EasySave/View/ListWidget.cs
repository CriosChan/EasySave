namespace EasySave.View;

public static class ListWidget
{
    public static void ShowList(List<Option> options)
    {
        var index = 0;
        ConsoleKeyInfo keyinfo;
        do
        {
            WriteMenu(options, options[index]);
            keyinfo = Console.ReadKey();
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
    private static void WriteMenu(List<Option> options, Option selectedOption)
    {
        Console.Clear();
        Console.WriteLine(Ressources.UserInterface.Menu_Header);
        foreach (Option option in options)
        {
            if (option == selectedOption)
            {
                Console.Write(@"> ");
            }
            else
            {
                Console.Write(@"  ");
            }
            
            Console.WriteLine(option.Description);
        }
    }
    
}