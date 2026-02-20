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

        // Execute jobs in parallel
        var tasks = jobList.Select(async job =>
        {
            // Check if execution should stop due to business software
            lock (lockObject)
            {
                if (stoppedByBusinessSoftware)
                {
                    _jobStates[job.Id] = JobExecutionState.Skipped;
                    return;
                }
            }

            _jobStates[job.Id] = JobExecutionState.Active;

            try
            {
                IProgress<BackupExecutionProgressSnapshot>? progress = null;
                if (progressCallback != null)
                    progress = new Progress<BackupExecutionProgressSnapshot>(snapshot => progressCallback(job, snapshot));

                var result = await _executionEngine.ExecuteJobAsync(job, progress, cancellationToken);
                results.Add(result);

                if (result.WasStoppedByBusinessSoftware)
                {
                    lock (lockObject)
                    {
                        stoppedByBusinessSoftware = true;
                    }

                    _jobStates[job.Id] = JobExecutionState.StoppedByBusinessSoftware;
                }
                else
                {
                    _jobStates[job.Id] = JobExecutionState.Completed;
                }
            }
            catch (OperationCanceledException)
            {
                _jobStates[job.Id] = JobExecutionState.Cancelled;
                throw;
            }
            catch (Exception)
            {
                _jobStates[job.Id] = JobExecutionState.Failed;
                throw;
            }
        });

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

/// <summary>
///     Represents the execution state of a job within the orchestrator.
/// </summary>
public enum JobExecutionState
{
    /// <summary>
    ///     Job is waiting to start.
    /// </summary>
    Pending,

    /// <summary>
    ///     Job is currently executing.
    /// </summary>
    Active,

    /// <summary>
    ///     Job completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    ///     Job failed with an error.
    /// </summary>
    Failed,

    /// <summary>
    ///     Job was cancelled by user request.
    /// </summary>
    Cancelled,

    /// <summary>
    ///     Job was stopped because business software was detected.
    /// </summary>
    StoppedByBusinessSoftware,

    /// <summary>
    ///     Job was skipped (e.g., due to earlier business software detection).
    /// </summary>
    Skipped
}

/// <summary>
///     Result summary from orchestrated execution.
/// </summary>
public sealed class OrchestrationResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OrchestrationResult" /> class.
    /// </summary>
    public OrchestrationResult(int completedCount, int failedCount, int cancelledCount, bool wasStoppedByBusinessSoftware)
    {
        CompletedCount = completedCount;
        FailedCount = failedCount;
        CancelledCount = cancelledCount;
        WasStoppedByBusinessSoftware = wasStoppedByBusinessSoftware;
    }

    /// <summary>
    ///     Gets the number of jobs that completed successfully.
    /// </summary>
    public int CompletedCount { get; }

    /// <summary>
    ///     Gets the number of jobs that failed.
    /// </summary>
    public int FailedCount { get; }

    /// <summary>
    ///     Gets the number of jobs that were cancelled or skipped.
    /// </summary>
    public int CancelledCount { get; }

    /// <summary>
    ///     Gets a value indicating whether execution was stopped due to business software.
    /// </summary>
    public bool WasStoppedByBusinessSoftware { get; }
}

