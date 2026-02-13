using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Models;
using EasySave.Data.Configuration;
using EasySave.Models.Backup;
using EasySave.Models.State;
using EasySave.Models.Utils;
using EasySave.Views.Resources;

namespace EasySave.ViewModels;

/// <summary>
///     Main window ViewModel for the EasySave backup manager application.
///     Handles all UI interactions, commands, and state management following MVVM pattern.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    #region Fields

    private readonly JobService _jobService;
    private readonly LocalizationApplier _localizationApplier;

    #endregion

    #region Observable Properties - UI State

    [ObservableProperty]
    private bool _isNotBusy = true;

    [ObservableProperty]
    private double _overallProgress;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    #endregion

    #region Observable Properties - Collections

    [ObservableProperty]
    private ObservableCollection<BackupJobItemViewModel> _jobs = [];

    [ObservableProperty]
    private BackupJobItemViewModel? _selectedJob;

    [ObservableProperty]
    private ObservableCollection<string> _backupTypes = [];

    [ObservableProperty]
    private string _selectedBackupType = string.Empty;

    #endregion

    #region Observable Properties - Input Fields

    [ObservableProperty]
    private string _newJobName = string.Empty;

    [ObservableProperty]
    private string _newSourceDirectory = string.Empty;

    [ObservableProperty]
    private string _newTargetDirectory = string.Empty;

    #endregion

    #region Observable Properties - Localized Labels

    [ObservableProperty]
    private string _windowTitle = string.Empty;

    [ObservableProperty]
    private string _currentSettingsLabel = string.Empty;

    [ObservableProperty]
    private string _jobsSectionTitle = string.Empty;

    [ObservableProperty]
    private string _addSectionTitle = string.Empty;

    [ObservableProperty]
    private string _nameLabel = string.Empty;

    [ObservableProperty]
    private string _sourceLabel = string.Empty;

    [ObservableProperty]
    private string _targetLabel = string.Empty;

    [ObservableProperty]
    private string _typeLabel = string.Empty;

    [ObservableProperty]
    private string _frenchButtonLabel = string.Empty;

    [ObservableProperty]
    private string _englishButtonLabel = string.Empty;

    [ObservableProperty]
    private string _jsonButtonLabel = string.Empty;

    [ObservableProperty]
    private string _xmlButtonLabel = string.Empty;

    [ObservableProperty]
    private string _addButtonLabel = string.Empty;

    [ObservableProperty]
    private string _removeButtonLabel = string.Empty;

    [ObservableProperty]
    private string _runSelectedButtonLabel = string.Empty;

    [ObservableProperty]
    private string _runAllButtonLabel = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    ///     Initializes a new instance of the MainWindowViewModel class.
    /// </summary>
    public MainWindowViewModel()
    {
        _jobService = new JobService();
        _localizationApplier = new LocalizationApplier();

        // Load configuration and apply localization
        var config = ApplicationConfiguration.Load();
        if (!string.IsNullOrEmpty(config.Localization))
        {
            _localizationApplier.Apply(config.Localization);
        }

        // Initialize backup types
        InitializeBackupTypes();

        // Load UI text resources
        UpdateUIText();

        // Load existing jobs
        RefreshJobs();

        // Set initial status
        StatusMessage = "Ready";
    }

    #endregion

    #region Initialization Methods

    /// <summary>
    ///     Initializes the backup types collection from the BackupType enum.
    /// </summary>
    private void InitializeBackupTypes()
    {
        BackupTypes = new ObservableCollection<string>(
            Enum.GetNames(typeof(BackupType))
        );
        
        if (BackupTypes.Count > 0)
        {
            SelectedBackupType = BackupTypes[0];
        }
    }

    /// <summary>
    ///     Updates all UI text from resource files based on current culture.
    /// </summary>
    private void UpdateUIText()
    {
        WindowTitle = "EasySave - Backup Manager";
        CurrentSettingsLabel = "Settings";
        JobsSectionTitle = UserInterface.Jobs_Header;
        AddSectionTitle = UserInterface.Add_Header;
        NameLabel = UserInterface.Add_PromptName;
        SourceLabel = UserInterface.Add_PromptSource;
        TargetLabel = UserInterface.Add_PromptTarget;
        TypeLabel = UserInterface.Add_PromptType;
        FrenchButtonLabel = "Français";
        EnglishButtonLabel = "English";
        JsonButtonLabel = "JSON";
        XmlButtonLabel = "XML";
        AddButtonLabel = "Add Job";
        RemoveButtonLabel = "Remove Selected";
        RunSelectedButtonLabel = "Run Selected Job";
        RunAllButtonLabel = "Run All Jobs";
    }

    #endregion

    #region Job Management Methods

    /// <summary>
    ///     Refreshes the jobs collection from the repository.
    /// </summary>
    private void RefreshJobs()
    {
        var jobModels = _jobService.GetAll();
        Jobs.Clear();
        
        foreach (var job in jobModels)
        {
            Jobs.Add(new BackupJobItemViewModel(job));
        }
    }

    #endregion

    #region Commands - Language Selection

    /// <summary>
    ///     Sets the application language to French.
    /// </summary>
    [RelayCommand]
    private void SetFrenchLanguage()
    {
        _localizationApplier.Apply("fr-FR");
        UpdateUIText();
        StatusMessage = "Langue changée en Français";
    }

    /// <summary>
    ///     Sets the application language to English.
    /// </summary>
    [RelayCommand]
    private void SetEnglishLanguage()
    {
        _localizationApplier.Apply("en-US");
        UpdateUIText();
        StatusMessage = "Language changed to English";
    }

    #endregion

    #region Commands - Log Type Selection

    /// <summary>
    ///     Sets the log format to JSON.
    /// </summary>
    [RelayCommand]
    private void SetJsonLogType()
    {
        // Log type configuration would be persisted here if supported
        StatusMessage = "Log type set to JSON";
    }

    /// <summary>
    ///     Sets the log format to XML.
    /// </summary>
    [RelayCommand]
    private void SetXmlLogType()
    {
        // Log type configuration would be persisted here if supported
        StatusMessage = "Log type set to XML";
    }

    #endregion

    #region Commands - Job Management

    /// <summary>
    ///     Adds a new backup job using the input fields.
    ///     Validates that all required fields are provided and that both source and target directories exist.
    ///     Note: Both directories must exist before creating a job. The target directory is not automatically
    ///     created to ensure the user has explicitly prepared the backup destination.
    /// </summary>
    [RelayCommand]
    private void AddJob()
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(NewJobName))
        {
            StatusMessage = "Error: Job name is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewSourceDirectory))
        {
            StatusMessage = "Error: Source directory is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewTargetDirectory))
        {
            StatusMessage = "Error: Target directory is required";
            return;
        }

        // Validate directory existence
        // Both source and target must exist before job creation
        // This ensures the backup destination is properly configured
        if (!Directory.Exists(NewSourceDirectory))
        {
            StatusMessage = "Error: Source directory does not exist";
            return;
        }

        if (!Directory.Exists(NewTargetDirectory))
        {
            StatusMessage = "Error: Target directory does not exist";
            return;
        }

        // Parse backup type
        if (!Enum.TryParse<BackupType>(SelectedBackupType, out var backupType))
        {
            StatusMessage = "Error: Invalid backup type";
            return;
        }

        // Create and add job
        var newJob = new BackupJob(NewJobName, NewSourceDirectory, NewTargetDirectory, backupType);
        var (ok, error) = _jobService.AddJob(newJob);

        if (ok)
        {
            StatusMessage = UserInterface.Add_Success;
            RefreshJobs();
            ClearInputFields();
        }
        else
        {
            // Map error key to localized message
            StatusMessage = error switch
            {
                "Error.NoFreeSlot" => UserInterface.Add_Error_NoFreeSlot,
                _ => $"{UserInterface.Add_Failed} {error}"
            };
        }
    }

    /// <summary>
    ///     Removes the currently selected backup job.
    /// </summary>
    [RelayCommand]
    private void RemoveSelectedJob()
    {
        if (SelectedJob == null)
        {
            StatusMessage = "Error: No job selected";
            return;
        }

        var jobId = SelectedJob.Job.Id.ToString();
        var removed = _jobService.RemoveJob(jobId);

        if (removed)
        {
            StatusMessage = $"Job '{SelectedJob.Job.Name}' removed successfully";
            RefreshJobs();
            SelectedJob = null;
        }
        else
        {
            StatusMessage = "Error: Failed to remove job";
        }
    }

    /// <summary>
    ///     Clears the input fields after successful job creation.
    /// </summary>
    private void ClearInputFields()
    {
        NewJobName = string.Empty;
        NewSourceDirectory = string.Empty;
        NewTargetDirectory = string.Empty;
    }

    #endregion

    #region Commands - Job Execution

    /// <summary>
    ///     Runs the selected backup job asynchronously.
    /// </summary>
    [RelayCommand]
    private async Task RunSelectedJob()
    {
        if (SelectedJob == null)
        {
            StatusMessage = "Error: No job selected";
            return;
        }

        await ExecuteJobAsync(SelectedJob.Job);
    }

    /// <summary>
    ///     Runs all backup jobs sequentially.
    ///     Note: Jobs are executed one at a time to avoid resource contention and ensure
    ///     predictable behavior. Parallel execution could cause conflicts if jobs access
    ///     the same file system resources.
    /// </summary>
    [RelayCommand]
    private async Task RunAllJobs()
    {
        if (Jobs.Count == 0)
        {
            StatusMessage = UserInterface.Jobs_None;
            return;
        }

        StatusMessage = UserInterface.Launch_RunningAll;
        IsNotBusy = false;
        OverallProgress = 0;

        try
        {
            var totalJobs = Jobs.Count;
            for (int i = 0; i < totalJobs; i++)
            {
                var jobViewModel = Jobs[i];
                StatusMessage = string.Format(UserInterface.Launch_RunningOne, 
                    jobViewModel.Job.Id, jobViewModel.Job.Name);

                // Execute jobs sequentially to prevent resource conflicts
                await Task.Run(() => jobViewModel.Job.StartBackup());

                // Update overall progress
                OverallProgress = ((i + 1) / (double)totalJobs) * 100;
            }

            StatusMessage = UserInterface.Launch_Done;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during execution: {ex.Message}";
        }
        finally
        {
            IsNotBusy = true;
            OverallProgress = 0;
        }
    }

    /// <summary>
    ///     Executes a single backup job and tracks its progress.
    /// </summary>
    /// <param name="job">The backup job to execute.</param>
    private async Task ExecuteJobAsync(BackupJob job)
    {
        IsNotBusy = false;
        OverallProgress = 0;
        StatusMessage = string.Format(UserInterface.Launch_RunningOne, job.Id, job.Name);

        try
        {
            // Run backup on background thread
            await Task.Run(() => job.StartBackup());

            // Update final status
            OverallProgress = 100;
            StatusMessage = UserInterface.Launch_Done;
        }
        catch (Exception ex)
        {
            // Include job context in error message for better debugging
            StatusMessage = $"Error executing job '{job.Name}' (ID: {job.Id}): {ex.Message}";
        }
        finally
        {
            IsNotBusy = true;
            
            // Reset progress after a delay
            await Task.Delay(2000);
            OverallProgress = 0;
        }
    }

    #endregion
}
