using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Data.Configuration;
using EasySave.Models.Logger;
using EasySave.Models.Utils;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Handles application settings screen state and actions.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly StatusBarViewModel _statusBar;
    private readonly IUiLocalizationService _uiLocalizationService;
    private readonly IUiTextService _uiTextService;

    [ObservableProperty]
    private ObservableCollection<string> _cryptoSoftExtensions = new(ApplicationConfiguration.Load().ExtensionToCrypt);

    [ObservableProperty] private string _cryptoSoftKey = CryptoSoftConfiguration.Load().Key;
    [ObservableProperty] private string _newExtensionContent = string.Empty;
    [ObservableProperty] private string _newPriorityExtensionContent = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _priorityExtensions = new(ApplicationConfiguration.Load().PriorityExtensions);

    [ObservableProperty]
    private string _largeFileThresholdKo = ApplicationConfiguration.Load().LargeFileThresholdKo.ToString();

    [ObservableProperty] private string _routingIp = ApplicationConfiguration.Load().EasySaveServerIp;
    [ObservableProperty] private string _routingPort = ApplicationConfiguration.Load().EasySaveServerPort.ToString();
    [ObservableProperty] private RoutingType _selectedRoutingType = ApplicationConfiguration.Load().RoutingType;

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
        _uiLocalizationService =
            uiLocalizationService ?? throw new ArgumentNullException(nameof(uiLocalizationService));
    }

    public List<RoutingType> RoutingTypes => Enum.GetValues(typeof(RoutingType)).Cast<RoutingType>().ToList();

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

    /// <summary>
    ///     Adds a new extension to the CryptoSoft extensions list and persists the configuration.
    ///     Ignores empty values and duplicates.
    /// </summary>
    [RelayCommand]
    private void AddExtensionToCryptoSoft()
    {
        var value = NewExtensionContent.Replace(".", "").Trim();
        if (value == string.Empty || CryptoSoftExtensions.Contains(value)) return;

        CryptoSoftExtensions.Add(value);
        ApplicationConfiguration.Load().ExtensionToCrypt = CryptoSoftExtensions.ToList();
        NewExtensionContent = string.Empty;
    }

    /// <summary>
    ///     Removes an extension from the CryptoSoft extensions list and persists the configuration.
    /// </summary>
    [RelayCommand]
    private void RemoveExtension(string ext)
    {
        CryptoSoftExtensions.Remove(ext);
        ApplicationConfiguration.Load().ExtensionToCrypt = CryptoSoftExtensions.ToList();
    }

    /// <summary>
    ///     Adds a new priority extension to the list and persists the configuration.
    ///     Ignores empty values and duplicates.
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
    ///     Removes a priority extension from the list and persists the configuration.
    /// </summary>
    [RelayCommand]
    private void RemovePriorityExtension(string ext)
    {
        PriorityExtensions.Remove(ext);
        ApplicationConfiguration.Load().PriorityExtensions = PriorityExtensions.ToList();
    }

    /// <summary>
    ///     Updates the CryptoSoft configuration key.
    /// </summary>
    partial void OnCryptoSoftKeyChanged(string value)
    {
        CryptoSoftConfiguration.Load().Key = value;
    }

    /// <summary>
    ///     Updates the EasySave server IP address if valid.
    ///     Initiates socket creation if the routing type is not local.
    /// </summary>
    partial void OnRoutingIpChanged(string value)
    {
        if (Validator.IsValidIPv4(value))
        {
            ApplicationConfiguration.Load().EasySaveServerIp = value;
            if (ApplicationConfiguration.Load().RoutingType != RoutingType.Local)
                new Thread(() => NetworkLog.Instance.CreateSocket()).Start();
        }
    }

    /// <summary>
    ///     Updates the EasySave server port if valid.
    ///     Initiates socket creation if the routing type is not local.
    /// </summary>
    partial void OnRoutingPortChanged(string value)
    {
        try
        {
            ApplicationConfiguration.Load().EasySaveServerPort = int.Parse(value);
            if (ApplicationConfiguration.Load().RoutingType != RoutingType.Local)
                new Thread(() => NetworkLog.Instance.CreateSocket()).Start();
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    ///     Updates the large-file threshold value in Ko if the input is valid.
    /// </summary>
    partial void OnLargeFileThresholdKoChanged(string value)
    {
        if (int.TryParse(value, out var thresholdKo) && thresholdKo > 0)
            ApplicationConfiguration.Load().LargeFileThresholdKo = thresholdKo;
    }

    /// <summary>
    ///     Updates the routing type and initiates socket creation if not local.
    /// </summary>
    partial void OnSelectedRoutingTypeChanged(RoutingType value)
    {
        ApplicationConfiguration.Load().RoutingType = value;
        if (value != RoutingType.Local) new Thread(() => NetworkLog.Instance.CreateSocket()).Start();
    }
}
