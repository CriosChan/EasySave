using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.ViewModels;

/// <summary>
///     Selectable item used in business software selection screens.
/// </summary>
public partial class SelectableBusinessSoftwareItemViewModel : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SelectableBusinessSoftwareItemViewModel" /> class.
    /// </summary>
    /// <param name="displayName">Display name shown in UI.</param>
    /// <param name="processName">Process name used for runtime detection.</param>
    public SelectableBusinessSoftwareItemViewModel(string displayName, string processName)
    {
        DisplayName = displayName;
        ProcessName = processName;
    }

    /// <summary>
    ///     Gets the display name shown in UI.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    ///     Gets the process name used by detection.
    /// </summary>
    public string ProcessName { get; }

    /// <summary>
    ///     Gets a display label combining software name and process name.
    /// </summary>
    public string DisplayLabel => $"{DisplayName} ({ProcessName})";

    /// <summary>
    ///     Raised whenever selection state changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    partial void OnIsSelectedChanged(bool value)
    {
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}