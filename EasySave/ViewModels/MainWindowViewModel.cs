using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Platform.Storage;
using EasySave.Data.Configuration;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.BusinessSoftware;
using EasySave.Models.Utils;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Main window orchestrator ViewModel.
///     Coordinates screen navigation and composes specialized child ViewModels.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IUiTextService _uiTextService;
    private ViewScreen _previousScreen = ViewScreen.Main;

    [ObservableProperty] private string _windowTitle = string.Empty;
    [ObservableProperty] private string _menuLabel = string.Empty;
    [ObservableProperty] private string _menuSettingsItemLabel = string.Empty;
    [ObservableProperty] private string _manageBusinessSoftwareMenuItemLabel = string.Empty;
    [ObservableProperty] private string _backButtonLabel = string.Empty;
    [ObservableProperty] private ViewScreen _currentScreen = ViewScreen.Main;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
    /// </summary>
    public MainWindowViewModel()
    {
        _uiTextService = new ResxUiTextService();

        StatusBar = new StatusBarViewModel();
        Jobs = new JobsViewModel(StatusBar);
        Settings = new SettingsViewModel(StatusBar,
            RefreshLocalizedUi);
        BusinessSoftware = new BusinessSoftwareViewModel(
            StatusBar);

        BusinessSoftware.ConfiguredProcessNamesChanged += OnConfiguredProcessNamesChanged;
        BusinessSoftware.OpenAddedSoftwareRequested += OnOpenAddedSoftwareRequested;

        ApplyConfiguredLocalization();
        RefreshLocalizedUi();
        BusinessSoftware.Initialize();
        StatusBar.StatusMessage = _uiTextService.Get("Gui.Status.Ready", "Ready");
    }

    /// <summary>
    ///     Gets the jobs section ViewModel.
    /// </summary>
    public JobsViewModel Jobs { get; }

    /// <summary>
    ///     Gets the settings section ViewModel.
    /// </summary>
    public SettingsViewModel Settings { get; }

    /// <summary>
    ///     Gets the business software section ViewModel.
    /// </summary>
    public BusinessSoftwareViewModel BusinessSoftware { get; }

    /// <summary>
    ///     Gets the global status bar ViewModel.
    /// </summary>
    public StatusBarViewModel StatusBar { get; }

    /// <summary>
    ///     Gets a value indicating whether the main screen is visible.
    /// </summary>
    public bool IsMainScreen => CurrentScreen == ViewScreen.Main;

    /// <summary>
    ///     Gets a value indicating whether the settings screen is visible.
    /// </summary>
    public bool IsSettingsScreen => CurrentScreen == ViewScreen.Settings;

    /// <summary>
    ///     Gets a value indicating whether the software catalog screen is visible.
    /// </summary>
    public bool IsSoftwareCatalogScreen => CurrentScreen == ViewScreen.SoftwareCatalog;

    /// <summary>
    ///     Gets a value indicating whether the added software screen is visible.
    /// </summary>
    public bool IsAddedSoftwareScreen => CurrentScreen == ViewScreen.AddedSoftware;

    /// <summary>
    ///     Forwards the storage provider to the jobs ViewModel.
    /// </summary>
    /// <param name="storageProvider">Window storage provider.</param>
    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        Jobs.SetStorageProvider(storageProvider);
    }

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
    ///     Returns from settings screen to previous screen.
    /// </summary>
    [RelayCommand]
    private void BackFromSettings()
    {
        var target = _previousScreen == ViewScreen.Settings ? ViewScreen.Main : _previousScreen;
        SetCurrentScreen(target);
    }

    /// <summary>
    ///     Opens the business software catalog screen.
    /// </summary>
    [RelayCommand]
    private void OpenBusinessSoftwareCatalog()
    {
        BusinessSoftware.OpenCatalog();
        SetCurrentScreen(ViewScreen.SoftwareCatalog);
    }

    /// <summary>
    ///     Opens the configured business software list screen.
    /// </summary>
    [RelayCommand]
    private void OpenAddedBusinessSoftware()
    {
        BusinessSoftware.OpenAddedSoftware();
        _previousScreen = CurrentScreen;
        SetCurrentScreen(ViewScreen.AddedSoftware);
    }

    /// <summary>
    ///     Returns from software catalog to main screen.
    /// </summary>
    [RelayCommand]
    private void BackFromSoftwareCatalog()
    {
        SetCurrentScreen(ViewScreen.Main);
    }

    /// <summary>
    ///     Returns from added software screen to previous screen.
    /// </summary>
    [RelayCommand]
    private void BackFromAddedSoftware()
    {
        SetCurrentScreen(_previousScreen);
    }

    partial void OnCurrentScreenChanged(ViewScreen value)
    {
        OnPropertyChanged(nameof(IsMainScreen));
        OnPropertyChanged(nameof(IsSettingsScreen));
        OnPropertyChanged(nameof(IsSoftwareCatalogScreen));
        OnPropertyChanged(nameof(IsAddedSoftwareScreen));
    }

    /// <summary>
    ///     Applies localization from persisted configuration at startup.
    /// </summary>
    private void ApplyConfiguredLocalization()
    {
        var config = ApplicationConfiguration.Load();
        if (!string.IsNullOrWhiteSpace(config.Localization))
            LocalizationApplier.Apply(config.Localization);
    }

    /// <summary>
    ///     Refreshes localized labels for this ViewModel and all child ViewModels.
    /// </summary>
    private void RefreshLocalizedUi()
    {
        WindowTitle = _uiTextService.Get("Gui.Window.Title", "EasySave - Backup Manager");
        MenuLabel = _uiTextService.Get("Gui.Menu.Root", "Menu");
        MenuSettingsItemLabel = _uiTextService.Get("Gui.Menu.Settings", "Settings");
        ManageBusinessSoftwareMenuItemLabel =
            _uiTextService.Get("Gui.Menu.ManageBusinessSoftware", "Manage Business Software");
        BackButtonLabel = _uiTextService.Get("Gui.Navigation.Back", "Back");

        Jobs.UpdateUiText();
        Settings.UpdateUiText();
        BusinessSoftware.UpdateUiText();
    }

    /// <summary>
    ///     Handles configured process name changes and refreshes job monitor instances.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void OnConfiguredProcessNamesChanged(object? sender, EventArgs e)
    {
        Jobs.RefreshBusinessSoftwareMonitors();
    }

    /// <summary>
    ///     Handles navigation requests emitted after adding business software.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void OnOpenAddedSoftwareRequested(object? sender, EventArgs e)
    {
        _previousScreen = ViewScreen.SoftwareCatalog;
        SetCurrentScreen(ViewScreen.AddedSoftware);
    }

    /// <summary>
    ///     Sets the currently visible screen.
    /// </summary>
    /// <param name="screen">Target screen value.</param>
    private void SetCurrentScreen(ViewScreen screen)
    {
        CurrentScreen = screen;
    }

    /// <summary>
    ///     Supported in-window screens.
    /// </summary>
    public enum ViewScreen
    {
        Main,
        Settings,
        SoftwareCatalog,
        AddedSoftware
    }
}
