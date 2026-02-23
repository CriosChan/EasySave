using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.ViewModels;

/// <summary>
/// Holds the global status and progress state displayed in the bottom status layer of the application.
/// </summary>
public partial class StatusBarViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isNotBusy = true;
    [ObservableProperty] private double _overallProgress;
    [ObservableProperty] private double _maxProgress;
    [ObservableProperty] private string _statusMessage = string.Empty;
}
