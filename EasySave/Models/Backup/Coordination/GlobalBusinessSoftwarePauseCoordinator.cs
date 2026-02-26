using EasySave.Core.Models;
using EasySave.Models.Backup.Abstractions;
using EasySave.Models.Logger;
using EasySave.Models.State;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup.Coordination;

/// <summary>
///     Global coordinator that pauses running jobs while business software is detected.
/// </summary>
public sealed class GlobalBusinessSoftwarePauseCoordinator : IBusinessSoftwarePauseCoordinator
{
    private const int DefaultPollingIntervalMs = 100;

    private readonly object _sync = new();
    private readonly Dictionary<int, JobRegistration> _registrations = new();
    private readonly HashSet<int> _pausedJobs = [];
    private readonly int _pollingIntervalMs;

    /// <summary>
    ///     Shared coordinator used by default by backup jobs.
    /// </summary>
    public static GlobalBusinessSoftwarePauseCoordinator Shared { get; } = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="GlobalBusinessSoftwarePauseCoordinator" /> class.
    /// </summary>
    /// <param name="pollingInterval">Polling interval used while waiting for software shutdown.</param>
    public GlobalBusinessSoftwarePauseCoordinator(TimeSpan? pollingInterval = null)
    {
        var interval = pollingInterval ?? TimeSpan.FromMilliseconds(DefaultPollingIntervalMs);
        if (interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(pollingInterval), "Polling interval must be greater than zero.");

        _pollingIntervalMs = Math.Max(1, (int)Math.Ceiling(interval.TotalMilliseconds));
    }

    /// <inheritdoc />
    public IDisposable RegisterJob(int jobId, string backupName, IBusinessSoftwareMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        var registration = new JobRegistration(
            jobId,
            backupName.ValidateNonEmpty(nameof(backupName)),
            monitor);

        lock (_sync)
        {
            _registrations[jobId] = registration;
            _pausedJobs.Remove(jobId);
        }

        return new RegistrationScope(() => UnregisterJob(jobId));
    }

    /// <inheritdoc />
    public void WaitWhileBusinessSoftwareRuns(int jobId, BackupJobState state, IFile? blockedFile, Func<bool> shouldStop,
        Action<bool>? onPauseStateChanged = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shouldStop);

        while (IsAnyBusinessSoftwareRunning())
        {
            if (shouldStop())
            {
                MarkResumed(jobId, blockedFile, onPauseStateChanged);
                return;
            }

            MarkPaused(jobId, state, blockedFile, onPauseStateChanged);
            Thread.Sleep(_pollingIntervalMs);
        }

        MarkResumed(jobId, blockedFile, onPauseStateChanged);
    }

    /// <summary>
    ///     Unregisters a job from global pause coordination.
    /// </summary>
    /// <param name="jobId">Job identifier.</param>
    private void UnregisterJob(int jobId)
    {
        lock (_sync)
        {
            _registrations.Remove(jobId);
            _pausedJobs.Remove(jobId);
        }
    }

    /// <summary>
    ///     Returns true when at least one registered monitor reports business software running.
    /// </summary>
    private bool IsAnyBusinessSoftwareRunning()
    {
        var monitors = GetMonitorSnapshot();
        foreach (var monitor in monitors)
        {
            try
            {
                if (monitor.IsBusinessSoftwareRunning())
                    return true;
            }
            catch
            {
                // Ignore monitor errors and keep evaluation running.
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns a monitor snapshot to evaluate without holding the registration lock.
    /// </summary>
    private IBusinessSoftwareMonitor[] GetMonitorSnapshot()
    {
        lock (_sync)
        {
            return _registrations.Values
                .Select(registration => registration.Monitor)
                .Distinct()
                .ToArray();
        }
    }

    /// <summary>
    ///     Marks a job as paused by business software and logs transition once.
    /// </summary>
    private void MarkPaused(int jobId, BackupJobState state, IFile? blockedFile, Action<bool>? onPauseStateChanged = null)
    {
        StateLogger.SetStatePausedBusinessSoftware(state, blockedFile);

        JobRegistration? registration;
        var shouldLog = false;

        lock (_sync)
        {
            if (!_registrations.TryGetValue(jobId, out registration))
                return;
            
            shouldLog = _pausedJobs.Add(jobId);
        }

        if (shouldLog)
        {
            LogPauseTransition(registration, blockedFile, started: true);
            onPauseStateChanged?.Invoke(true);
        }
    }

    /// <summary>
    ///     Marks a job as resumed from business software pause and logs transition once.
    /// </summary>
    private void MarkResumed(int jobId, IFile? blockedFile, Action<bool>? onPauseStateChanged = null)
    {
        JobRegistration? registration;
        var shouldLog = false;

        lock (_sync)
        {
            if (!_registrations.TryGetValue(jobId, out registration))
                return;

            shouldLog = _pausedJobs.Remove(jobId);
        }

        if (shouldLog)
        {
            LogPauseTransition(registration, blockedFile, started: false);
            onPauseStateChanged?.Invoke(false);
        }
    }

    /// <summary>
    ///     Logs business software pause/resume transition for one job.
    /// </summary>
    private static void LogPauseTransition(JobRegistration registration, IFile? blockedFile, bool started)
    {
        var logger = new ConfigurableLogWriter<LogEntry>();
        var configuredSoftware = registration.Monitor.ConfiguredSoftwareNames;
        var softwareLabel = configuredSoftware.Count == 0
            ? "configured business software"
            : string.Join(", ", configuredSoftware);

        logger.Log(new LogEntry
        {
            BackupName = registration.BackupName,
            SourcePath = blockedFile == null ? string.Empty : PathService.ToFullUncLikePath(blockedFile.SourceFile),
            TargetPath = blockedFile == null ? string.Empty : PathService.ToFullUncLikePath(blockedFile.TargetFile),
            FileSizeBytes = 0,
            TransferTimeMs = 0,
            ErrorMessage = started
                ? $"Automatic pause: business software running ('{softwareLabel}')."
                : $"Automatic resume: business software stopped ('{softwareLabel}')."
        });
    }

    /// <summary>
    ///     Registered runtime data for one job.
    /// </summary>
    private sealed class JobRegistration
    {
        public JobRegistration(int jobId, string backupName, IBusinessSoftwareMonitor monitor)
        {
            JobId = jobId;
            BackupName = backupName;
            Monitor = monitor;
        }

        public int JobId { get; }
        public string BackupName { get; }
        public IBusinessSoftwareMonitor Monitor { get; }
    }

    /// <summary>
    ///     Disposable scope used to unregister a job.
    /// </summary>
    private sealed class RegistrationScope : IDisposable
    {
        private readonly Action _onDispose;
        private int _disposed;

        public RegistrationScope(Action onDispose)
        {
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            _onDispose();
        }
    }
}
