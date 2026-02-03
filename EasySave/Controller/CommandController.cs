using System;

public class CommandController
{
    /// <summary>
    /// Entry point of the application.
    /// This method is called automatically when the executable is launched.
    /// </summary>
    /// <param name="args">
    /// Command-line arguments passed to the executable.
    /// Example:
    ///   EasySave.exe 1-3
    ///   EasySave.exe 1;3
    /// </param>
    private static void Run(string[] args)
    {

        // If no argument is provided, the program cannot know which backups to run
        if (args.Length == 0)
        {
            // Display usage information
            return;
        }

        // Parse the first argument to determine which backups must be executed
        List<int> sauvegardes = ParseArguments(args[0]);

        foreach (int id in sauvegardes)
        {
            // Execute the backup with the specified ID
        }
    }

    /// <summary>
    /// Parses the command-line argument and converts it into a list of backup IDs.
    /// Supported formats:
    ///   - "1-3"  → backups 1, 2, 3
    ///   - "1;3"  → backups 1 and 3
    ///   - "2"    → backup 2 only
    /// </summary>
    /// <param name="arg">Raw argument string passed from the command line</param>
    /// <returns>List of backup IDs to execute</returns>
    private static List<int> ParseArguments(string arg)
    {
        // List that will contain all backup IDs to execute
        var result = new List<int>();

        // Case 1: Range format (e.g. "1-3")
        if (arg.Contains("-"))
        {
            var parts = arg.Split('-');
            int start = int.Parse(parts[0]);
            int end = int.Parse(parts[1]);
            for (int i = start; i <= end; i++)
                result.Add(i);
        }

        // Case 2: Multiple explicit values (e.g. "1;3;5")
        else if (arg.Contains(";"))
        {
            result = arg
                .Split(';')
                .Select(int.Parse)
                .ToList();
        }

        // Case 3: Single backup ID (e.g. "2")
        else
        {
            result.Add(int.Parse(arg));
        }

        return result;
    }
}
