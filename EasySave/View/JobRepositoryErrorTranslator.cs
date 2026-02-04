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
            "Error.MaxJobs" => Text.Get("Add.Error.MaxJobs"),
            "Error.NoFreeSlot" => Text.Get("Add.Error.NoFreeSlot"),
            _ => errorCode
        };
    }
}
