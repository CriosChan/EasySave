using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EasySave.ViewModels;

public partial class MainWindowViewModel
{
    [ObservableProperty] private string _settingsScreenTitle = "Application Settings";
    [ObservableProperty] private string _settingsLanguageSectionTitle = "Language";
    [ObservableProperty] private string _settingsLogTypeSectionTitle = "Log Format";

    /// <summary>
    ///     Opens the settings screen in the current window.
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        _previousScreen = CurrentScreen;
        SetCurrentScreen(ViewScreen.Settings);
    }

    /// <summary>
    ///     Returns from the settings screen to the previous screen.
    /// </summary>
    [RelayCommand]
    private void BackFromSettings()
    {
        var target = _previousScreen == ViewScreen.Settings ? ViewScreen.Main : _previousScreen;
        SetCurrentScreen(target);
    }

    /// <summary>
    ///     Updates settings screen labels.
    /// </summary>
    private void UpdateSettingsUiText()
    {
        SettingsScreenTitle = UiText("Gui.Settings.Screen.Title", "Application Settings");
        SettingsLanguageSectionTitle = UiText("Gui.Settings.Section.Language", "Language");
        SettingsLogTypeSectionTitle = UiText("Gui.Settings.Section.LogType", "Log Format");
    }
}
