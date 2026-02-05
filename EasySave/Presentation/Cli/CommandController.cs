using EasySave.Application.Abstractions;

namespace EasySave.Presentation.Cli;

/// <summary>
/// Handles execution of backup jobs via command line arguments.
/// </summary>
public static class CommandController
{
    /// <summary>
    /// Entry point for command-line execution.
    ///
    /// Supported formats:
    /// - "1-3"  -> backups 1, 2, 3
    /// - "1;3"  -> backups 1 and 3
    /// - "2"    -> backup 2 only
    /// </summary>
    /// <returns>Process exit code.</returns>
    public static int Run(
        string[] args,
        IJobRepository repository,
        IBackupService backupService,
        IStateService stateService,
        IPathService paths)
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

        var runner = new CommandJobRunner(repository, backupService, stateService, paths);
        return runner.RunJobs(ids);
    }

    /// <summary>
    /// Affiche la syntaxe d'utilisation.
    /// </summary>
    private static void PrintUsage()
    {
        CommandUsagePrinter.Print();
    }

    /// <summary>
    /// Parse les arguments bruts en liste d'identifiants.
    /// </summary>
    /// <param name="arg">Argument brut.</param>
    /// <returns>Liste d'IDs.</returns>
    private static List<int> ParseArguments(string arg)
    {
        return CommandLineArgumentParser.Parse(arg);
    }
}


