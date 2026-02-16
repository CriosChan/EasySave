namespace EasySave.Models.BusinessSoftware;

/// <summary>
///     Represents a selectable business software candidate.
/// </summary>
public sealed class BusinessSoftwareCatalogItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BusinessSoftwareCatalogItem" /> class.
    /// </summary>
    /// <param name="displayName">Display name shown in the UI.</param>
    /// <param name="processName">Process name used for backup blocking detection.</param>
    public BusinessSoftwareCatalogItem(string displayName, string processName)
    {
        DisplayName = displayName;
        ProcessName = processName;
    }

    /// <summary>
    ///     Gets the display name shown in the UI.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    ///     Gets the process name used by runtime detection.
    /// </summary>
    public string ProcessName { get; }
}