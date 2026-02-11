using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using EasySave.Application.Models;
using EasySave.Bootstrap;
using EasySave.Domain.Models;

namespace EasySave.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly AppRuntime _runtime;
    private string _businessProcessName = string.Empty;
    private string _centralLogEndpoint = string.Empty;
    private string _cryptoExtensions = string.Empty;
    private string _cryptoSoftArguments = "\"{0}\"";
    private string _cryptoSoftPath = string.Empty;
    private bool _enableBusinessProcessMonitor = true;
    private string _largeFileThresholdKb = "2048";
    private string _newBackupName = string.Empty;
    private string _newSourceDirectory = string.Empty;
    private string _newTargetDirectory = string.Empty;
    private string _newType = nameof(BackupType.Complete);
    private string _priorityExtensions = string.Empty;
    private JobItemViewModel? _selectedJob;
    private string _selectedLanguage = "en-US";
    private string _selectedLogMode = "local";
    private string _selectedLogType = "json";
    private string _statusMessage = string.Empty;

    public MainWindowViewModel(AppRuntime runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        Jobs = [];

        ReloadJobsCommand = new RelayCommand(ReloadJobs);
        AddJobCommand = new RelayCommand(AddJob);
        RemoveSelectedJobCommand = new RelayCommand(RemoveSelectedJob);
        RunSelectedCommand = new AsyncRelayCommand(RunSelectedAsync);
        RunAllCommand = new AsyncRelayCommand(RunAllAsync);
        PauseSelectedCommand = new RelayCommand(PauseSelected);
        ResumeSelectedCommand = new RelayCommand(ResumeSelected);
        StopSelectedCommand = new RelayCommand(StopSelected);
        PauseAllCommand = new RelayCommand(_runtime.BackupRuntime.PauseAll);
        ResumeAllCommand = new RelayCommand(_runtime.BackupRuntime.ResumeAll);
        StopAllCommand = new RelayCommand(_runtime.BackupRuntime.StopAll);
        SaveSettingsCommand = new RelayCommand(SaveSettings);

        var appSettings = _runtime.GeneralSettings.Current;
        PriorityExtensions = string.Join(";", appSettings.PriorityExtensions);
        CryptoExtensions = string.Join(";", appSettings.CryptoExtensions);
        LargeFileThresholdKb = appSettings.LargeFileThresholdKb.ToString();
        BusinessProcessName = appSettings.BusinessProcessName;
        EnableBusinessProcessMonitor = appSettings.EnableBusinessProcessMonitor;
        CryptoSoftPath = appSettings.CryptoSoftPath;
        CryptoSoftArguments = appSettings.CryptoSoftArguments;
        SelectedLogMode = appSettings.LogMode;
        CentralLogEndpoint = appSettings.CentralLogEndpoint;
        SelectedLanguage = _runtime.Preferences.Localization;
        SelectedLogType = _runtime.Preferences.LogType;

        _runtime.BackupRuntime.JobStateChanged += OnJobStateChanged;
        ReloadJobs();
    }

    public ObservableCollection<JobItemViewModel> Jobs { get; }

    public IReadOnlyList<string> BackupTypeOptions { get; } = [nameof(BackupType.Complete), nameof(BackupType.Differential)];
    public IReadOnlyList<string> LogTypeOptions { get; } = ["json", "xml"];
    public IReadOnlyList<string> LanguageOptions { get; } = ["en-US", "fr-FR"];
    public IReadOnlyList<string> LogModeOptions { get; } = ["local", "centralized", "both"];

    public JobItemViewModel? SelectedJob
    {
        get => _selectedJob;
        set => SetProperty(ref _selectedJob, value);
    }

    public string NewBackupName
    {
        get => _newBackupName;
        set => SetProperty(ref _newBackupName, value);
    }

    public string NewSourceDirectory
    {
        get => _newSourceDirectory;
        set => SetProperty(ref _newSourceDirectory, value);
    }

    public string NewTargetDirectory
    {
        get => _newTargetDirectory;
        set => SetProperty(ref _newTargetDirectory, value);
    }

    public string NewType
    {
        get => _newType;
        set => SetProperty(ref _newType, value);
    }

    public string PriorityExtensions
    {
        get => _priorityExtensions;
        set => SetProperty(ref _priorityExtensions, value);
    }

    public string CryptoExtensions
    {
        get => _cryptoExtensions;
        set => SetProperty(ref _cryptoExtensions, value);
    }

    public string LargeFileThresholdKb
    {
        get => _largeFileThresholdKb;
        set => SetProperty(ref _largeFileThresholdKb, value);
    }

    public string BusinessProcessName
    {
        get => _businessProcessName;
        set => SetProperty(ref _businessProcessName, value);
    }

    public bool EnableBusinessProcessMonitor
    {
        get => _enableBusinessProcessMonitor;
        set => SetProperty(ref _enableBusinessProcessMonitor, value);
    }

    public string CryptoSoftPath
    {
        get => _cryptoSoftPath;
        set => SetProperty(ref _cryptoSoftPath, value);
    }

    public string CryptoSoftArguments
    {
        get => _cryptoSoftArguments;
        set => SetProperty(ref _cryptoSoftArguments, value);
    }

    public string SelectedLogMode
    {
        get => _selectedLogMode;
        set => SetProperty(ref _selectedLogMode, value);
    }

    public string CentralLogEndpoint
    {
        get => _centralLogEndpoint;
        set => SetProperty(ref _centralLogEndpoint, value);
    }

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public string SelectedLogType
    {
        get => _selectedLogType;
        set => SetProperty(ref _selectedLogType, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public IRelayCommand ReloadJobsCommand { get; }
    public IRelayCommand AddJobCommand { get; }
    public IRelayCommand RemoveSelectedJobCommand { get; }
    public IAsyncRelayCommand RunSelectedCommand { get; }
    public IAsyncRelayCommand RunAllCommand { get; }
    public IRelayCommand PauseSelectedCommand { get; }
    public IRelayCommand ResumeSelectedCommand { get; }
    public IRelayCommand StopSelectedCommand { get; }
    public IRelayCommand PauseAllCommand { get; }
    public IRelayCommand ResumeAllCommand { get; }
    public IRelayCommand StopAllCommand { get; }
    public IRelayCommand SaveSettingsCommand { get; }

    private async Task RunSelectedAsync()
    {
        if (SelectedJob == null)
        {
            StatusMessage = "Select a job before running.";
            return;
        }

        try
        {
            StatusMessage = $"Running job {SelectedJob.Id}...";
            await _runtime.BackupRuntime.RunJobAsync(SelectedJob.ToDomainJob()).ConfigureAwait(false);
            StatusMessage = $"Job {SelectedJob.Id} finished.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Run failed: {ex.Message}";
        }
    }

    private async Task RunAllAsync()
    {
        var jobs = Jobs.Select(x => x.ToDomainJob()).ToList();
        if (jobs.Count == 0)
        {
            StatusMessage = "No configured backup jobs.";
            return;
        }

        try
        {
            StatusMessage = "Running all jobs in parallel...";
            await _runtime.BackupRuntime.RunJobsParallelAsync(jobs).ConfigureAwait(false);
            StatusMessage = "All jobs finished.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Parallel run failed: {ex.Message}";
        }
    }

    private void PauseSelected()
    {
        if (SelectedJob == null)
            return;

        _runtime.BackupRuntime.PauseJob(SelectedJob.Id);
        StatusMessage = $"Pause requested for job {SelectedJob.Id}.";
    }

    private void ResumeSelected()
    {
        if (SelectedJob == null)
            return;

        _runtime.BackupRuntime.ResumeJob(SelectedJob.Id);
        StatusMessage = $"Resume requested for job {SelectedJob.Id}.";
    }

    private void StopSelected()
    {
        if (SelectedJob == null)
            return;

        _runtime.BackupRuntime.StopJob(SelectedJob.Id);
        StatusMessage = $"Stop requested for job {SelectedJob.Id}.";
    }

    private void ReloadJobs()
    {
        var configured = _runtime.JobService.GetAll().OrderBy(j => j.Id).ToList();

        Jobs.Clear();
        foreach (var job in configured)
        {
            var vm = new JobItemViewModel(job);
            var state = _runtime.StateService.GetOrCreate(job);
            vm.ApplyState(state);
            Jobs.Add(vm);
        }

        SelectedJob = Jobs.FirstOrDefault();
        StatusMessage = $"Loaded {Jobs.Count} job(s).";
    }

    private void AddJob()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewBackupName) ||
                string.IsNullOrWhiteSpace(NewSourceDirectory) ||
                string.IsNullOrWhiteSpace(NewTargetDirectory))
            {
                StatusMessage = "Name, source and target are required.";
                return;
            }

            if (!Enum.TryParse<BackupType>(NewType, ignoreCase: true, out var backupType))
                backupType = BackupType.Complete;

            var job = new BackupJob(NewBackupName.Trim(), NewSourceDirectory.Trim(), NewTargetDirectory.Trim(), backupType);
            var validation = _runtime.Validator.Validate(job);
            if (!validation.IsValid)
            {
                StatusMessage = validation.Error switch
                {
                    JobValidationError.SourceMissing => "Source directory does not exist.",
                    JobValidationError.TargetMissing => "Target directory does not exist.",
                    _ => "Invalid job configuration."
                };
                return;
            }

            var (ok, error) = _runtime.JobService.AddJob(job);
            if (!ok)
            {
                StatusMessage = string.IsNullOrWhiteSpace(error) ? "Unable to add job." : error;
                return;
            }

            _runtime.StateSynchronizer.Refresh();
            NewBackupName = string.Empty;
            ReloadJobs();
            StatusMessage = "Job added.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Add failed: {ex.Message}";
        }
    }

    private void RemoveSelectedJob()
    {
        if (SelectedJob == null)
        {
            StatusMessage = "Select a job to remove.";
            return;
        }

        if (_runtime.JobService.RemoveJob(SelectedJob.Id.ToString()))
        {
            _runtime.StateSynchronizer.Refresh();
            ReloadJobs();
            StatusMessage = $"Job {SelectedJob.Id} removed.";
            return;
        }

        StatusMessage = "Unable to remove selected job.";
    }

    private void SaveSettings()
    {
        try
        {
            if (!long.TryParse(LargeFileThresholdKb, out var thresholdKb))
                thresholdKb = 2048;

            var settings = new GeneralSettings
            {
                PriorityExtensions = ParseExtensions(PriorityExtensions),
                CryptoExtensions = ParseExtensions(CryptoExtensions),
                LargeFileThresholdKb = Math.Max(1, thresholdKb),
                BusinessProcessName = BusinessProcessName.Trim(),
                EnableBusinessProcessMonitor = EnableBusinessProcessMonitor,
                BusinessProcessCheckIntervalMs = _runtime.GeneralSettings.Current.BusinessProcessCheckIntervalMs,
                CryptoSoftPath = CryptoSoftPath.Trim(),
                CryptoSoftArguments = string.IsNullOrWhiteSpace(CryptoSoftArguments) ? "\"{0}\"" : CryptoSoftArguments.Trim(),
                LogMode = SelectedLogMode.Trim(),
                CentralLogEndpoint = CentralLogEndpoint.Trim()
            };

            _runtime.GeneralSettings.Save(settings);
            _runtime.Preferences.SetLocalization(SelectedLanguage);
            _runtime.Preferences.SetLogType(SelectedLogType);
            _runtime.Localization.Apply(SelectedLanguage);

            StatusMessage = "Settings saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Settings save failed: {ex.Message}";
        }
    }

    private static List<string> ParseExtensions(string raw)
    {
        return (raw ?? string.Empty)
            .Split([';', ',', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.StartsWith('.') ? x : "." + x)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void OnJobStateChanged(BackupJobState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var vm = Jobs.FirstOrDefault(x => x.Id == state.JobId);
            vm?.ApplyState(state);
        });
    }
}
