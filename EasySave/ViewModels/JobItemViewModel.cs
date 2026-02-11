using EasySave.Domain.Models;

namespace EasySave.ViewModels;

public sealed class JobItemViewModel : ViewModelBase
{
    private string _backupName;
    private string _currentAction;
    private string _lastError;
    private double _progressPercent;
    private int _remainingFiles;
    private long _remainingSizeBytes;
    private JobRunState _state;
    private string _sourceDirectory;
    private string _targetDirectory;

    public JobItemViewModel(BackupJob job)
    {
        Id = job.Id;
        _backupName = job.Name;
        _sourceDirectory = job.SourceDirectory;
        _targetDirectory = job.TargetDirectory;
        Type = job.Type;
        _state = JobRunState.Inactive;
        _currentAction = "idle";
        _lastError = string.Empty;
    }

    public int Id { get; }

    public BackupType Type { get; }

    public string BackupName
    {
        get => _backupName;
        set => SetProperty(ref _backupName, value);
    }

    public string SourceDirectory
    {
        get => _sourceDirectory;
        set => SetProperty(ref _sourceDirectory, value);
    }

    public string TargetDirectory
    {
        get => _targetDirectory;
        set => SetProperty(ref _targetDirectory, value);
    }

    public JobRunState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    public double ProgressPercent
    {
        get => _progressPercent;
        set => SetProperty(ref _progressPercent, value);
    }

    public int RemainingFiles
    {
        get => _remainingFiles;
        set => SetProperty(ref _remainingFiles, value);
    }

    public long RemainingSizeBytes
    {
        get => _remainingSizeBytes;
        set => SetProperty(ref _remainingSizeBytes, value);
    }

    public string CurrentAction
    {
        get => _currentAction;
        set => SetProperty(ref _currentAction, value);
    }

    public string LastError
    {
        get => _lastError;
        set => SetProperty(ref _lastError, value);
    }

    public string Summary =>
        $"[{Id}] {BackupName} | {Type} | {State} | {ProgressPercent:F1}% | Remaining files: {RemainingFiles}";

    public BackupJob ToDomainJob()
    {
        return new BackupJob(Id, BackupName, SourceDirectory, TargetDirectory, Type);
    }

    public void ApplyState(BackupJobState state)
    {
        if (state.JobId != Id)
            return;

        BackupName = state.BackupName;
        State = state.State;
        ProgressPercent = state.ProgressPercent;
        RemainingFiles = state.RemainingFiles;
        RemainingSizeBytes = state.RemainingSizeBytes;
        CurrentAction = state.CurrentAction ?? string.Empty;
        LastError = state.LastError ?? string.Empty;
        OnPropertyChanged(nameof(Summary));
    }
}
