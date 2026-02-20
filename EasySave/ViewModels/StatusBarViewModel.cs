using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.ViewModels;

/// <summary>
/// Holds the global status and progress state displayed in the bottom status layer of the application.
/// </summary>
public partial class StatusBarViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isNotBusy = true; // Indicates if the application is busy or not
    [ObservableProperty] private double _overallProgress; // Tracks overall progress as a percentage
    [ObservableProperty] private double _maxProgress; // Represents the maximum progress threshold
    [ObservableProperty] private string _statusMessage = string.Empty; // Message to display in the status bar

    private readonly Lock _lockOverall = new(); // Lock for synchronizing access to overall progress
    private readonly Lock _lockMax = new(); // Lock for synchronizing access to maximum progress

    /// <summary>
    /// Increments the overall progress by one unit.
    /// </summary>
    public void UpdateOverallProgress()
    {
        lock (_lockOverall) // Ensure thread-safe access to overall progress
        {
            OverallProgress += 1; // Increment overall progress
        }
    }

    /// <summary>
    /// Increases the maximum progress threshold by the specified amount.
    /// </summary>
    /// <param name="max">The amount to increase max progress by.</param>
    public void AddMaxProgress(double max)
    {
        lock (_lockMax) // Ensure thread-safe access to max progress
        {
            MaxProgress += max; // Increase maximum progress
        }
    }

    /// <summary>
    /// Removes a specified number from both overall and maximum progress.
    /// </summary>
    /// <param name="number">The amount to decrease from both progress values.</param>
    public void RemoveProgress(double number)
    {
        lock (_lockMax) // Ensure thread-safe access to max progress
        {
            lock (_lockOverall) // Ensure thread-safe access to overall progress
            {
                MaxProgress -= number; // Decrease maximum progress
                OverallProgress -= number; // Decrease overall progress
            }
        }
    }
}
