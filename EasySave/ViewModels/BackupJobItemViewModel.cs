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
/// ViewModel wrapper for the BackupJob model, providing UI-specific properties and commands.
/// </summary>
public sealed partial class BackupJobItemViewModel : ViewModelBase
{
    private StatusBarViewModel _statusBar; // Reference to the status bar view model

    /// <summary>
    /// Initializes a new instance of the BackupJobItemViewModel class with a specified BackupJob and a StatusBarViewModel.
    /// </summary>
    /// <param name="job">The BackupJob model to wrap.</param>
    public BackupJobItemViewModel(BackupJob job) : this(job, new StatusBarViewModel())
    {
    }

    public BackupJobItemViewModel(BackupJob job, StatusBarViewModel statusBar)
    {
        Job = job ?? throw new ArgumentNullException(nameof(job)); // Ensure job is not null
        _stopped = job.WasStopped;
        _statusBar = statusBar;

        // Subscribe to events from the BackupJob
        Job.PauseEvent += OnPausedChanged;
        Job.ProgressChanged += OnProgressChanged;
        Job.StopEvent += OnStopChanged;
        Job.EndEvent += OnJobEnded;
        Job.FilesCountEvent += OnFilesCountChange;
    }

    /// <summary>
    /// Gets the underlying BackupJob model.
    /// </summary>
    public BackupJob Job { get; }

    /// <summary>
    /// Gets the display name for the backup job, formatted with its ID and name.
    /// </summary>
    public string DisplayName => $"[{Job.Id}] {Job.Name}";

    /// <summary>
    /// Gets the backup type as a string representation.
    /// </summary>
    public string Type => Job.Type.ToString();

    /// <summary>
    /// Gets the formatted display path for source and target directories.
    /// </summary>
    public string DisplayPath => $"{Job.SourceDirectory} → {Job.TargetDirectory}";

    [ObservableProperty] private bool _paused = false; // Indicates if the job is paused
    [ObservableProperty] private bool _stopped; // Indicates if the job is stopped
    [ObservableProperty] private bool _inverseStopped; // Inverse state for UI purposes
    [ObservableProperty] private double _progress = 0; // Progress percentage of the job
    [ObservableProperty] private Brush _progressBarColor = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Color of the progress bar
    [ObservableProperty] private Bitmap? _pauseIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/pause-button.png")); // Icon for pause
    [ObservableProperty] private Bitmap? _stopIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/play-button.png")); // Icon for stop
    [ObservableProperty] private string _statusMessage = ""; // Message to display status updates

    /// <summary>
    /// Event handler called when the paused state changes.
    /// </summary>
    private void OnPausedChanged(object? sender, EventArgs e)
    {
        Paused = Job.IsPaused(); // Update the paused state
        PauseIcon = Job.IsPaused() 
            ? ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/play-button.png")) 
            : ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/pause-button.png"));
    }

    /// <summary>
    /// Event handler called when the stopped state changes.
    /// </summary>
    private void OnStopChanged(object? sender, EventArgs e)
    {
        Stopped = Job.WasStopped; // Update the stopped state
        if (Stopped)
        {
            StopIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/play-button.png"));
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressBarColor = new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Change color to red for stopped state
            });
        }
        else
        {
            StopIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/stop-button.png"));
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressBarColor = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Change color back to original
            });
        }
        InverseStopped = !Job.WasStopped; // Toggle inverse stopped state for UI
    }

    /// <summary>
    /// Event handler called when the job progress changes.
    /// </summary>
    private void OnProgressChanged(object? sender, EventArgs e)
    {
        Progress = Job.CurrentProgress; // Update progress
        _statusBar.UpdateOverallProgress(); // Update overall progress in status bar
        StatusMessage =
            $"({Job.CurrentFileIndex} / {Job.FilesCount} files)\n" +
            $"({Math.Round(Job.TransferredSize / 1048576.0)} / {Math.Round(Job.TotalSize / 1048576.0)} MB)"; // Show detailed status message
    }

    /// <summary>
    /// Event handler called when the backup job ends.
    /// </summary>
    private void OnJobEnded(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            StatusMessage = ""; // Clear status message
            ProgressBarColor = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Set color to green for completion
            Progress = 100; // Set progress to 100%
            Stopped = true; // Mark job as stopped
            InverseStopped = false; // Reset inverse stopped state
            StopIcon = ImageHelper.LoadFromResource(new Uri("avares://EasySave/Assets/play-button.png")); // Reset stop icon
        });
        _statusBar.RemoveProgress(Job.FilesCount); // Update status bar to remove progress
    }

    /// <summary>
    /// Event handler called when the file count changes.
    /// </summary>
    private void OnFilesCountChange(object? sender, EventArgs e)
    {
        _statusBar.AddMaxProgress(Job.FilesCount); // Update max progress in the status bar
    }

    /// <summary>
    /// Command to pause or resume the backup job.
    /// </summary>
    [RelayCommand]
    private void PauseResumeJob()
    {
        if (!Paused)
        {
            Job.Pause(); // Pause the job
            ProgressBarColor = new SolidColorBrush(Job.WasStoppedByBusinessSoftware ? 
                Color.FromRgb(255, 152, 0) : Color.FromRgb(251, 255, 0)); // Update progress bar color based on state
        }
        else
        {
            Job.Resume(); // Resume the job
            ProgressBarColor = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Reset progress bar color
        }
    }

    /// <summary>
    /// Command to start or stop the backup job.
    /// </summary>
    [RelayCommand]
    private void StartStopJob()
    {
        if (!Stopped)
        {
            Job.Stop(); // Stop the job
        }
        else
        {
            // TODO: Handle threading appropriately
            new Thread(Job.StartBackup).Start(); // Start the job in a new thread
        }
    }
}
