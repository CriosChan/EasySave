namespace EasySave.Models.Data.Configuration;

/// <summary>
///     Represents the different routing types for log storage.
/// </summary>
public enum RoutingType
{
    /// <summary>
    ///     Logs are stored locally on the client machine.
    /// </summary>
    Local,

    /// <summary>
    ///     Logs are sent to a central server for storage.
    /// </summary>
    Central,

    /// <summary>
    ///     Logs are stored both locally and on a central server.
    /// </summary>
    LocalCentral
}