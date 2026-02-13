namespace EasySave.Models.BusinessSoftware;

/// <summary>
///     Provides available business software candidates from the local machine.
/// </summary>
public interface IBusinessSoftwareCatalogService
{
    /// <summary>
    ///     Loads available software candidates.
    /// </summary>
    /// <returns>Read-only list of selectable software candidates.</returns>
    IReadOnlyList<BusinessSoftwareCatalogItem> GetAvailableSoftware();
}
