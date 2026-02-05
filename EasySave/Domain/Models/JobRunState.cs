namespace EasySave.Domain.Models;

/// <summary>
/// Etats possibles d'un job en execution.
/// </summary>
public enum JobRunState
{
    /// <summary>
    /// Job inactif.
    /// </summary>
    Inactive,
    /// <summary>
    /// Job en cours.
    /// </summary>
    Active,
    /// <summary>
    /// Job termine avec succes.
    /// </summary>
    Completed,
    /// <summary>
    /// Job termine avec erreur.
    /// </summary>
    Failed
}
