using EasySave.Models.Backup.Interfaces;
using EasySave.Models.Utils;

namespace EasySave.Models.State;

/// <summary>
///     Manages and updates the state of backup jobs during their execution.
///     Provides methods to change the state based on different events in the backup process.
/// </summary>
public static class StateLogger
{
    /// <summary>
    ///     Sets the state of the backup job to active, initializing relevant properties.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    /// <param name="filesCount">The total number of files to be processed.</param>
    /// <param name="totalSize">The total size of files to be backed up.</param>
    public static void SetStateActive(BackupJobState state, int filesCount, long totalSize)
    {
        StateFileSingleton.Instance.UpdateState(state, jobState =>
        {
            jobState.State = JobRunState.Active; // Mark the job as active
            jobState.TotalFiles = filesCount; // Set total files count
            jobState.TotalSizeBytes = totalSize; // Set total size
            jobState.ProgressPercent = 0; // Initialize progress percentage
            jobState.RemainingFiles = filesCount; // Set remaining files count
            jobState.RemainingSizeBytes = totalSize; // Set remaining size
            jobState.CurrentAction = "start"; // Set current action
            jobState.CurrentSourcePath = null; // Reset current source path
            jobState.CurrentTargetPath = null; // Reset current target path
        });
    }

    /// <summary>
    ///     Sets the state of the backup job to failed due to an error.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    public static void SetStateFailed(BackupJobState state)
    {
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.State = JobRunState.Failed; // Mark the job as failed
            s.CurrentAction = "Source or target unavailable"; // Set action message
            s.CurrentSourcePath = null; // Reset source path
            s.CurrentTargetPath = null; // Reset target path
            s.ProgressPercent = 0; // Reset progress
            s.RemainingFiles = 0; // Reset remaining files
            s.RemainingSizeBytes = 0; // Reset remaining size
        });
    }

    /// <summary>
    ///     Sets the state of the backup job to failed because a business software process is running.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    public static void SetStateStoppedByBusinessSoftware(BackupJobState state)
    {
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.State = JobRunState.Stopped;
            s.CurrentAction = "Stopped: business software running";
            s.CurrentSourcePath = null;
            s.CurrentTargetPath = null;
        });
    }

    public static void SetStatePaused(BackupJobState state)
    {
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.State = JobRunState.Paused;
        });
    }

    /// <summary>
    ///     Sets the state of the backup job to completed, based on if an error occurred.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    /// <param name="hadError">Indicates if an error occurred during the backup.</param>
    public static void SetStateEnd(BackupJobState state, bool hadError, bool wasStopped)
    {
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.State = wasStopped ? JobRunState.Stopped : (hadError ? JobRunState.Failed : JobRunState.Completed); // Set job state
            s.CurrentAction = hadError ? "completed_with_errors" : "completed"; // Action message
            s.CurrentSourcePath = null; // Reset source path
            s.CurrentTargetPath = null; // Reset target path
            s.ProgressPercent = 100; // Set progress to 100%
            s.RemainingFiles = 0; // Reset remaining files
            s.RemainingSizeBytes = 0; // Reset remaining size
        });
    }

    /// <summary>
    ///     Sets the state for the beginning of a file transfer.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    /// <param name="file">The file being transferred.</param>
    public static void SetStateStartTransfer(BackupJobState state, IFile file)
    {
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.State = JobRunState.Active;
            s.CurrentAction = "file_transfer"; // Set current action
            s.CurrentSourcePath = PathService.ToFullUncLikePath(file.SourceFile); // Set source path
            s.CurrentTargetPath = PathService.ToFullUncLikePath(file.TargetFile); // Set target path
        });
    }

    /// <summary>
    ///     Sets the state of the backup job to active and initialises priority/standard queue counters.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    /// <param name="filesCount">Total number of files to process.</param>
    /// <param name="totalSize">Total size in bytes.</param>
    /// <param name="priorityCount">Number of priority files.</param>
    /// <param name="standardCount">Number of standard files.</param>
    public static void SetStateActiveWithQueues(
        BackupJobState state,
        int filesCount,
        long totalSize,
        int priorityCount,
        int standardCount)
    {
        StateFileSingleton.Instance.UpdateState(state, jobState =>
        {
            jobState.State = JobRunState.Active;
            jobState.TotalFiles = filesCount;
            jobState.TotalSizeBytes = totalSize;
            jobState.ProgressPercent = 0;
            jobState.RemainingFiles = filesCount;
            jobState.RemainingSizeBytes = totalSize;
            jobState.CurrentAction = "start";
            jobState.CurrentSourcePath = null;
            jobState.CurrentTargetPath = null;
            jobState.RemainingPriorityFiles = priorityCount;
            jobState.RemainingStandardFiles = standardCount;
        });
    }

    /// <summary>
    ///     Sets the state for the beginning of a file transfer, labelling it as priority or standard.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    /// <param name="file">The file being transferred.</param>
    /// <param name="isPriority">Whether the file belongs to the priority queue.</param>
    public static void SetStateStartTransfer(BackupJobState state, IFile file, bool isPriority)
    {
        var label = isPriority ? "priority file transfer" : "standard file transfer";
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.State = JobRunState.Active;
            s.CurrentAction = label;
            s.CurrentSourcePath = PathService.ToFullUncLikePath(file.SourceFile);
            s.CurrentTargetPath = PathService.ToFullUncLikePath(file.TargetFile);
        });
    }

    /// <summary>
    ///     Updates progress counters, including priority/standard remaining counts.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    /// <param name="filesCount">Total number of files.</param>
    /// <param name="processedIndex">Zero-based index of the file just processed.</param>
    /// <param name="totalSize">Total size in bytes.</param>
    /// <param name="transferredSize">Transferred size in bytes so far.</param>
    /// <param name="currentProgress">Current progress percentage.</param>
    /// <param name="remainingPriority">Remaining priority files count.</param>
    /// <param name="remainingStandard">Remaining standard files count.</param>
    public static void SetStateEndTransferWithQueues(
        BackupJobState state,
        int filesCount,
        int processedIndex,
        long totalSize,
        long transferredSize,
        double currentProgress,
        int remainingPriority,
        int remainingStandard)
    {
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.RemainingFiles = filesCount - (processedIndex + 1);
            s.RemainingSizeBytes = totalSize - transferredSize;
            s.ProgressPercent = currentProgress;
            s.RemainingPriorityFiles = remainingPriority;
            s.RemainingStandardFiles = remainingStandard;
        });
    }
}
