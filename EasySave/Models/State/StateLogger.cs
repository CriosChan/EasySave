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
    ///     Sets the state of the backup job to completed, based on if an error occurred.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    /// <param name="hadError">Indicates if an error occurred during the backup.</param>
    public static void SetStateEnd(BackupJobState state, bool hadError)
    {
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.State = hadError ? JobRunState.Failed : JobRunState.Completed; // Set job state
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
            s.CurrentAction = "file_transfer"; // Set current action
            s.CurrentSourcePath = PathService.ToFullUncLikePath(file.SourceFile); // Set source path
            s.CurrentTargetPath = PathService.ToFullUncLikePath(file.TargetFile); // Set target path
        });
    }

    /// <summary>
    ///     Updates the state with the progress of file transfers.
    /// </summary>
    /// <param name="state">The current state of the backup job.</param>
    /// <param name="filesCount">Total number of files being processed.</param>
    /// <param name="i1">The index of the current file.</param>
    /// <param name="totalSize">Total size of the files being processed.</param>
    /// <param name="transferredSize">Size of the files that have been transferred.</param>
    /// <param name="currentProgress">Current progress percentage.</param>
    public static void SetStateEndTransfer(BackupJobState state, int filesCount, int i1, long totalSize,
        long transferredSize, double currentProgress)
    {
        StateFileSingleton.Instance.UpdateState(state, s =>
        {
            s.RemainingFiles = filesCount - (i1 + 1); // Update remaining files
            s.RemainingSizeBytes = totalSize - transferredSize; // Update remaining size
            s.ProgressPercent = currentProgress; // Update current progress
        });
    }
}