using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Models.Backup;
using EasySave.Models.BusinessSoftware;

namespace EasySave.ViewModels;

public partial class MainWindowViewModel
{
    private readonly IBusinessSoftwareCatalogService _businessSoftwareCatalogService = new BusinessSoftwareCatalogService();
    private readonly IBusinessSoftwareSettingsService _businessSoftwareSettingsService = new BusinessSoftwareSettingsService();

    private ViewScreen _previousScreen = ViewScreen.Main;

    [ObservableProperty] private ViewScreen _currentScreen = ViewScreen.Main;
    [ObservableProperty] private ObservableCollection<SelectableBusinessSoftwareItemViewModel> _allAvailableBusinessSoftware = [];
    [ObservableProperty] private ObservableCollection<SelectableBusinessSoftwareItemViewModel> _filteredBusinessSoftware = [];
    [ObservableProperty] private ObservableCollection<SelectableBusinessSoftwareItemViewModel> _addedBusinessSoftware = [];
    [ObservableProperty] private string _businessSoftwareSearchText = string.Empty;

    [ObservableProperty] private string _featuresMenuLabel = "Features";
    [ObservableProperty] private string _manageBusinessSoftwareMenuItemLabel = "Manage Business Software";
    [ObservableProperty] private string _backButtonLabel = "Back";
    [ObservableProperty] private string _availableBusinessSoftwareTitle = "Select Business Software";
    [ObservableProperty] private string _addedBusinessSoftwareTitle = "Added Business Software";
    [ObservableProperty] private string _businessSoftwareSearchLabel = "Search software";
    [ObservableProperty] private string _businessSoftwareSearchPlaceholder = "Type to filter software";
    [ObservableProperty] private string _addSelectedBusinessSoftwareButtonLabel = "Add Selected";
    [ObservableProperty] private string _viewAddedBusinessSoftwareButtonLabel = "View Added Software";
    [ObservableProperty] private string _removeAddedBusinessSoftwareButtonLabel = "Remove Selected";

    /// <summary>
    ///     Gets a value indicating whether the main backup manager screen is visible.
    /// </summary>
    public bool IsMainScreen => CurrentScreen == ViewScreen.Main;

    /// <summary>
    ///     Gets a value indicating whether the software catalog screen is visible.
    /// </summary>
    public bool IsSoftwareCatalogScreen => CurrentScreen == ViewScreen.SoftwareCatalog;

    /// <summary>
    ///     Gets a value indicating whether the added software screen is visible.
    /// </summary>
    public bool IsAddedSoftwareScreen => CurrentScreen == ViewScreen.AddedSoftware;

    /// <summary>
    ///     Gets a value indicating whether at least one available software item is selected.
    /// </summary>
    public bool CanAddSelectedBusinessSoftware => AllAvailableBusinessSoftware.Any(item => item.IsSelected);

    /// <summary>
    ///     Gets a value indicating whether at least one configured software item is selected.
    /// </summary>
    public bool CanRemoveAddedBusinessSoftware => AddedBusinessSoftware.Any(item => item.IsSelected);

    /// <summary>
    ///     Initializes business software management state.
    /// </summary>
    private void InitializeBusinessSoftwareManagement()
    {
        CurrentScreen = ViewScreen.Main;
        LoadAddedBusinessSoftware();
        ApplyBusinessSoftwareFilter();
    }

    /// <summary>
    ///     Updates business software screen labels.
    /// </summary>
    private void UpdateBusinessSoftwareUiText()
    {
        FeaturesMenuLabel = UiText("Gui.Menu.Features", "Features");
        ManageBusinessSoftwareMenuItemLabel =
            UiText("Gui.Menu.ManageBusinessSoftware", "Manage Business Software");
        BackButtonLabel = UiText("Gui.Navigation.Back", "Back");
        AvailableBusinessSoftwareTitle =
            UiText("Gui.BusinessSoftware.Catalog.Title", "Select Business Software");
        AddedBusinessSoftwareTitle = UiText("Gui.BusinessSoftware.Added.Title", "Added Business Software");
        BusinessSoftwareSearchLabel = UiText("Gui.BusinessSoftware.Search.Label", "Search software");
        BusinessSoftwareSearchPlaceholder =
            UiText("Gui.BusinessSoftware.Search.Placeholder", "Type to filter software");
        AddSelectedBusinessSoftwareButtonLabel =
            UiText("Gui.BusinessSoftware.Button.AddSelected", "Add Selected");
        ViewAddedBusinessSoftwareButtonLabel =
            UiText("Gui.BusinessSoftware.Button.ViewAdded", "View Added Software");
        RemoveAddedBusinessSoftwareButtonLabel =
            UiText("Gui.BusinessSoftware.Button.RemoveSelected", "Remove Selected");
    }

    /// <summary>
    ///     Opens the business software catalog screen.
    /// </summary>
    [RelayCommand]
    private void OpenBusinessSoftwareCatalog()
    {
        LoadAvailableBusinessSoftware();
        LoadAddedBusinessSoftware();
        BusinessSoftwareSearchText = string.Empty;
        SetCurrentScreen(ViewScreen.SoftwareCatalog);
    }

    /// <summary>
    ///     Opens the screen that lists already configured software blockers.
    /// </summary>
    [RelayCommand]
    private void OpenAddedBusinessSoftware()
    {
        LoadAddedBusinessSoftware();
        _previousScreen = CurrentScreen;
        SetCurrentScreen(ViewScreen.AddedSoftware);
    }

    /// <summary>
    ///     Returns from the software catalog screen to the main screen.
    /// </summary>
    [RelayCommand]
    private void BackFromSoftwareCatalog()
    {
        SetCurrentScreen(ViewScreen.Main);
    }

    /// <summary>
    ///     Returns from the added software screen to the previous screen.
    /// </summary>
    [RelayCommand]
    private void BackFromAddedSoftware()
    {
        SetCurrentScreen(_previousScreen);
    }

    /// <summary>
    ///     Adds selected available software items to the configured blocker list.
    /// </summary>
    [RelayCommand]
    private void AddSelectedBusinessSoftware()
    {
        var selectedItems = AllAvailableBusinessSoftware.Where(item => item.IsSelected).ToList();
        if (selectedItems.Count == 0)
            return;

        var existingProcessNames = AddedBusinessSoftware
            .Select(item => item.ProcessName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var selectedItem in selectedItems)
        {
            if (existingProcessNames.Contains(selectedItem.ProcessName))
                continue;

            var addedItem = new SelectableBusinessSoftwareItemViewModel(selectedItem.DisplayName, selectedItem.ProcessName);
            addedItem.SelectionChanged += OnAddedSoftwareSelectionChanged;
            AddedBusinessSoftware.Add(addedItem);
            existingProcessNames.Add(selectedItem.ProcessName);
        }

        foreach (var selectedItem in selectedItems)
            selectedItem.IsSelected = false;

        PersistAddedBusinessSoftware();
        _previousScreen = ViewScreen.SoftwareCatalog;
        SetCurrentScreen(ViewScreen.AddedSoftware);
        StatusMessage = UiText("Gui.BusinessSoftware.Status.Updated", "Business software list updated.");
        OnPropertyChanged(nameof(CanAddSelectedBusinessSoftware));
    }

    /// <summary>
    ///     Removes selected items from the configured blocker list.
    /// </summary>
    [RelayCommand]
    private void RemoveSelectedAddedBusinessSoftware()
    {
        var itemsToRemove = AddedBusinessSoftware.Where(item => item.IsSelected).ToList();
        if (itemsToRemove.Count == 0)
            return;

        foreach (var item in itemsToRemove)
        {
            item.SelectionChanged -= OnAddedSoftwareSelectionChanged;
            AddedBusinessSoftware.Remove(item);
        }

        PersistAddedBusinessSoftware();
        StatusMessage = UiText("Gui.BusinessSoftware.Status.Removed",
            "Business software removed from blocker list.");
        OnPropertyChanged(nameof(CanRemoveAddedBusinessSoftware));
    }

    partial void OnBusinessSoftwareSearchTextChanged(string value)
    {
        ApplyBusinessSoftwareFilter();
    }

    partial void OnCurrentScreenChanged(ViewScreen value)
    {
        OnPropertyChanged(nameof(IsMainScreen));
        OnPropertyChanged(nameof(IsSoftwareCatalogScreen));
        OnPropertyChanged(nameof(IsAddedSoftwareScreen));
    }

    /// <summary>
    ///     Loads available software from the catalog service.
    /// </summary>
    private void LoadAvailableBusinessSoftware()
    {
        foreach (var item in AllAvailableBusinessSoftware)
            item.SelectionChanged -= OnAvailableSoftwareSelectionChanged;

        var items = _businessSoftwareCatalogService.GetAvailableSoftware()
            .Select(item => new SelectableBusinessSoftwareItemViewModel(item.DisplayName, item.ProcessName))
            .ToList();

        foreach (var item in items)
            item.SelectionChanged += OnAvailableSoftwareSelectionChanged;

        AllAvailableBusinessSoftware = new ObservableCollection<SelectableBusinessSoftwareItemViewModel>(items);
        ApplyBusinessSoftwareFilter();
    }

    /// <summary>
    ///     Loads configured software blockers from persisted settings.
    /// </summary>
    private void LoadAddedBusinessSoftware()
    {
        foreach (var item in AddedBusinessSoftware)
            item.SelectionChanged -= OnAddedSoftwareSelectionChanged;

        var availableByProcessName = AllAvailableBusinessSoftware
            .GroupBy(item => item.ProcessName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var configuredItems = _businessSoftwareSettingsService.LoadConfiguredProcessNames()
            .Select(processName =>
            {
                if (availableByProcessName.TryGetValue(processName, out var availableItem))
                    return new SelectableBusinessSoftwareItemViewModel(availableItem.DisplayName, availableItem.ProcessName);

                return new SelectableBusinessSoftwareItemViewModel(processName, processName);
            })
            .ToList();

        foreach (var configuredItem in configuredItems)
            configuredItem.SelectionChanged += OnAddedSoftwareSelectionChanged;

        AddedBusinessSoftware = new ObservableCollection<SelectableBusinessSoftwareItemViewModel>(configuredItems);
        OnPropertyChanged(nameof(CanRemoveAddedBusinessSoftware));
    }

    /// <summary>
    ///     Applies search filtering on available software list.
    /// </summary>
    private void ApplyBusinessSoftwareFilter()
    {
        var query = BusinessSoftwareSearchText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query))
        {
            FilteredBusinessSoftware =
                new ObservableCollection<SelectableBusinessSoftwareItemViewModel>(AllAvailableBusinessSoftware);
        }
        else
        {
            var filtered = AllAvailableBusinessSoftware
                .Where(item =>
                    item.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    item.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            FilteredBusinessSoftware = new ObservableCollection<SelectableBusinessSoftwareItemViewModel>(filtered);
        }

        OnPropertyChanged(nameof(CanAddSelectedBusinessSoftware));
    }

    /// <summary>
    ///     Persists configured software blockers and refreshes runtime monitors used by existing jobs.
    /// </summary>
    private void PersistAddedBusinessSoftware()
    {
        _businessSoftwareSettingsService.SaveConfiguredProcessNames(
            AddedBusinessSoftware.Select(item => item.ProcessName));

        foreach (var job in Jobs)
            job.Job.BusinessSoftwareMonitor = new BusinessSoftwareMonitor();
    }

    /// <summary>
    ///     Sets the current screen for in-window navigation.
    /// </summary>
    /// <param name="screen">Target screen.</param>
    private void SetCurrentScreen(ViewScreen screen)
    {
        CurrentScreen = screen;
    }

    /// <summary>
    ///     Handles selection updates from available software list.
    /// </summary>
    private void OnAvailableSoftwareSelectionChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CanAddSelectedBusinessSoftware));
    }

    /// <summary>
    ///     Handles selection updates from added software list.
    /// </summary>
    private void OnAddedSoftwareSelectionChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CanRemoveAddedBusinessSoftware));
    }

    public enum ViewScreen
    {
        Main,
        SoftwareCatalog,
        AddedSoftware
    }
}
