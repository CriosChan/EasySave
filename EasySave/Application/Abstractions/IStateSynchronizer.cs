namespace EasySave.Application.Abstractions;

/// <summary>
/// Contract for synchronizing job configuration with execution state.
/// </summary>
public interface IStateSynchronizer
{
    /// <summary>
    /// Reloads jobs and reinitializes state.
    /// </summary>
    void Refresh();
}
