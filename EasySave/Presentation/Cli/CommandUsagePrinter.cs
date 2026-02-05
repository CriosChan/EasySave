namespace EasySave.Presentation.Cli;

/// <summary>
/// Centralizes user-facing usage messages for the command-line entrypoint.
/// </summary>
internal static class CommandUsagePrinter
{
    /// <summary>
    /// Affiche l'aide d'utilisation de la ligne de commande.
    /// </summary>
    internal static void Print()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  EasySave.exe 1-3");
        Console.WriteLine("  EasySave.exe 1;3");
        Console.WriteLine("  EasySave.exe 2");
    }
}
