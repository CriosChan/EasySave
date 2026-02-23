using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Data.Configuration;
using EasySave.Models.Data.Configuration;
using EasySave.Models.Utils;
using EasySave.Translations;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Handles application settings screen state and actions.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly StatusBarViewModel _statusBar;
    private readonly IUiTextService _uiTextService = new TlumachUiTextService();

    [ObservableProperty] private string _cryptoSoftKey = CryptoSoftConfiguration.Load().Key;

    [ObservableProperty] private ObservableCollection<string> _cryptoSoftExtensions = new(ApplicationConfiguration.Load().ExtensionToCrypt);
    [ObservableProperty] private string _newExtensionContent = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsViewModel" /> class.
    /// </summary>
    public SettingsViewModel(StatusBarViewModel statusBar)
    {
        _statusBar = statusBar;
    }

    /// <summary>
    ///     Applies French localization and persists the setting.
    /// </summary>
    [RelayCommand]
    private void SetFrenchLanguage()
    {
        ApplicationConfiguration.Load().Localization = "fr-FR";
        Strings.TranslationManager.CurrentCulture = new CultureInfo("fr-FR");
        _statusBar.StatusMessage = _uiTextService.Get("Gui_Status_LanguageChangedFr", "Langue changee en Francais");
    }

    /// <summary>
    ///     Applies English localization and persists the setting.
    /// </summary>
    [RelayCommand]
    private void SetEnglishLanguage()
    {
        ApplicationConfiguration.Load().Localization = "en-US";
        Strings.TranslationManager.CurrentCulture = new CultureInfo("en-US");
        _statusBar.StatusMessage = _uiTextService.Get("Gui_Status_LanguageChangedEn", "Language changed to English");
    }

    /// <summary>
    ///     Persists JSON log output type.
    /// </summary>
    [RelayCommand]
    private void SetJsonLogType()
    {
        ApplicationConfiguration.Load().LogType = "json";
        _statusBar.StatusMessage = _uiTextService.Get("Gui_Status_LogTypeJsonSet", "Log type set to JSON");
    }

    /// <summary>
    ///     Persists XML log output type.
    /// </summary>
    [RelayCommand]
    private void SetXmlLogType()
    {
        ApplicationConfiguration.Load().LogType = "xml";
        _statusBar.StatusMessage = _uiTextService.Get("Gui_Status_LogTypeXmlSet", "Log type set to XML");
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