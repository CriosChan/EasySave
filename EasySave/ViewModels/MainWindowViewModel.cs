using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Contracts;
using EasySave.Core.Models;
using EasySave.Core.Validation;
using EasySave.Presentation.Resources;
using EasySave.Services;

namespace EasySave.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IBackupService _backupService;
    private readonly IJobService _jobService;
    private readonly IJobValidator _validator;
    private readonly ILocalizationApplier _localization;
    private readonly IPathService _paths;
    private readonly IUserPreferences _preferences;
    private readonly UiProgressReporter _progressReporter;
    private readonly IStateSynchronizer _stateSynchronizer;

    public MainWindowViewModel(
        IJobService jobService,
        IBackupService backupService,
        IStateSynchronizer stateSynchronizer,
        IJobValidator validator,
        IUserPreferences preferences,
        ILocalizationApplier localization,
        IPathService paths,
        UiProgressReporter? progressReporter)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _stateSynchronizer = stateSynchronizer ?? throw new ArgumentNullException(nameof(stateSynchronizer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));

        _progressReporter = progressReporter ?? new UiProgressReporter();
        _progressReporter.ProgressChanged += OnProgressChanged;

        RefreshJobs();
    }

    public ObservableCollection<BackupJobItemViewModel> Jobs { get; } = [];
    public IReadOnlyList<BackupType> BackupTypes { get; } = Enum.GetValues<BackupType>();

    [ObservableProperty] private string _newJobName = string.Empty;
    [ObservableProperty] private string _newSourceDirectory = string.Empty;
    [ObservableProperty] private string _newTargetDirectory = string.Empty;
    [ObservableProperty] private BackupType _selectedBackupType = BackupType.Complete;
    [ObservableProperty] private BackupJobItemViewModel? _selectedJob;
    [ObservableProperty] private double _overallProgress;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public string WindowTitle => "EasySave";
    public string JobsSectionTitle => UserInterface.Jobs_Header;
    public string NoJobsText => UserInterface.Jobs_None;

    public string AddSectionTitle => UserInterface.Add_Header;
    public string NameLabel => UserInterface.Add_PromptName;
    public string SourceLabel => UserInterface.Add_PromptSource;
    public string TargetLabel => UserInterface.Add_PromptTarget;
    public string TypeLabel => UserInterface.Add_PromptType;

    public string AddButtonLabel => UserInterface.Menu_AddBackup;
    public string RemoveButtonLabel => UserInterface.Menu_RemoveBackup;
    public string RefreshButtonLabel => UserInterface.Menu_ListJobs;
    public string RunSelectedButtonLabel => UserInterface.Menu_LaunchBackupJob;
    public string RunAllButtonLabel => UserInterface.Jobs_Execute_All;

    public string LanguageSectionTitle => UserInterface.Menu_Title_Lang;
    public string LogTypeSectionTitle => UserInterface.Menu_Title_LogType;

    public string FrenchButtonLabel => BuildSelectedLabel("Français", IsCurrentLanguage("fr-FR"));
    public string EnglishButtonLabel => BuildSelectedLabel("English", IsCurrentLanguage("en-US"));
    public string JsonButtonLabel => BuildSelectedLabel("JSON", IsCurrentLogType("json"));
    public string XmlButtonLabel => BuildSelectedLabel("XML", IsCurrentLogType("xml"));

    public string CurrentSettingsLabel => $"Language: {CurrentLanguageDisplay} | Log: {CurrentLogDisplay}";

    private string CurrentLanguageDisplay => IsCurrentLanguage("fr-FR") ? "fr-FR" : "en-US";
    private string CurrentLogDisplay => IsCurrentLogType("xml") ? "XML" : "JSON";

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    [RelayCommand]
    private void RefreshJobs()
    {
        var currentSelectionId = SelectedJob?.Id;
        var jobs = _jobService.GetAll()
            .OrderBy(j => j.Id)
            .Select(j => new BackupJobItemViewModel(j))
            .ToList();

        Jobs.Clear();
        foreach (var job in jobs)
            Jobs.Add(job);

        SelectedJob = currentSelectionId.HasValue
            ? Jobs.FirstOrDefault(j => j.Id == currentSelectionId.Value)
            : Jobs.FirstOrDefault();

        OnPropertyChanged(nameof(CurrentSettingsLabel));
    }

    [RelayCommand]
    private void AddJob()
    {
        if (IsBusy)
            return;

        var name = (NewJobName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus(UserInterface.Common_InvalidInput);
            return;
        }

        if (!_paths.TryNormalizeExistingDirectory(NewSourceDirectory, out var sourceDir))
        {
            SetStatus(UserInterface.Path_SourceNotFound);
            return;
        }

        if (!_paths.TryNormalizeExistingDirectory(NewTargetDirectory, out var targetDir))
        {
            SetStatus(UserInterface.Path_TargetNotFound);
            return;
        }

        BackupJob newJob;
        try
        {
            newJob = new BackupJob(name, sourceDir, targetDir, SelectedBackupType);
        }
        catch
        {
            SetStatus(UserInterface.Common_InvalidInput);
            return;
        }

        var (ok, error) = _jobService.AddJob(newJob);
        if (!ok)
        {
            SetStatus(TranslateAddError(error));
            return;
        }

        _stateSynchronizer.Refresh();
        RefreshJobs();

        NewJobName = string.Empty;
        NewSourceDirectory = string.Empty;
        NewTargetDirectory = string.Empty;
        SelectedBackupType = BackupType.Complete;

        SetStatus(UserInterface.Add_Success);
    }

    [RelayCommand]
    private void RemoveSelectedJob()
    {
        if (IsBusy)
            return;

        if (SelectedJob == null)
        {
            SetStatus(UserInterface.Remove_NotFound);
            return;
        }

        var removed = _jobService.RemoveJob(SelectedJob.Id.ToString());
        if (!removed)
        {
            SetStatus(UserInterface.Remove_NotFound);
            return;
        }

        _stateSynchronizer.Refresh();
        RefreshJobs();
        SetStatus(UserInterface.Remove_Success);
    }

    [RelayCommand]
    private async Task RunSelectedJobAsync()
    {
        if (SelectedJob == null)
        {
            SetStatus(UserInterface.Launch_NotFound);
            return;
        }

        var job = _jobService.GetAll().FirstOrDefault(j => j.Id == SelectedJob.Id);
        if (job == null)
        {
            SetStatus(UserInterface.Launch_NotFound);
            return;
        }

        await RunJobsAsync(
            [job],
            string.Format(UserInterface.Launch_RunningOne, job.Id, job.Name));
    }

    [RelayCommand]
    private async Task RunAllJobsAsync()
    {
        var jobs = _jobService.GetAll().OrderBy(j => j.Id).ToList();
        if (jobs.Count == 0)
        {
            SetStatus(UserInterface.Jobs_None);
            return;
        }

        await RunJobsAsync(jobs, UserInterface.Launch_RunningAll);
    }

    [RelayCommand]
    private void SetFrenchLanguage()
    {
        SetLanguage("fr-FR");
    }

    [RelayCommand]
    private void SetEnglishLanguage()
    {
        SetLanguage("en-US");
    }

    [RelayCommand]
    private void SetJsonLogType()
    {
        SetLogType("json");
    }

    [RelayCommand]
    private void SetXmlLogType()
    {
        SetLogType("xml");
    }

    private async Task RunJobsAsync(IReadOnlyList<BackupJob> jobs, string startMessage)
    {
        if (IsBusy)
            return;

        IsBusy = true;
        OverallProgress = 0;
        SetStatus(startMessage);

        var skipped = new List<string>();

        try
        {
            await Task.Run(() =>
            {
                foreach (var job in jobs.OrderBy(j => j.Id))
                {
                    if (!IsJobRunnable(job, out var reason))
                    {
                        skipped.Add($"[{job.Id}] {reason}");
                        continue;
                    }

                    _backupService.RunJob(job);
                }
            });

            var completionMessage = skipped.Count == 0
                ? UserInterface.Launch_Done
                : $"{UserInterface.Launch_Done} {string.Join(" | ", skipped)}";

            SetStatus(completionMessage);
        }
        finally
        {
            IsBusy = false;
            RefreshJobs();
        }
    }

    private bool IsJobRunnable(BackupJob job, out string message)
    {
        var validation = _validator.Validate(job);
        if (validation.IsValid)
        {
            message = string.Empty;
            return true;
        }

        message = validation.Error switch
        {
            JobValidationError.SourceMissing => UserInterface.Path_SourceNotFound,
            JobValidationError.TargetMissing => UserInterface.Path_TargetNotFound,
            _ => UserInterface.Path_SourceNotFound
        };

        return false;
    }

    private void SetLanguage(string culture)
    {
        if (IsBusy)
            return;

        _preferences.SetLocalization(culture);
        _localization.Apply(culture);

        RefreshLocalizedBindings();
        SetStatus($"{UserInterface.Menu_Lang}: {CurrentLanguageDisplay}");
    }

    private void SetLogType(string logType)
    {
        if (IsBusy)
            return;

        _preferences.SetLogType(logType);
        RefreshLocalizedBindings();
        SetStatus($"{UserInterface.Menu_Log}: {CurrentLogDisplay}");
    }

    private void RefreshLocalizedBindings()
    {
        OnPropertyChanged(nameof(JobsSectionTitle));
        OnPropertyChanged(nameof(NoJobsText));
        OnPropertyChanged(nameof(AddSectionTitle));
        OnPropertyChanged(nameof(NameLabel));
        OnPropertyChanged(nameof(SourceLabel));
        OnPropertyChanged(nameof(TargetLabel));
        OnPropertyChanged(nameof(TypeLabel));
        OnPropertyChanged(nameof(AddButtonLabel));
        OnPropertyChanged(nameof(RemoveButtonLabel));
        OnPropertyChanged(nameof(RefreshButtonLabel));
        OnPropertyChanged(nameof(RunSelectedButtonLabel));
        OnPropertyChanged(nameof(RunAllButtonLabel));
        OnPropertyChanged(nameof(LanguageSectionTitle));
        OnPropertyChanged(nameof(LogTypeSectionTitle));
        OnPropertyChanged(nameof(FrenchButtonLabel));
        OnPropertyChanged(nameof(EnglishButtonLabel));
        OnPropertyChanged(nameof(JsonButtonLabel));
        OnPropertyChanged(nameof(XmlButtonLabel));
        OnPropertyChanged(nameof(CurrentSettingsLabel));
    }

    private void OnProgressChanged(double percentage)
    {
        var clamped = Math.Max(0, Math.Min(percentage, 100));

        if (Dispatcher.UIThread.CheckAccess())
        {
            OverallProgress = clamped;
            return;
        }

        Dispatcher.UIThread.Post(() => OverallProgress = clamped);
    }

    private void SetStatus(string message)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            StatusMessage = message;
            return;
        }

        Dispatcher.UIThread.Post(() => StatusMessage = message);
    }

    private bool IsCurrentLanguage(string culture)
    {
        var current = string.IsNullOrWhiteSpace(_preferences.Localization) ? "en-US" : _preferences.Localization;
        return string.Equals(current, culture, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsCurrentLogType(string logType)
    {
        return string.Equals(_preferences.LogType, logType, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildSelectedLabel(string label, bool selected)
    {
        return selected ? $"{label} (Selected)" : label;
    }

    private static string TranslateAddError(string errorCode)
    {
        return errorCode switch
        {
            "Error.MaxJobs" => UserInterface.Add_Error_MaxJobs,
            "Error.NoFreeSlot" => UserInterface.Add_Error_NoFreeSlot,
            _ => errorCode
        };
    }
}
