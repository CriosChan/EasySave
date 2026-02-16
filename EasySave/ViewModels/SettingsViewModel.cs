using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Data.Configuration;
using EasySave.Models.Data.Configuration;
using EasySave.Models.Utils;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Handles application settings screen state and actions.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly Action _onLocalizationChanged;
    private readonly StatusBarViewModel _statusBar;
    private readonly IUiTextService _uiTextService;
    [ObservableProperty] private string _englishButtonLabel = string.Empty;
    [ObservableProperty] private string _frenchButtonLabel = string.Empty;
    [ObservableProperty] private string _jsonButtonLabel = string.Empty;
    [ObservableProperty] private string _settingsLanguageSectionTitle = string.Empty;
    [ObservableProperty] private string _settingsLogTypeSectionTitle = string.Empty;
    [ObservableProperty] private string _settingsCryptoSoftKey = string.Empty;
    [ObservableProperty] private string _tooltipRemoveExtension = string.Empty;
    [ObservableProperty] private string _addExtensionToList = string.Empty;
    [ObservableProperty] private string _extensionToCrypt = string.Empty;

    
    [ObservableProperty] private string _cryptoSoftKey = CryptoSoftConfiguration.Load().Key;

    [ObservableProperty] private string _settingsScreenTitle = string.Empty;
    [ObservableProperty] private string _xmlButtonLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _cryptoSoftExtensions = new(ApplicationConfiguration.Load().ExtensionToCrypt);
    [ObservableProperty] private string _newExtensionContent = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsViewModel" /> class.
    /// </summary>
    public SettingsViewModel(StatusBarViewModel statusBar, Action onLocalizationChanged)
    {
        _statusBar = statusBar;
        _uiTextService = new ResxUiTextService();
        _onLocalizationChanged = onLocalizationChanged;
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
        SettingsCryptoSoftKey = _uiTextService.Get("Gui.Settings.CryptoSoftKey", "CryptoSoft Key");
        TooltipRemoveExtension = _uiTextService.Get("Gui.Tooltip.DeleteExtension", "Supprimer l'extension");
        AddExtensionToList = _uiTextService.Get("Gui.AddExtension", "Add Extension");
        ExtensionToCrypt = _uiTextService.Get("Gui.Settings.ExtensionToCrypt", "Extension to Crypt");
    }

    /// <summary>
    ///     Applies French localization and persists the setting.
    /// </summary>
    [RelayCommand]
    private void SetFrenchLanguage()
    {
        ApplicationConfiguration.Load().Localization = "fr-FR";
        LocalizationApplier.Apply("fr-FR");
        _onLocalizationChanged();
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LanguageChangedFr", "Langue changee en Francais");
    }

    /// <summary>
    ///     Applies English localization and persists the setting.
    /// </summary>
    [RelayCommand]
    private void SetEnglishLanguage()
    {
        ApplicationConfiguration.Load().Localization = "en-US";
        LocalizationApplier.Apply("en-US");
        _onLocalizationChanged();
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LanguageChangedEn", "Language changed to English");
    }

    /// <summary>
    ///     Persists JSON log output type.
    /// </summary>
    [RelayCommand]
    private void SetJsonLogType()
    {
        ApplicationConfiguration.Load().LogType = "json";
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LogTypeJsonSet", "Log type set to JSON");
    }

    /// <summary>
    ///     Persists XML log output type.
    /// </summary>
    [RelayCommand]
    private void SetXmlLogType()
    {
        ApplicationConfiguration.Load().LogType = "xml";
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LogTypeXmlSet", "Log type set to XML");
    }

    [RelayCommand]
    private void AddExtensionToCryptoSoft()
    {
        var value = NewExtensionContent.Replace(".", "").Trim();
        if (value == string.Empty || CryptoSoftExtensions.Contains(value))
        {
            return;
        }
        
        CryptoSoftExtensions.Add(value);
        ApplicationConfiguration.Load().ExtensionToCrypt = CryptoSoftExtensions.ToList();
        NewExtensionContent = string.Empty;
    }

    [RelayCommand]
    private void RemoveExtension(string ext)
    {
        CryptoSoftExtensions.Remove(ext);
        ApplicationConfiguration.Load().ExtensionToCrypt = CryptoSoftExtensions.ToList();
    }

    partial void OnCryptoSoftKeyChanged(string value)
    {
        CryptoSoftConfiguration.Load().Key = value;
    }
}