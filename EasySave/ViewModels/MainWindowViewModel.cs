using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Data.Configuration;
using EasySave.Models.Logger;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Main window orchestrator ViewModel.
///     Coordinates screen navigation and composes specialized child ViewModels.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    ///     Supported in-window screens.
    /// </summary>
    public enum ViewScreen
    {
        Main,
        Settings,
        SoftwareCatalog,
        AddedSoftware,
        EditBackup
    }

    private readonly IUiLocalizationService _uiLocalizationService;

    private readonly IUiTextService _uiTextService;
    [ObservableProperty] private ViewScreen _currentScreen = ViewScreen.Main;
    private ViewScreen _previousScreen = ViewScreen.Main;
    [ObservableProperty] private SolidColorBrush _serverColor = SolidColorBrush.Parse("#008000");
    [ObservableProperty] private string _serverText = "";
    [ObservableProperty] private bool _useServer = ApplicationConfiguration.Load().RoutingType != RoutingType.Local;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
    /// </summary>
    public MainWindowViewModel()
    {
        _uiTextService = new TlumachUiTextService();
        _uiLocalizationService = new TlumachUiLocalizationService();

        StatusBar = new StatusBarViewModel(_uiTextService);
        Jobs = new JobsViewModel(StatusBar, _uiTextService);
        Settings = new SettingsViewModel(StatusBar, _uiTextService, _uiLocalizationService);
        EditBackup = new EditBackupViewModel(StatusBar, _uiTextService);
        BusinessSoftware = new BusinessSoftwareViewModel(
            StatusBar,
            _uiTextService);

        Jobs.EditJobRequested += OnEditJobRequested;
        EditBackup.JobUpdated += OnBackupJobUpdated;
        BusinessSoftware.ConfiguredProcessNamesChanged += OnConfiguredProcessNamesChanged;
        BusinessSoftware.OpenAddedSoftwareRequested += OnOpenAddedSoftwareRequested;

        NetworkLog.Instance.OnConnect += OnServerConnection;
        NetworkLog.Instance.OnDisconnect += OnServerDisconnect;

        if (ApplicationConfiguration.Load().RoutingType != RoutingType.Local) NetworkLog.Instance.CreateSocket();

        ApplyConfiguredLocalization();
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
    ///     Gets the backup edition screen ViewModel.
    /// </summary>
    public EditBackupViewModel EditBackup { get; }

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
    public bool IsJobScreen => CurrentScreen == ViewScreen.Main;

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
    ///     Gets a value indicating whether the backup edition screen is visible.
    /// </summary>
    public bool IsEditBackupScreen => CurrentScreen == ViewScreen.EditBackup;

    /// <summary>
    ///     Forwards the storage provider to the jobs ViewModel.
    /// </summary>
    /// <param name="storageProvider">Window storage provider.</param>
    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        Jobs.SetStorageProvider(storageProvider);
        EditBackup.SetStorageProvider(storageProvider);
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

    /// <summary>
    ///     Returns from backup edition screen to main screen.
    /// </summary>
    [RelayCommand]
    private void BackFromEditBackup()
    {
        SetCurrentScreen(ViewScreen.Main);
    }

    partial void OnCurrentScreenChanged(ViewScreen value)
    {
        OnPropertyChanged(nameof(IsJobScreen));
        OnPropertyChanged(nameof(IsSettingsScreen));
        OnPropertyChanged(nameof(IsSoftwareCatalogScreen));
        OnPropertyChanged(nameof(IsAddedSoftwareScreen));
        OnPropertyChanged(nameof(IsEditBackupScreen));
    }

    /// <summary>
    ///     Applies localization from persisted configuration at startup.
    /// </summary>
    private void ApplyConfiguredLocalization()
    {
        var config = ApplicationConfiguration.Load();
        if (!string.IsNullOrWhiteSpace(config.Localization))
            _uiLocalizationService.Apply(config.Localization);
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
    ///     Opens backup edition for the selected job.
    /// </summary>
    /// <param name="job">Job to edit.</param>
    private void OnEditJobRequested(BackupJob job)
    {
        EditBackup.BeginEdit(job);
        SetCurrentScreen(ViewScreen.EditBackup);
    }

    /// <summary>
    ///     Refreshes job list when a job has been updated from edition screen.
    /// </summary>
    /// <param name="job">Updated job.</param>
    private void OnBackupJobUpdated(BackupJob job)
    {
        Jobs.RefreshJobs();
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
    ///     Updates the UI to reflect that the server is online.
    ///     Changes the server color to green and updates the status text.
    /// </summary>
    private void OnServerConnection(object? sender, EventArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            UseServer = true;
            ServerColor = SolidColorBrush.Parse("#27F535");
            ServerText = _uiTextService.Get("Gui.Status.ServerOnline", "Server: Online");
        });
    }

    /// <summary>
    ///     Updates the UI to reflect that the server is offline.
    ///     Changes the server color to red and updates the status text.
    /// </summary>
    private void OnServerDisconnect(object? sender, EventArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            UseServer = true;
            ServerColor = SolidColorBrush.Parse("#F52727");
            ServerText = _uiTextService.Get("Gui.Status.ServerOffline", "Server: Offline");
        });
    }
}
