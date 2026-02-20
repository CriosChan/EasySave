using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.ViewModels;

/// <summary>
///     Holds global status and progress state displayed in the bottom status layer.
/// </summary>
public partial class StatusBarViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isNotBusy = true;
    [ObservableProperty] private double _overallProgress;
    [ObservableProperty] private double _maxProgress;
    [ObservableProperty] private string _statusMessage = string.Empty;
    private readonly Lock _lockOverall = new();
    private readonly Lock _lockMax = new();

    public void UpdateOverallProgress()
    {
        lock (_lockOverall)
        {
            OverallProgress += 1;
        }
    }

    public void AddMaxProgress(double max)
    {
        lock (_lockMax)
        {
            MaxProgress += max;
        }
    }

    public void RemoveProgress(double number)
    {
        lock (_lockMax)
        {
            lock (_lockOverall)
            {
                MaxProgress -= number;
                OverallProgress -= number;
            }
        }
    }
}