using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Data.Configuration;
using EasySave.Models.Data.Configuration;
using EasySave.Models.Utils;
using EasySave.ViewModels.Services;
using Tlumach.Avalonia;

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
    ///     Gets the localized settings screen title.
    /// </summary>
    public TranslationUnit SettingsScreenTitle { get; } = Localizer.CreateUnit("Gui.Settings.Screen.Title");

    /// <summary>
    ///     Gets the localized language section title.
    /// </summary>
    public TranslationUnit SettingsLanguageSectionTitle { get; } = Localizer.CreateUnit("Gui.Settings.Section.Language");

    /// <summary>
    ///     Gets the localized log type section title.
    /// </summary>
    public TranslationUnit SettingsLogTypeSectionTitle { get; } = Localizer.CreateUnit("Gui.Settings.Section.LogType");

    /// <summary>
    ///     Gets the localized French button label.
    /// </summary>
    public TranslationUnit FrenchButtonLabel { get; } = Localizer.CreateUnit("Gui.Button.French");

    /// <summary>
    ///     Gets the localized English button label.
    /// </summary>
    public TranslationUnit EnglishButtonLabel { get; } = Localizer.CreateUnit("Gui.Button.English");

    /// <summary>
    ///     Gets the localized JSON button label.
    /// </summary>
    public TranslationUnit JsonButtonLabel { get; } = Localizer.CreateUnit("Gui.Button.Json");

    /// <summary>
    ///     Gets the localized XML button label.
    /// </summary>
    public TranslationUnit XmlButtonLabel { get; } = Localizer.CreateUnit("Gui.Button.Xml");

    /// <summary>
    ///     Gets the localized CryptoSoft key section label.
    /// </summary>
    public TranslationUnit SettingsCryptoSoftKey { get; } = Localizer.CreateUnit("Gui.Settings.CryptoSoftKey");

    /// <summary>
    ///     Gets the localized tooltip for removing an extension.
    /// </summary>
    public TranslationUnit TooltipRemoveExtension { get; } = Localizer.CreateUnit("Gui.Tooltip.DeleteExtension");

    /// <summary>
    ///     Gets the localized add extension button label.
    /// </summary>
    public TranslationUnit AddExtensionToList { get; } = Localizer.CreateUnit("Gui.AddExtension");

    /// <summary>
    ///     Gets the localized extension to crypt section label.
    /// </summary>
    public TranslationUnit ExtensionToCrypt { get; } = Localizer.CreateUnit("Gui.Settings.ExtensionToCrypt");

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
        LocalizationApplier.Apply("fr-FR");
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