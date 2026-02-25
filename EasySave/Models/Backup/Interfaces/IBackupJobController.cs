namespace EasySave.Models.Backup.Interfaces;

/// <summary>
///     Defines pause / resume / stop control operations for a backup job.
/// </summary>
public interface IBackupJobController
{
    /// <summary> Gets a value indicating whether the job was stopped. </summary>
    bool WasStopped { get; }

    /// <summary> Gets a value indicating whether the job was stopped by business-software detection. </summary>
    bool WasStoppedByBusinessSoftware { get; set; }

    /// <summary> Pauses the job execution. </summary>
    void Pause();

    /// <summary> Resumes a paused job. </summary>
    void Resume();

    /// <summary> Stops the job immediately. </summary>
    void Stop();

    /// <summary> Returns true when the job is currently paused. </summary>
    bool IsPaused();

    /// <summary> Marks the job as not stopped (resets the stopped flag). </summary>
    void ResetStopped();

    /// <summary> Waits for the pause event to be signalled; blocks indefinitely. </summary>
    void WaitIfPaused();

    /// <summary>
    ///     Waits for the pause event to be signalled with a timeout.
    /// </summary>
    /// <param name="millisecondsTimeout">Milliseconds to wait before returning.</param>
    void WaitIfPaused(int millisecondsTimeout);

    /// <summary> Raised when the pause / resume state changes. </summary>
    event EventHandler? PauseEvent;

    /// <summary> Raised when the job is stopped. </summary>
    event EventHandler? StopEvent;
}

