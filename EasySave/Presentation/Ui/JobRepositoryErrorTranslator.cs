namespace EasySave.Presentation.Ui;

/// <summary>
///     Translates repository error codes to localized user messages.
/// </summary>
internal sealed class JobRepositoryErrorTranslator
{
    /// <summary>
    ///     Translates an error code into a localized message.
    /// </summary>
    /// <param name="errorCode">Error code.</param>
    /// <returns>User-facing message.</returns>
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