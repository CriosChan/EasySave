using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Data.Configuration;
using EasySave.Models.Data.Configuration;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Handles application settings screen state and actions.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly IUiLocalizationService _uiLocalizationService;
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

    // Priority extensions
    [ObservableProperty] private string _priorityExtensionsSectionTitle = string.Empty;
    [ObservableProperty] private string _addPriorityExtensionLabel = string.Empty;
    [ObservableProperty] private string _tooltipRemovePriorityExtension = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _priorityExtensions =
        new(ApplicationConfiguration.Load().PriorityExtensions);
    [ObservableProperty] private string _newPriorityExtensionContent = string.Empty;

    
    [ObservableProperty] private string _cryptoSoftKey = CryptoSoftConfiguration.Load().Key;

    [ObservableProperty] private string _settingsScreenTitle = string.Empty;
    [ObservableProperty] private string _xmlButtonLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _cryptoSoftExtensions = new(ApplicationConfiguration.Load().ExtensionToCrypt);
    [ObservableProperty] private string _newExtensionContent = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsViewModel" /> class.
    /// </summary>
    /// <param name="uiTextService">Localized UI text service.</param>
    /// <param name="uiLocalizationService">UI localization switch service.</param>
    public SettingsViewModel(
        StatusBarViewModel statusBar,
        IUiTextService uiTextService,
        IUiLocalizationService uiLocalizationService)
    {
        _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
        _uiTextService = uiTextService ?? throw new ArgumentNullException(nameof(uiTextService));
        _uiLocalizationService = uiLocalizationService ?? throw new ArgumentNullException(nameof(uiLocalizationService));
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
        PriorityExtensionsSectionTitle = _uiTextService.Get("Gui.Settings.PriorityExtensions", "Priority extensions");
        AddPriorityExtensionLabel = _uiTextService.Get("Gui.AddPriorityExtension", "Add Extension");
        TooltipRemovePriorityExtension = _uiTextService.Get("Gui.Tooltip.DeletePriorityExtension", "Remove priority extension.");
    }

    /// <summary>
    ///     Applies French localization and persists the setting.
    /// </summary>
    [RelayCommand]
    private void SetFrenchLanguage()
    {
        ApplicationConfiguration.Load().Localization = "fr-FR";
        _uiLocalizationService.Apply("fr-FR");
        _statusBar.StatusMessage = _uiTextService.Get("Gui.Status.LanguageChangedFr", "Langue changee en Francais");
    }

    /// <summary>
    ///     Applies English localization and persists the setting.
    /// </summary>
    [RelayCommand]
    private void SetEnglishLanguage()
    {
        ApplicationConfiguration.Load().Localization = "en-US";
        _uiLocalizationService.Apply("en-US");
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

    /// <summary>
    ///     Adds a normalised extension to the priority list and persists it.
    /// </summary>
    [RelayCommand]
    private void AddPriorityExtension()
    {
        var value = NewPriorityExtensionContent.Replace(".", "").Trim();
        if (value == string.Empty || PriorityExtensions.Contains(value))
            return;

        PriorityExtensions.Add(value);
        ApplicationConfiguration.Load().PriorityExtensions = PriorityExtensions.ToList();
        NewPriorityExtensionContent = string.Empty;
    }

    /// <summary>
    ///     Removes an extension from the priority list and persists the change.
    /// </summary>
    [RelayCommand]
    private void RemovePriorityExtension(string ext)
    {
        PriorityExtensions.Remove(ext);
        ApplicationConfiguration.Load().PriorityExtensions = PriorityExtensions.ToList();
    }

    partial void OnCryptoSoftKeyChanged(string value)
    {
        CryptoSoftConfiguration.Load().Key = value;
    }
}
