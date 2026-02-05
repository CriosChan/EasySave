namespace EasySave.Bootstrap;

/// <summary>
/// Small abstraction to keep Program focused on the entry point.
/// </summary>
internal interface IApplication
{
    /// <summary>
    /// Execute l'application avec les arguments fournis.
    /// </summary>
    /// <param name="args">Arguments de ligne de commande.</param>
    /// <returns>Code de sortie du processus.</returns>
    int Run(string[] args);
}
