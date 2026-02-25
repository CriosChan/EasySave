using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Implements global priority arbitration across multiple backup jobs.
///     Maintains a global counter of remaining priority files and prevents standard file processing
///     while any priority files remain in the system, ensuring v3 business rule compliance.
/// </summary>
public sealed class GlobalPriorityArbitrator : IPriorityArbitrator
{
    private readonly object _sync = new();
    private Dictionary<int, int> _jobPriorityCounts = new();
    private int _totalPriorityFilesRemaining = 0;

    /// <summary>
    ///     Anti-starvation threshold: if a job waits longer than this duration without progress,
    ///     standard files are allowed to proceed.
    /// </summary>
    private const int AntiFamineTimeoutMs = 10000; // 10 seconds

    private Dictionary<int, DateTime> _jobWaitStartTimes = new();

    /// <inheritdoc />
    public void Initialize(Dictionary<int, int> jobPriorityCounts)
    {
        ArgumentNullException.ThrowIfNull(jobPriorityCounts);

        lock (_sync)
        {
            _jobPriorityCounts = new Dictionary<int, int>(jobPriorityCounts);
            _totalPriorityFilesRemaining = jobPriorityCounts.Values.Sum();
            _jobWaitStartTimes.Clear();
        }
    }

    /// <inheritdoc />
    public int GetGlobalPriorityFilesRemaining()
    {
        lock (_sync)
        {
            return _totalPriorityFilesRemaining;
        }
    }

    /// <inheritdoc />
    public void UpdateGlobalPriorityCount(int jobId, int newPriorityCount)
    {
        lock (_sync)
        {
            if (!_jobPriorityCounts.TryGetValue(jobId, out var oldCount))
                return; // Job not tracked

            var delta = newPriorityCount - oldCount;
            _jobPriorityCounts[jobId] = newPriorityCount;
            _totalPriorityFilesRemaining = Math.Max(0, _totalPriorityFilesRemaining + delta);

            // Remove from wait tracking if job is no longer blocked
            if (_jobWaitStartTimes.ContainsKey(jobId) && newPriorityCount == 0)
                _jobWaitStartTimes.Remove(jobId);
        }
    }

    /// <inheritdoc />
    public bool CanProcessStandardFile(int jobId)
    {
        lock (_sync)
        {
            // If no priority files remain globally, always allow
            if (_totalPriorityFilesRemaining == 0)
            {
                _jobWaitStartTimes.Remove(jobId);
                return true;
            }

            // Check anti-famine: if job has been waiting longer than threshold, allow
            if (_jobWaitStartTimes.TryGetValue(jobId, out var waitStart))
            {
                var elapsedMs = (DateTime.UtcNow - waitStart).TotalMilliseconds;
                if (elapsedMs > AntiFamineTimeoutMs)
                {
                    // Reset timer so the next blocking period starts fresh
                    _jobWaitStartTimes.Remove(jobId);
                    return true;
                }
            }
            else
            {
                // Record start of wait for this job
                _jobWaitStartTimes[jobId] = DateTime.UtcNow;
            }

            return false;
        }
    }

    /// <inheritdoc />
    public void OnJobCompleted(int jobId)
    {
        lock (_sync)
        {
            _jobPriorityCounts.Remove(jobId);
            _jobWaitStartTimes.Remove(jobId);

            // Recalculate total in case there are lingering counts
            _totalPriorityFilesRemaining = Math.Max(0, _jobPriorityCounts.Values.Sum());
        }
    }
}
