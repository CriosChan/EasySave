using System.Collections.Concurrent;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Orchestrates parallel execution of multiple backup jobs with centralized supervision.
/// </summary>
public sealed class ParallelJobOrchestrator
{
    private readonly IBackupExecutionEngine _executionEngine;
    private readonly ConcurrentDictionary<int, JobExecutionState> _jobStates = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ParallelJobOrchestrator" /> class.
    /// </summary>
    /// <param name="executionEngine">Execution engine for running individual jobs.</param>
    public ParallelJobOrchestrator(IBackupExecutionEngine executionEngine)
    {
        _executionEngine = executionEngine ?? throw new ArgumentNullException(nameof(executionEngine));
    }

    /// <summary>
    ///     Gets the count of active jobs currently running.
    /// </summary>
    public int ActiveJobCount => _jobStates.Count(kvp => kvp.Value == JobExecutionState.Active);

    /// <summary>
    ///     Gets the count of completed jobs.
    /// </summary>
    public int CompletedJobCount => _jobStates.Count(kvp => kvp.Value == JobExecutionState.Completed);

    /// <summary>
    ///     Gets the count of failed jobs.
    /// </summary>
    public int FailedJobCount => _jobStates.Count(kvp => kvp.Value == JobExecutionState.Failed);

    /// <summary>
    ///     Gets the count of pending jobs.
    /// </summary>
    public int PendingJobCount => _jobStates.Count(kvp => kvp.Value == JobExecutionState.Pending);

    /// <summary>
    ///     Executes multiple backup jobs in parallel with centralized supervision.
    /// </summary>
    /// <param name="jobs">Jobs to execute.</param>
    /// <param name="progressCallback">Optional callback invoked when a job updates its progress.</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation.</param>
    /// <returns>Orchestration result containing execution summary.</returns>
    public async Task<OrchestrationResult> ExecuteAllAsync(
        IEnumerable<BackupJob> jobs,
        Action<BackupJob, BackupExecutionProgressSnapshot>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        var jobList = jobs.ToList();
        if (jobList.Count == 0)
            return new OrchestrationResult(0, 0, 0, false);

        // Initialize all jobs as pending
        _jobStates.Clear();
        foreach (var job in jobList)
            _jobStates[job.Id] = JobExecutionState.Pending;

        var results = new ConcurrentBag<BackupExecutionResult>();
        var stoppedByBusinessSoftware = false;
        var lockObject = new object();

        // Launch each job sequentially so they register correctly,
        // then let them all run in parallel via Task.WhenAll.
        var tasks = new List<Task>();
        foreach (var job in jobList)
        {
            lock (lockObject)
            {
                if (stoppedByBusinessSoftware)
                {
                    _jobStates[job.Id] = JobExecutionState.Skipped;
                    continue;
                }
            }

            _jobStates[job.Id] = JobExecutionState.Active;

            IProgress<BackupExecutionProgressSnapshot>? progress = null;
            if (progressCallback != null)
            {
                var capturedJob = job;
                progress = new DirectProgress<BackupExecutionProgressSnapshot>(snapshot =>
                    progressCallback(capturedJob, snapshot));
            }

            // Start the task and store it — execution runs in parallel on the thread pool
            var task = _executionEngine.ExecuteJobAsync(job, progress, cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _jobStates[job.Id] = JobExecutionState.Failed;
                        return;
                    }
                    if (t.IsCanceled)
                    {
                        _jobStates[job.Id] = JobExecutionState.Cancelled;
                        return;
                    }

                    var result = t.Result;
                    results.Add(result);

                    if (result.WasStoppedByBusinessSoftware)
                    {
                        lock (lockObject) { stoppedByBusinessSoftware = true; }
                        _jobStates[job.Id] = JobExecutionState.StoppedByBusinessSoftware;
                    }
                    else
                    {
                        _jobStates[job.Id] = JobExecutionState.Completed;
                    }
                }, TaskScheduler.Default);

            tasks.Add(task);
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception)
        {
            // Individual job errors are tracked in state
        }

        var completed = results.Count(r => !r.WasStoppedByBusinessSoftware);
        var failed = _jobStates.Count(kvp => kvp.Value == JobExecutionState.Failed);
        var cancelled = _jobStates.Count(kvp => kvp.Value == JobExecutionState.Cancelled || kvp.Value == JobExecutionState.Skipped);

        return new OrchestrationResult(completed, failed, cancelled, stoppedByBusinessSoftware);
    }

    /// <summary>
    ///     Gets the current state of a specific job.
    /// </summary>
    /// <param name="jobId">Job identifier.</param>
    /// <returns>Job execution state, or null if job is not tracked.</returns>
    public JobExecutionState? GetJobState(int jobId)
    {
        return _jobStates.TryGetValue(jobId, out var state) ? state : null;
    }

    /// <summary>
    ///     Clears all tracked job states.
    /// </summary>
    public void ClearStates()
    {
        _jobStates.Clear();
    }
}
