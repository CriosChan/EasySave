namespace EasySave.Bootstrap;

/// <summary>
/// Point d'entree principal du programme.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Lance l'application et renvoie le code de sortie.
    /// </summary>
    /// <param name="args">Arguments de ligne de commande.</param>
    private static void Main(string[] args)
    {
        IApplication app = new EasySaveApplication();
        Environment.ExitCode = app.Run(args);
    }
}
