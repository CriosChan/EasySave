using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Manages pause, resume, and stop control for a backup job.
/// </summary>
public sealed class BackupJobController : IBackupJobController
{
    private readonly ManualResetEvent _pauseEvent = new(true);

    /// <inheritdoc />
    public bool WasStopped { get;
        private set
        {
            field = value;
            StopEvent?.Invoke(this, EventArgs.Empty);
        } } = true;

    /// <inheritdoc />
    public bool WasStoppedByBusinessSoftware { get;
        set
        {
            field = value;
            //StopEPavent?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public event EventHandler? PauseEvent;

    /// <inheritdoc />
    public event EventHandler? StopEvent;

    /// <inheritdoc />
    public void Pause()
    {
        _pauseEvent.Reset();
        PauseEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Resume()
    {
        _pauseEvent.Set();
        PauseEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Stop()
    {
        WasStopped = true;
        _pauseEvent.Set();
        PauseEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public bool IsPaused()
    {
        return !_pauseEvent.WaitOne(0);
    }

    /// <inheritdoc />
    public void ResetStopped()
    {
        WasStopped = false;
    }

    /// <inheritdoc />
    public void WaitIfPaused()
    {
        _pauseEvent.WaitOne();
    }

    /// <inheritdoc />
    public void WaitIfPaused(int millisecondsTimeout)
    {
        _pauseEvent.WaitOne(millisecondsTimeout);
    }
}

