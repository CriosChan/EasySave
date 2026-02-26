using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Models;
using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.Utils;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Handles backup job edition workflow and persistence.
/// </summary>
public partial class EditBackupViewModel : ViewModelBase
{
    private readonly IJobService _jobService;
    private readonly StatusBarViewModel _statusBar;
    private readonly IUiTextService _uiTextService;
    private BackupJob? _originalJob;
    private IStorageProvider? _storageProvider;

    [ObservableProperty] private ObservableCollection<string> _backupTypes = [];
    [ObservableProperty] private bool _hasPendingChanges;
    [ObservableProperty] private int _jobId;
    [ObservableProperty] private string _jobName = string.Empty;
    [ObservableProperty] private string _selectedBackupType = string.Empty;
    [ObservableProperty] private string _sourceDirectory = string.Empty;
    [ObservableProperty] private string _targetDirectory = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EditBackupViewModel" /> class.
    /// </summary>
    /// <param name="statusBar">Shared status bar state.</param>
    /// <param name="uiTextService">Localized UI text service.</param>
    /// <param name="jobService">Backup job service. If null, default implementation is used.</param>
    public EditBackupViewModel(
        StatusBarViewModel statusBar,
        IUiTextService uiTextService,
        IJobService? jobService = null)
    {
        _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
        _uiTextService = uiTextService ?? throw new ArgumentNullException(nameof(uiTextService));
        _jobService = jobService ?? new JobService();

        InitializeBackupTypes();
    }

    /// <summary>
    ///     Raised when a backup job is successfully updated.
    /// </summary>
    public event Action<BackupJob>? JobUpdated;

    /// <summary>
    ///     Sets the storage provider used by folder picker dialogs.
    /// </summary>
    /// <param name="storageProvider">Storage provider from the main window.</param>
    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    /// <summary>
    ///     Loads a job into editable fields.
    /// </summary>
    /// <param name="job">Job to edit.</param>
    public void BeginEdit(BackupJob job)
    {
        _originalJob = job ?? throw new ArgumentNullException(nameof(job));

        JobId = job.Id;
        JobName = job.Name;
        SourceDirectory = job.SourceDirectory;
        TargetDirectory = job.TargetDirectory;
        SelectedBackupType = job.Type.ToString();
        RecalculatePendingChanges();
    }

    /// <summary>
    ///     Opens a folder picker to select source directory.
    /// </summary>
    [RelayCommand]
    private async Task BrowseSourceDirectory()
    {
        if (_storageProvider == null)
            return;

        var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = _uiTextService.Get("Gui.Dialog.SelectSourceDirectory", "Select Source Directory"),
            AllowMultiple = false
        });

        if (folders.Count > 0)
            SourceDirectory = folders[0].Path.LocalPath;
    }

    /// <summary>
    ///     Opens a folder picker to select target directory.
    /// </summary>
    [RelayCommand]
    private async Task BrowseTargetDirectory()
    {
        if (_storageProvider == null)
            return;

        var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = _uiTextService.Get("Gui.Dialog.SelectTargetDirectory", "Select Target Directory"),
            AllowMultiple = false
        });

        if (folders.Count > 0)
            TargetDirectory = folders[0].Path.LocalPath;
    }

    /// <summary>
    ///     Persists modifications when validation succeeds.
    /// </summary>
    [RelayCommand]
    private void SaveChanges()
    {
        if (_originalJob == null)
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.NoJobSelected",
                "Error: No job selected");
            return;
        }

        var normalizedName = (JobName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.JobNameRequired",
                "Error: Job name is required");
            return;
        }

        if (string.IsNullOrWhiteSpace(SourceDirectory))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.SourceRequired",
                "Error: Source directory is required");
            return;
        }

        if (!Directory.Exists(SourceDirectory))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Path.SourceNotFound",
                "Source directory does not exist. Please enter an existing directory.");
            return;
        }

        if (!PathService.IsDirectoryAccessible(SourceDirectory, out var sourceError))
        {
            _statusBar.StatusMessage =
                $"{_uiTextService.Get("Path.SourceNotAccessible", "Source directory is not accessible:")} {sourceError}";
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetDirectory))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.TargetRequired",
                "Error: Target directory is required");
            return;
        }

        if (!Directory.Exists(TargetDirectory))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Path.TargetNotFound",
                "Target directory does not exist. Please enter an existing directory.");
            return;
        }

        if (!PathService.IsDirectoryAccessible(TargetDirectory, out var targetError))
        {
            _statusBar.StatusMessage =
                $"{_uiTextService.Get("Path.TargetNotAccessible", "Target directory is not accessible:")} {targetError}";
            return;
        }

        if (!Enum.TryParse<BackupType>(SelectedBackupType, out var backupType))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.InvalidBackupType",
                "Error: Invalid backup type");
            return;
        }

        var updatedJob = new BackupJob(_originalJob.Id, normalizedName, SourceDirectory, TargetDirectory, backupType);
        if (!_jobService.UpdateJob(updatedJob))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.UpdateFailed",
                "Error: Failed to update job");
            return;
        }

        _originalJob = updatedJob;
        JobName = updatedJob.Name;
        SourceDirectory = updatedJob.SourceDirectory;
        TargetDirectory = updatedJob.TargetDirectory;
        SelectedBackupType = updatedJob.Type.ToString();
        HasPendingChanges = false;

        _statusBar.StatusMessage = _uiTextService.Format("Gui.Status.JobUpdated",
            "Job '{0}' updated successfully", updatedJob.Name);
        JobUpdated?.Invoke(updatedJob);
    }

    partial void OnJobNameChanged(string value)
    {
        RecalculatePendingChanges();
    }

    partial void OnSourceDirectoryChanged(string value)
    {
        RecalculatePendingChanges();
    }

    partial void OnTargetDirectoryChanged(string value)
    {
        RecalculatePendingChanges();
    }

    partial void OnSelectedBackupTypeChanged(string value)
    {
        RecalculatePendingChanges();
    }

    /// <summary>
    ///     Initializes available backup type values.
    /// </summary>
    private void InitializeBackupTypes()
    {
        BackupTypes = new ObservableCollection<string>(Enum.GetNames(typeof(BackupType)));
        if (BackupTypes.Count > 0 && string.IsNullOrWhiteSpace(SelectedBackupType))
            SelectedBackupType = BackupTypes[0];
    }

    /// <summary>
    ///     Recomputes dirty state against the originally loaded job.
    /// </summary>
    private void RecalculatePendingChanges()
    {
        if (_originalJob == null)
        {
            HasPendingChanges = false;
            return;
        }

        HasPendingChanges =
            !string.Equals(JobName, _originalJob.Name, StringComparison.Ordinal) ||
            !string.Equals(SourceDirectory, _originalJob.SourceDirectory, StringComparison.Ordinal) ||
            !string.Equals(TargetDirectory, _originalJob.TargetDirectory, StringComparison.Ordinal) ||
            !string.Equals(SelectedBackupType, _originalJob.Type.ToString(), StringComparison.Ordinal);
    }
}
