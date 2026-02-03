namespace EasySave.View;

public static class UserInterface
{
    /// <summary>
    /// Show the main menu to the user.
    /// </summary>
    public static void ShowMenu()
    {
        List<Option> options =
        [
            new Option(Ressources.UserInterface.Menu_ListJobs, ListJobs),
            new Option(Ressources.UserInterface.Menu_AddBackup, () => throw new NotImplementedException()),
            new Option(Ressources.UserInterface.Menu_RemoveBackup, () => throw new NotImplementedException()),
            new Option(Ressources.UserInterface.Menu_LaunchBackupJob, () => throw new NotImplementedException())
        ];
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
    /// <summary>
    /// List all backup jobs.
    /// </summary>
    private static void ListJobs()
    {
        // TODO: Implement job listing functionality
        throw new NotImplementedException();
    }
}