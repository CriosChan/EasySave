using EasySave.ViewModels.Services;

namespace EasySave.Cli;

/// <summary>
///     Centralizes user-facing usage messages for the command-line entrypoint.
/// </summary>
internal static class CommandUsagePrinter
{
    /// <summary>
    ///     Prints command-line usage help.
    /// </summary>
    /// <param name="uiTextService">Localized text service.</param>
    internal static void Print(IUiTextService uiTextService)
    {
        ArgumentNullException.ThrowIfNull(uiTextService);

        Console.WriteLine(uiTextService.Get("Terminal.Log.Usage", "Usage:"));
        Console.WriteLine("  EasySave.exe 1-3");
        Console.WriteLine("  EasySave.exe 1;3");
        Console.WriteLine("  EasySave.exe 2");
    }
}