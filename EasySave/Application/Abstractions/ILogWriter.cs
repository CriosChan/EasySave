namespace EasySave.Application.Abstractions;

/// <summary>
/// Contrat minimal pour l'ecriture de logs.
/// </summary>
public interface ILogWriter<in T>
{
    /// <summary>
    /// Ecrit une entree de log.
    /// </summary>
    /// <param name="entry">Entree a enregistrer.</param>
    void Log(T entry);
}
