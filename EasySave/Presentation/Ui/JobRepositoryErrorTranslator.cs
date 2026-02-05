using EasySave.Presentation.Resources;

namespace EasySave.Presentation.Ui;

/// <summary>
/// Translates repository error codes to localized user messages.
/// </summary>
internal sealed class JobRepositoryErrorTranslator
{
    /// <summary>
    /// Traduit un code d'erreur en message localise.
    /// </summary>
    /// <param name="errorCode">Code d'erreur.</param>
    /// <returns>Message utilisateur.</returns>
    public string Translate(string errorCode)
    {
        return errorCode switch
        {
            "Error.MaxJobs" => Resources.UserInterface.Add_Error_MaxJobs,
            "Error.NoFreeSlot" => Resources.UserInterface.Add_Error_NoFreeSlot,
            _ => errorCode
        };
    }
}
