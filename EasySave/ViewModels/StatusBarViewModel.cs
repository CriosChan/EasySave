using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.ViewModels;

/// <summary>
///     Holds global status and progress state displayed in the bottom status layer.
/// </summary>
public partial class StatusBarViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isNotBusy = true;
    [ObservableProperty] private double _overallProgress;
    [ObservableProperty] private string _statusMessage = string.Empty;
}