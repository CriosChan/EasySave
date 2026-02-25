using System.Collections.Concurrent;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.Models.Backup;

namespace EasySave.ViewModels;

/// <summary>
///     Holds the global status and progress state displayed in the bottom status layer of the application.
/// </summary>
public partial class StatusBarViewModel : ViewModelBase
{
    // Active job snapshots keyed by job ID
    private readonly ConcurrentDictionary<int, BackupExecutionProgressSnapshot> _activeSnapshots = new();
    [ObservableProperty] private double _maxProgress;
    [ObservableProperty] private double _overallProgress;
    [ObservableProperty] private string _statusMessage = string.Empty;

    /// <summary>
    ///     Registers or updates the progress snapshot for a running job,
    ///     then rebuilds the aggregated status message on the UI thread.
    /// </summary>
    /// <param name="snapshot">Latest progress snapshot for the job.</param>
    public void ReportJobProgress(BackupExecutionProgressSnapshot snapshot)
    {
        _activeSnapshots[snapshot.JobId] = snapshot;
        RefreshStatusMessage();
    }

    /// <summary>
    ///     Removes a job from the active registry and refreshes the status message.
    /// </summary>
    /// <param name="jobId">ID of the completed or failed job.</param>
    public void UnregisterJob(int jobId)
    {
        _activeSnapshots.TryRemove(jobId, out _);
        RefreshStatusMessage();
    }

    /// <summary>
    ///     Clears all active job snapshots (e.g. on full reset).
    /// </summary>
    public void ClearActiveJobs()
    {
        _activeSnapshots.Clear();
    }

    /// <summary>
    ///     Rebuilds the aggregated progress message from all active snapshots.
    /// </summary>
    private void RefreshStatusMessage()
    {
        var snapshots = _activeSnapshots.Values.ToList();
        if (snapshots.Count == 0)
            return;

        var totalFiles = snapshots.Sum(s => s.FilesCount);
        var processedFiles = snapshots.Sum(s => s.CurrentFileIndex);
        var totalBytes = snapshots.Sum(s => s.TotalSize);
        var processedBytes = snapshots.Sum(s => s.TransferredSize);

        var message = string.Format(
            "Running {0} job{1} - ({2} / {3} files) - ({4} / {5} MB)",
            snapshots.Count,
            snapshots.Count > 1 ? "s" : "",
            processedFiles,
            totalFiles,
            Math.Round(processedBytes / 1048576.0),
            Math.Round(totalBytes / 1048576.0));

        // Always post to UI thread — this may be called from background threads
        Dispatcher.UIThread.Post(() => StatusMessage = message);

        // Update global progress as average across all active jobs
        var avgProgress = snapshots.Average(s => s.CurrentProgress);
        Dispatcher.UIThread.Post(() => OverallProgress = avgProgress);
    }
}