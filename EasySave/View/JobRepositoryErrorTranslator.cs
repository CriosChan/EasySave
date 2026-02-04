namespace EasySave.View;

/// <summary>
/// Translates repository error codes to localized user messages.
/// </summary>
internal sealed class JobRepositoryErrorTranslator
{
    public string Translate(string errorCode)
    {
        return errorCode switch
        {
            "Error.MaxJobs" => Ressources.UserInterface.Add_Error_MaxJobs,
            "Error.NoFreeSlot" => Ressources.UserInterface.Add_Error_NoFreeSlot,
            _ => errorCode
        };
    }
}
