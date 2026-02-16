using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Models;
using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.Utils;
using EasySave.ViewModels.Services;

namespace EasySave.ViewModels;

/// <summary>
///     Handles backup job creation, removal, execution and related UI state.
/// </summary>
public partial class JobsViewModel : ViewModelBase
{
    private readonly IJobService _jobService;
    private readonly IUiTextService _uiTextService;
    private readonly StatusBarViewModel _statusBar;
    private IStorageProvider? _storageProvider;

    [ObservableProperty] private ObservableCollection<BackupJobItemViewModel> _jobs = [];
    [ObservableProperty] private BackupJobItemViewModel? _selectedJob;
    [ObservableProperty] private ObservableCollection<string> _backupTypes = [];
    [ObservableProperty] private string _selectedBackupType = string.Empty;

    [ObservableProperty] private string _newJobName = string.Empty;
    [ObservableProperty] private string _newSourceDirectory = string.Empty;
    [ObservableProperty] private string _newTargetDirectory = string.Empty;

    [ObservableProperty] private string _jobsSectionTitle = string.Empty;
    [ObservableProperty] private string _addSectionTitle = string.Empty;
    [ObservableProperty] private string _nameLabel = string.Empty;
    [ObservableProperty] private string _sourceLabel = string.Empty;
    [ObservableProperty] private string _targetLabel = string.Empty;
    [ObservableProperty] private string _typeLabel = string.Empty;
    [ObservableProperty] private string _browseSourceLabel = string.Empty;
    [ObservableProperty] private string _browseTargetLabel = string.Empty;
    [ObservableProperty] private string _addButtonLabel = string.Empty;
    [ObservableProperty] private string _removeButtonLabel = string.Empty;
    [ObservableProperty] private string _runSelectedButtonLabel = string.Empty;
    [ObservableProperty] private string _runAllButtonLabel = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JobsViewModel" /> class.
    /// </summary>
    /// <param name="jobService">Backup job service.</param>
    /// <param name="uiTextService">Localized text service.</param>
    /// <param name="statusBar">Shared status bar state.</param>
    public JobsViewModel(StatusBarViewModel statusBar)
    {
        _jobService = new JobService();
        _uiTextService = new ResxUiTextService();
        _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));

        InitializeBackupTypes();
        RefreshJobs();
        UpdateUiText();
    }

    /// <summary>
    ///     Sets the storage provider used by folder picker dialogs.
    /// </summary>
    /// <param name="storageProvider">Storage provider from the main window.</param>
    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    /// <summary>
    ///     Updates localized labels used by the jobs area.
    /// </summary>
    public void UpdateUiText()
    {
        JobsSectionTitle = _uiTextService.Get("Jobs.Header", "Configured backup jobs");
        AddSectionTitle = _uiTextService.Get("Add.Header", "Add a backup job");
        NameLabel = _uiTextService.Get("Add.PromptName", "Backup name:");
        SourceLabel = _uiTextService.Get("Add.PromptSource", "Source directory:");
        TargetLabel = _uiTextService.Get("Add.PromptTarget", "Target directory:");
        TypeLabel = _uiTextService.Get("Add.PromptType", "Backup type:");
        BrowseSourceLabel = _uiTextService.Get("Gui.Button.BrowseSource", "Browse source...");
        BrowseTargetLabel = _uiTextService.Get("Gui.Button.BrowseTarget", "Browse target...");
        AddButtonLabel = _uiTextService.Get("Gui.Button.AddJob", "Add Job");
        RemoveButtonLabel = _uiTextService.Get("Gui.Button.RemoveSelected", "Remove Selected");
        RunSelectedButtonLabel = _uiTextService.Get("Gui.Button.RunSelectedJob", "Run Selected Job");
        RunAllButtonLabel = _uiTextService.Get("Gui.Button.RunAllJobs", "Run All Jobs");
    }

    /// <summary>
    ///     Reloads configured jobs from persistence.
    /// </summary>
    public void RefreshJobs()
    {
        var jobModels = _jobService.GetAll();
        Jobs.Clear();

        foreach (var job in jobModels)
            Jobs.Add(new BackupJobItemViewModel(job));
    }

    /// <summary>
    ///     Recreates business software monitors for existing loaded jobs.
    /// </summary>
    public void RefreshBusinessSoftwareMonitors()
    {
        foreach (var job in Jobs)
            job.Job.BusinessSoftwareMonitor = new BusinessSoftwareMonitor();
    }

    /// <summary>
    ///     Adds a backup job using current input fields.
    /// </summary>
    [RelayCommand]
    private void AddJob()
    {
        if (string.IsNullOrWhiteSpace(NewJobName))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.JobNameRequired", "Error: Job name is required");
            return;
        }

        if (string.IsNullOrWhiteSpace(NewSourceDirectory))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.SourceRequired", "Error: Source directory is required");
            return;
        }

        if (string.IsNullOrWhiteSpace(NewTargetDirectory))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.TargetRequired", "Error: Target directory is required");
            return;
        }

        if (!Directory.Exists(NewSourceDirectory))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Path.SourceNotFound",
                "Source directory does not exist. Please enter an existing directory.");
            return;
        }

        if (!PathService.IsDirectoryAccessible(NewSourceDirectory, out var sourceError))
        {
            _statusBar.StatusMessage = $"{_uiTextService.Get("Path.SourceNotAccessible", "Source directory is not accessible:")} {sourceError}";
            return;
        }

        if (!Directory.Exists(NewTargetDirectory))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Path.TargetNotFound",
                "Target directory does not exist. Please enter an existing directory.");
            return;
        }

        if (!PathService.IsDirectoryAccessible(NewTargetDirectory, out var targetError))
        {
            _statusBar.StatusMessage = $"{_uiTextService.Get("Path.TargetNotAccessible", "Target directory is not accessible:")} {targetError}";
            return;
        }

        if (!Enum.TryParse<BackupType>(SelectedBackupType, out var backupType))
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.InvalidBackupType", "Error: Invalid backup type");
            return;
        }

        var newJob = new BackupJob(NewJobName, NewSourceDirectory, NewTargetDirectory, backupType);
        var (ok, error) = _jobService.AddJob(newJob);

        if (!ok)
        {
            _statusBar.StatusMessage = error switch
            {
                "Error.NoFreeSlot" => _uiTextService.Get("Add.Error.NoFreeSlot", "No free slot available (1..5)."),
                _ => $"{_uiTextService.Get("Add.Failed", "Unable to create the job:")} {error}"
            };
            return;
        }

        _statusBar.StatusMessage = _uiTextService.Get("Add.Success", "Backup job created.");
        RefreshJobs();
        ClearInputFields();
    }

    /// <summary>
    ///     Removes the selected backup job.
    /// </summary>
    [RelayCommand]
    private void RemoveSelectedJob()
    {
        if (SelectedJob == null)
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.NoJobSelected", "Error: No job selected");
            return;
        }

        var selectedName = SelectedJob.Job.Name;
        var selectedId = SelectedJob.Job.Id.ToString();
        var removed = _jobService.RemoveJob(selectedId);
        if (!removed)
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.RemoveFailed", "Error: Failed to remove job");
            return;
        }

        _statusBar.StatusMessage = string.Format(
            _uiTextService.Get("Gui.Status.JobRemoved", "Job '{0}' removed successfully"),
            selectedName);
        SelectedJob = null;
        RefreshJobs();
    }

    /// <summary>
    ///     Executes the selected backup job.
    /// </summary>
    [RelayCommand]
    private async Task RunSelectedJob()
    {
        if (SelectedJob == null)
        {
            _statusBar.StatusMessage = _uiTextService.Get("Gui.Error.NoJobSelected", "Error: No job selected");
            return;
        }

        _statusBar.IsNotBusy = false;
        _statusBar.OverallProgress = 0;

        try
        {
            var stoppedByBusinessSoftware = await ExecuteJobCoreAsync(SelectedJob.Job);
            if (!stoppedByBusinessSoftware)
            {
                _statusBar.OverallProgress = 100;
                _statusBar.StatusMessage = _uiTextService.Get("Launch.Done", "Execution finished.");
            }
        }
        catch (Exception ex)
        {
            _statusBar.StatusMessage =
                $"Error executing job '{SelectedJob.Job.Name}' (ID: {SelectedJob.Job.Id}): {ex.Message}";
        }
        finally
        {
            _statusBar.IsNotBusy = true;
            await Task.Delay(2000);
            _statusBar.OverallProgress = 0;
        }
    }

    /// <summary>
    ///     Executes all backup jobs sequentially.
    /// </summary>
    [RelayCommand]
    private async Task RunAllJobs()
    {
        if (Jobs.Count == 0)
        {
            _statusBar.StatusMessage = _uiTextService.Get("Jobs.None", "No backup job is configured.");
            return;
        }

        _statusBar.StatusMessage = _uiTextService.Get("Launch.RunningAll", "Running all jobs...");
        _statusBar.IsNotBusy = false;
        _statusBar.OverallProgress = 0;

        try
        {
            var totalJobs = Jobs.Count;
            var stoppedByBusinessSoftware = false;

            for (var i = 0; i < totalJobs; i++)
            {
                var job = Jobs[i].Job;
                stoppedByBusinessSoftware = await ExecuteJobCoreAsync(job);
                if (stoppedByBusinessSoftware)
                    break;

                _statusBar.OverallProgress = ((i + 1) / (double)totalJobs) * 100;
            }

            if (!stoppedByBusinessSoftware)
                _statusBar.StatusMessage = _uiTextService.Get("Launch.Done", "Execution finished.");
        }
        catch (Exception ex)
        {
            _statusBar.StatusMessage = $"Error during execution: {ex.Message}";
        }
        finally
        {
            _statusBar.IsNotBusy = true;
            _statusBar.OverallProgress = 0;
        }
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
            NewSourceDirectory = folders[0].Path.LocalPath;
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
            NewTargetDirectory = folders[0].Path.LocalPath;
    }

    /// <summary>
    ///     Executes one backup job and handles progress notifications.
    /// </summary>
    /// <param name="job">Job to execute.</param>
    /// <returns>True when execution was stopped by business software.</returns>
    private async Task<bool> ExecuteJobCoreAsync(BackupJob job)
    {
        // Verify that directories are accessible before starting
        if (!PathService.IsDirectoryAccessible(job.SourceDirectory, out var sourceError))
        {
            _statusBar.StatusMessage = $"{_uiTextService.Get("Gui.Error.SourceNotAccessible", "Error: Source directory is not accessible")} (Job {job.Id}): {sourceError}";
            Console.WriteLine($"[ERROR] Job {job.Id} - {job.Name}: Source directory error - {sourceError}");
            throw new Exception($"Source directory error - {sourceError}");
        }

        if (!PathService.IsDirectoryAccessible(job.TargetDirectory, out var targetError))
        {
            _statusBar.StatusMessage = $"{_uiTextService.Get("Gui.Error.TargetNotAccessible", "Error: Target directory is not accessible")} (Job {job.Id}): {targetError}";
            Console.WriteLine($"[ERROR] Job {job.Id} - {job.Name}: Target directory error - {targetError}");
            throw new Exception($"Target directory error - {targetError}");
        }

        _statusBar.StatusMessage = _uiTextService.Format("Launch.RunningOne", "Running job {0} - {1}...", job.Id,
            job.Name);

        EventHandler handler = OnJobProgressChanged;
        job.ProgressChanged += handler;

        try
        {
            await Task.Run(job.StartBackup);

            if (!job.WasStoppedByBusinessSoftware)
                return false;

            _statusBar.StatusMessage = _uiTextService.Format("Gui.Status.BackupStoppedByBusinessSoftware",
                "Backup '{0}' stopped: business software is running", job.Name);
            return true;
        }
        finally
        {
            job.ProgressChanged -= handler;
        }
    }

    /// <summary>
    ///     Handles per-file progress updates and marshals UI updates to the UI thread.
    /// </summary>
    /// <param name="sender">Backup job sender.</param>
    /// <param name="e">Event args.</param>
    private void OnJobProgressChanged(object? sender, EventArgs e)
    {
        if (sender is not BackupJob job)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            _statusBar.StatusMessage =
                $"{_uiTextService.Format("Launch.RunningOne", "Running job {0} - {1}...", job.Id, job.Name)} " +
                $"({job.CurrentFileIndex} / {job.FilesCount} files) - " +
                $"({Math.Round(job.TransferredSize / 1048576.0)} / {Math.Round(job.TotalSize / 1048576.0)} MB)";

            _statusBar.OverallProgress = job.CurrentProgress;
        });
    }

    /// <summary>
    ///     Initializes available backup type values.
    /// </summary>
    private void InitializeBackupTypes()
    {
        BackupTypes = new ObservableCollection<string>(Enum.GetNames(typeof(BackupType)));
        if (BackupTypes.Count > 0)
            SelectedBackupType = BackupTypes[0];
    }

    /// <summary>
    ///     Clears job creation input fields.
    /// </summary>
    private void ClearInputFields()
    {
        NewJobName = string.Empty;
        NewSourceDirectory = string.Empty;
        NewTargetDirectory = string.Empty;
    }
}
