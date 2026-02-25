using EasySave.Models.State;

namespace EasySave.Models.Backup.Interfaces;

/// <summary>
///     Coordinates automatic pause/resume behavior when business software is detected.
/// </summary>
public interface IBusinessSoftwarePauseCoordinator
{
    /// <summary>
    ///     Registers a running job so it participates in global business-software detection.
    /// </summary>
    /// <param name="jobId">Running job identifier.</param>
    /// <param name="backupName">Running job display name.</param>
    /// <param name="monitor">Business software monitor used by this job.</param>
    /// <returns>A scope that unregisters the job when disposed.</returns>
    IDisposable RegisterJob(int jobId, string backupName, IBusinessSoftwareMonitor monitor);

    /// <summary>
    ///     Blocks the caller while business software is running, and marks runtime state accordingly.
    /// </summary>
    /// <param name="jobId">Calling job identifier.</param>
    /// <param name="state">Runtime state entry of the calling job.</param>
    /// <param name="blockedFile">File currently blocked by the pause, when available.</param>
    /// <param name="shouldStop">Function that indicates whether caller should stop waiting.</param>
    void WaitWhileBusinessSoftwareRuns(int jobId, BackupJobState state, IFile? blockedFile, Func<bool> shouldStop);
}
