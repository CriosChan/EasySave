using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Data.Configuration;
using EasySave.Models.BusinessSoftware;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Handles business software catalog/search/add/remove workflow.
/// </summary>
public partial class BusinessSoftwareViewModel : ViewModelBase
{
    private readonly IBusinessSoftwareCatalogService _businessSoftwareCatalogService;
    private readonly IUiTextService _uiTextService;
    private readonly StatusBarViewModel _statusBar;

    [ObservableProperty] private ObservableCollection<SelectableBusinessSoftwareItemViewModel> _allAvailableBusinessSoftware = [];
    [ObservableProperty] private ObservableCollection<SelectableBusinessSoftwareItemViewModel> _filteredBusinessSoftware = [];
    [ObservableProperty] private ObservableCollection<SelectableBusinessSoftwareItemViewModel> _addedBusinessSoftware = [];
    [ObservableProperty] private string _businessSoftwareSearchText = string.Empty;

    [ObservableProperty] private string _availableBusinessSoftwareTitle = string.Empty;
    [ObservableProperty] private string _addedBusinessSoftwareTitle = string.Empty;
    [ObservableProperty] private string _businessSoftwareSearchLabel = string.Empty;
    [ObservableProperty] private string _businessSoftwareSearchPlaceholder = string.Empty;
    [ObservableProperty] private string _addSelectedBusinessSoftwareButtonLabel = string.Empty;
    [ObservableProperty] private string _viewAddedBusinessSoftwareButtonLabel = string.Empty;
    [ObservableProperty] private string _removeAddedBusinessSoftwareButtonLabel = string.Empty;

    /// <summary>
    ///     Raised when configured process names were persisted.
    /// </summary>
    public event EventHandler? ConfiguredProcessNamesChanged;

    /// <summary>
    ///     Raised when UI should navigate to the added software screen.
    /// </summary>
    public event EventHandler? OpenAddedSoftwareRequested;

    /// <summary>
    ///     Gets a value indicating whether at least one available software item is selected.
    /// </summary>
    public bool CanAddSelectedBusinessSoftware => AllAvailableBusinessSoftware.Any(item => item.IsSelected);

    /// <summary>
    ///     Gets a value indicating whether at least one configured software item is selected.
    /// </summary>
    public bool CanRemoveAddedBusinessSoftware => AddedBusinessSoftware.Any(item => item.IsSelected);

    /// <summary>
    ///     Initializes a new instance of the <see cref="BusinessSoftwareViewModel" /> class.
    /// </summary>
    /// <param name="statusBar">Shared status bar state.</param>
    public BusinessSoftwareViewModel(
        StatusBarViewModel statusBar)
    {
        _businessSoftwareCatalogService = new BusinessSoftwareCatalogService();
        _uiTextService = new ResxUiTextService();
        _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
    }

    /// <summary>
    ///     Initializes business software state used by UI.
    /// </summary>
    public void Initialize()
    {
        LoadAddedBusinessSoftware();
        ApplyBusinessSoftwareFilter();
        UpdateUiText();
    }

    /// <summary>
    ///     Updates business software labels from localization resources.
    /// </summary>
    public void UpdateUiText()
    {
        AvailableBusinessSoftwareTitle = _uiTextService.Get("Gui.BusinessSoftware.Catalog.Title", "Select Business Software");
        AddedBusinessSoftwareTitle = _uiTextService.Get("Gui.BusinessSoftware.Added.Title", "Added Business Software");
        BusinessSoftwareSearchLabel = _uiTextService.Get("Gui.BusinessSoftware.Search.Label", "Search software");
        BusinessSoftwareSearchPlaceholder =
            _uiTextService.Get("Gui.BusinessSoftware.Search.Placeholder", "Type to filter software");
        AddSelectedBusinessSoftwareButtonLabel = _uiTextService.Get("Gui.BusinessSoftware.Button.AddSelected", "Add Selected");
        ViewAddedBusinessSoftwareButtonLabel =
            _uiTextService.Get("Gui.BusinessSoftware.Button.ViewAdded", "View Added Software");
        RemoveAddedBusinessSoftwareButtonLabel =
            _uiTextService.Get("Gui.BusinessSoftware.Button.RemoveSelected", "Remove Selected");
    }

    /// <summary>
    ///     Prepares available and added lists when opening software catalog screen.
    /// </summary>
    public void OpenCatalog()
    {
        LoadAvailableBusinessSoftware();
        LoadAddedBusinessSoftware();
        BusinessSoftwareSearchText = string.Empty;
    }

    /// <summary>
    ///     Prepares configured list when opening added software screen.
    /// </summary>
    public void OpenAddedSoftware()
    {
        LoadAddedBusinessSoftware();
    }

    /// <summary>
    ///     Adds selected software candidates to the configured blocker list.
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
        _statusBar.StatusMessage = _uiTextService.Get("Gui.BusinessSoftware.Status.Updated", "Business software list updated.");
        OnPropertyChanged(nameof(CanAddSelectedBusinessSoftware));
        OpenAddedSoftwareRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Removes selected software from the configured blocker list.
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
        _statusBar.StatusMessage = _uiTextService.Get("Gui.BusinessSoftware.Status.Removed",
            "Business software removed from blocker list.");
        OnPropertyChanged(nameof(CanRemoveAddedBusinessSoftware));
    }

    partial void OnBusinessSoftwareSearchTextChanged(string value)
    {
        ApplyBusinessSoftwareFilter();
    }

    /// <summary>
    ///     Loads available software candidates.
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

        var configuredItems = ApplicationConfiguration.Load().BusinessSoftwareProcessNames
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
    ///     Applies current search text filter on available software list.
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
    ///     Persists configured blockers and notifies listeners.
    /// </summary>
    private void PersistAddedBusinessSoftware()
    {
        ApplicationConfiguration.Load().BusinessSoftwareProcessNames = AddedBusinessSoftware.Select(item => item.ProcessName).ToArray();

        ConfiguredProcessNamesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Handles available list selection updates.
    /// </summary>
    private void OnAvailableSoftwareSelectionChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CanAddSelectedBusinessSoftware));
    }

    /// <summary>
    ///     Handles configured list selection updates.
    /// </summary>
    private void OnAddedSoftwareSelectionChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CanRemoveAddedBusinessSoftware));
    }
}
