using EasySave.Data.Configuration;
using EasySave.ViewModels.Services;

namespace EasySave.Cli;

/// <summary>
///     Handles execution of backup jobs via command line arguments.
/// </summary>
public static class CommandController
{
    /// <summary>
    ///     Entry point for command-line execution.
    ///     Supported formats:
    ///     - "1-3"  -> backups 1, 2, 3
    ///     - "1;3"  -> backups 1 and 3
    ///     - "2"    -> backup 2 only
    /// </summary>
    /// <returns>Process exit code.</returns>
    public static int Run(
        string[] args)
    {
        IUiTextService uiTextService = new TlumachUiTextService();
        IUiLocalizationService localizationService = new TlumachUiLocalizationService();
        localizationService.Apply(ApplicationConfiguration.Load().Localization);

        var raw = string.Join(string.Empty, args).Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            PrintUsage(uiTextService);
            return 1;
        }

        List<int> ids;
        try
        {
            ids = ParseArguments(raw);
        }
        catch
        {
            PrintUsage(uiTextService);
            return 1;
        }

        var runner = new CommandJobRunner(uiTextService);
        return runner.RunJobs(ids);
    }

    /// <summary>
    ///     Prints usage syntax.
    /// </summary>
    /// <param name="uiTextService">Localized text service.</param>
    private static void PrintUsage(IUiTextService uiTextService)
    {
        CommandUsagePrinter.Print(uiTextService);
    }

    /// <summary>
    ///     Parses raw arguments into a list of ids.
    /// </summary>
    /// <param name="arg">Raw argument string.</param>
    /// <returns>List of ids.</returns>
    private static List<int> ParseArguments(string arg)
    {
        return CommandLineArgumentParser.Parse(arg);
    }
}
