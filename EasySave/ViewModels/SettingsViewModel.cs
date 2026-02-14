using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Data.Configuration;
using EasySave.Models.Utils;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Handles application settings screen state and actions.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ILocalizationApplier _localizationApplier;
    private readonly IApplicationSettingsService _applicationSettingsService;
    private readonly IUiTextService _uiTextService;
    private readonly StatusBarViewModel _statusBar;
    private readonly Action _onLocalizationChanged;

    [ObservableProperty] private string _settingsScreenTitle = string.Empty;
    [ObservableProperty] private string _settingsLanguageSectionTitle = string.Empty;
    [ObservableProperty] private string _settingsLogTypeSectionTitle = string.Empty;
    [ObservableProperty] private string _frenchButtonLabel = string.Empty;
    [ObservableProperty] private string _englishButtonLabel = string.Empty;
    [ObservableProperty] private string _jsonButtonLabel = string.Empty;
    [ObservableProperty] private string _xmlButtonLabel = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsViewModel" /> class.
    /// </summary>
    /// <param name="localizationApplier">Localization applier service.</param>
    /// <param name="applicationSettingsService">Application settings persistence service.</param>
    /// <param name="uiTextService">Localized text service.</param>
    /// <param name="statusBar">Shared status bar state.</param>
    /// <param name="onLocalizationChanged">Callback invoked after language changes.</param>
    public SettingsViewModel(
        ILocalizationApplier localizationApplier,
        IApplicationSettingsService applicationSettingsService,
        IUiTextService uiTextService,
        StatusBarViewModel statusBar,
        Action onLocalizationChanged)
    {
        _localizationApplier = localizationApplier ?? throw new ArgumentNullException(nameof(localizationApplier));
        _applicationSettingsService =
            applicationSettingsService ?? throw new ArgumentNullException(nameof(applicationSettingsService));
        _uiTextService = uiTextService ?? throw new ArgumentNullException(nameof(uiTextService));
        _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
        _onLocalizationChanged = onLocalizationChanged ?? throw new ArgumentNullException(nameof(onLocalizationChanged));

        UpdateUiText();
    }

    /// <summary>
    ///     Updates settings labels from localization resources.
    /// </summary>
    public void UpdateUiText()
    {
        SettingsScreenTitle = _uiTextService.Get("Gui.Settings.Screen.Title", "Application Settings");
        SettingsLanguageSectionTitle = _uiTextService.Get("Gui.Settings.Section.Language", "Language");
        SettingsLogTypeSectionTitle = _uiTextService.Get("Gui.Settings.Section.LogType", "Log Format");
        FrenchButtonLabel = _uiTextService.Get("Gui.Button.French", "Francais");
        EnglishButtonLabel = _uiTextService.Get("Gui.Button.English", "English");
        JsonButtonLabel = _uiTextService.Get("Gui.Button.Json", "JSON");
        XmlButtonLabel = _uiTextService.Get("Gui.Button.Xml", "XML");
    }

    /// <summary>
    ///     Applies French localization and persists the setting.
    /// </summary>
    [RelayCommand]
    private void SetFrenchLanguage()
    {
        _applicationSettingsService.SetLocalization("fr-FR");
        _localizationApplier.Apply("fr-FR");
        _onLocalizationChanged();
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LanguageChangedFr", "Langue changee en Francais");
    }

    /// <summary>
    ///     Applies English localization and persists the setting.
    /// </summary>
    [RelayCommand]
    private void SetEnglishLanguage()
    {
        _applicationSettingsService.SetLocalization("en-US");
        _localizationApplier.Apply("en-US");
        _onLocalizationChanged();
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LanguageChangedEn", "Language changed to English");
    }

    /// <summary>
    ///     Persists JSON log output type.
    /// </summary>
    [RelayCommand]
    private void SetJsonLogType()
    {
        _applicationSettingsService.SetLogType("json");
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LogTypeJsonSet", "Log type set to JSON");
    }

    /// <summary>
    ///     Persists XML log output type.
    /// </summary>
    [RelayCommand]
    private void SetXmlLogType()
    {
        _applicationSettingsService.SetLogType("xml");
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LogTypeXmlSet", "Log type set to XML");
    }
}
