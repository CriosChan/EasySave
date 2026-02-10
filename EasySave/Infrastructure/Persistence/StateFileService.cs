using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Infrastructure.IO;

namespace EasySave.Infrastructure.Persistence;

/// <summary>
///     Writes the real-time backup state file (<c>state.json</c>).
/// </summary>
/// <remarks>
///     v1.0/v1.1 execute jobs sequentially in a single thread.
///     Therefore we do not need locking primitives; we just keep an in-memory list and
///     rewrite the state file atomically each time the state changes.
/// </remarks>
public sealed class StateFileService : IStateService
{
    private readonly string _statePath;
    private List<BackupJobState> _states = new();

    /// <summary>
    ///     Creates the service and computes the state file location.
    /// </summary>
    /// <param name="stateDirectory">Directory where state.json is written.</param>
    public StateFileService(string stateDirectory)
    {
        _statePath = Path.Combine(stateDirectory, "state.json");
    }

    /// <summary>
    ///     Initializes the state file with all jobs as "Inactive".
    /// </summary>
    public void Initialize(IEnumerable<BackupJob> jobs)
    {
        _states = jobs
            .OrderBy(j => j.Id)
            .Select(j => new BackupJobState
            {
                JobId = j.Id,
                BackupName = j.Name,
                LastActionTimestamp = DateTime.Now,
                State = JobRunState.Inactive,
                TotalFiles = 0,
                TotalSizeBytes = 0,
                ProgressPercent = 0,
                RemainingFiles = 0,
                RemainingSizeBytes = 0,
                CurrentAction = null,
                CurrentSourcePath = null,
                CurrentTargetPath = null
            })
            .ToList();

        JsonFile.WriteAtomic(_statePath, _states);
    }

    /// <summary>
    ///     Updates the in-memory state for a job and rewrites state.json.
    /// </summary>
    public void Update(BackupJobState updated)
    {
        var idx = _states.FindIndex(s => s.JobId == updated.JobId);
        if (idx >= 0)
            _states[idx] = updated;
        else
            _states.Add(updated);

        _states = _states.OrderBy(s => s.JobId).ToList();
        JsonFile.WriteAtomic(_statePath, _states);
    }

    /// <summary>
    ///     Returns the existing state for a job, or creates it if missing.
    /// </summary>
    public BackupJobState GetOrCreate(BackupJob job)
    {
        var existing = _states.FirstOrDefault(s => s.JobId == job.Id);
        if (existing != null)
            return existing;

        existing = new BackupJobState
        {
            JobId = job.Id,
            BackupName = job.Name,
            LastActionTimestamp = DateTime.Now,
            State = JobRunState.Inactive
        };

        _states.Add(existing);
        JsonFile.WriteAtomic(_statePath, _states);
        return existing;
    }
}