using System.ComponentModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Models.Backup;
using EasySave.Models.Utils;

namespace EasySave.ViewModels;

/// <summary>
///     ViewModel wrapper for BackupJob model, providing UI-specific properties.
/// </summary>
public sealed partial class BackupJobItemViewModel : ViewModelBase
{
    /// <summary>
    ///     Initializes a new instance of the BackupJobItemViewModel class.
    /// </summary>
    /// <param name="job">The BackupJob model to wrap.</param>
    public BackupJobItemViewModel(BackupJob job)
    {
        Job = job ?? throw new ArgumentNullException(nameof(job));
        _stopped = job.WasStopped;
        Job.PauseEvent += OnPausedChanged;
        Job.ProgressChanged += OnProgressChanged;
        Job.StopEvent += OnStopChanged;
        Job.EndEvent += OnJobEnded;
    }

    /// <summary>
    ///     Gets the underlying BackupJob model.
    /// </summary>
    public BackupJob Job { get; }

    /// <summary>
    ///     Gets the display name for the job.
    /// </summary>
    public string DisplayName => $"[{Job.Id}] {Job.Name}";

    /// <summary>
    ///     Gets the backup type as a string.
    /// </summary>
    public string Type => Job.Type.ToString();

    /// <summary>
    ///     Gets the formatted path display (Source → Target).
    /// </summary>
    public string DisplayPath => $"{Job.SourceDirectory} → {Job.TargetDirectory}";

    [ObservableProperty] private bool _paused = false;
    [ObservableProperty] private bool _stopped; 
    [ObservableProperty] private bool _inverseStopped;
    [ObservableProperty] private double _progress = 0;
    [ObservableProperty] private Brush _progressBarColor = new SolidColorBrush(Color.FromRgb(33, 150, 243));
    [ObservableProperty] private Bitmap? _pauseIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/pause-button.png"));
    [ObservableProperty] private Bitmap? _stopIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/play-button.png"));
    [ObservableProperty] private string _statusMessage = "";
    
    private void OnPausedChanged(object? sender, EventArgs e)
    {
        Paused = Job.IsPaused();
        PauseIcon = Job.IsPaused() ? ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/play-button.png")) : PauseIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/pause-button.png"));;
    }

    private void OnStopChanged(object? sender, EventArgs e)
    {
        Stopped = Job.WasStopped;
        if (Stopped)
        {
            StopIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/play-button.png"));
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressBarColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            });
        }
        else
        {
            StopIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/stop-button.png"));
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressBarColor = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            });
        }
        InverseStopped = !Job.WasStopped;
    }

    private void OnProgressChanged(object? sender, EventArgs e)
    {
        Progress = Job.CurrentProgress;
        StatusMessage =
            $"({Job.CurrentFileIndex} / {Job.FilesCount} files)\n" +
            $"({Math.Round(Job.TransferredSize / 1048576.0)} / {Math.Round(Job.TotalSize / 1048576.0)} MB)";
    }

    private void OnJobEnded(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            StatusMessage = "";
            ProgressBarColor = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            Progress = 100;
            Stopped = true;
            InverseStopped = false;
            StopIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/play-button.png"));
        });
    }

    [RelayCommand]
    private void PauseResumeJob()
    {
        Console.WriteLine(Paused);
        if (!Paused)
        {
            Job.Pause();
            ProgressBarColor = new SolidColorBrush(Job.WasStoppedByBusinessSoftware ? Color.FromRgb(255, 152, 0) : Color.FromRgb(251, 255, 0));
        }
        else
        {
            Job.Resume();
            ProgressBarColor = new SolidColorBrush(Color.FromRgb(33, 150, 243));
        }
    }

    [RelayCommand]
    private void StartStopJob()
    {
        if (!Stopped)
        {
            Job.Stop();
        }
        else
        {
            // Todo : TEMP THREADING HERE
            new Thread(Job.StartBackup).Start();
        }
    }
}