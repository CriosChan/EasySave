using EasySave.Models;
using EasySave.Services;
using EasySave.Utils;
using EasySave.Controller.CommandLine;

namespace EasySave.Controller;

/// <summary>
/// Handles execution of backup jobs via command line arguments.
/// </summary>
public static class CommandController
{
    /// <summary>
    /// Entry point for command-line execution.
    ///
    /// Supported formats:
    /// - "1-3"  → backups 1, 2, 3
    /// - "1;3"  → backups 1 and 3
    /// - "2"    → backup 2 only
    /// </summary>
    /// <returns>Process exit code.</returns>
    public static int Run(string[] args, JobRepository repository, BackupService backupService, StateFileService stateService)
    {
        string raw = string.Join(string.Empty, args).Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            PrintUsage();
            return 1;
        }

        List<int> ids;
        try
        {
            ids = ParseArguments(raw);
        }
        catch
        {
            PrintUsage();
            return 1;
        }

        var runner = new CommandJobRunner(repository, backupService, stateService);
        return runner.RunJobs(ids);
    }

    private static void PrintUsage()
    {
        CommandUsagePrinter.Print();
    }

    private static List<int> ParseArguments(string arg)
    {
        return CommandLineArgumentParser.Parse(arg);
    }
}
