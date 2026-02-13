using EasySave.Models.Backup;
using EasySave.Models.Utils;

namespace EasySave.Models.State;

/// <summary>
///     Writes the real-time backup state file (<c>state.json</c>).
/// </summary>
/// <remarks>
///     v1.0/v1.1 execute jobs sequentially in a single thread.
///     Therefore we do not need locking primitives; we just keep an in-memory list and
///     rewrite the state file atomically each time the state changes.
/// </remarks>
public sealed class StateFileSingleton
{
    // Singleton instance for the StateFileSingleton class
    private static readonly Lazy<StateFileSingleton> _instance = new(() => new StateFileSingleton());

    private string _statePath; // Path to the state file
    private List<BackupJobState> _states = new(); // In-memory list of job states

    /// <summary>
    ///     Private constructor to prevent instantiation.
    ///     Initializes the state path.
    /// </summary>
    private StateFileSingleton()
    {
        _statePath = string.Empty; // Initialize to empty; should be set through the Init method
    }

    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static StateFileSingleton Instance => _instance.Value;

    /// <summary>
    ///     Initializes the state file with the provided directory and job list.
    /// </summary>
    /// <param name="stateDirectory">Directory where state.json is written.</param>
    /// <param name="jobs">List of backup jobs to initialize state for.</param>
    public void Initialize(string stateDirectory)
    {
        _statePath = Path.Combine(stateDirectory, "state.json");
        _states = new JobService().GetAll()
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

        // Write the initial state to the state file
        JsonFile.WriteAtomic(_statePath, _states);
    }

    /// <summary>
    ///     Updates the in-memory state for a job and rewrites state.json.
    /// </summary>
    /// <param name="updated">The updated job state to apply.</param>
    public void Update(BackupJobState updated)
    {
        var idx = _states.FindIndex(s => s.JobId == updated.JobId);
        if (idx >= 0)
            _states[idx] = updated; // Update existing state
        else
            _states.Add(updated); // Add new state if it doesn't exist

        // Keep the list ordered by JobId
        _states = _states.OrderBy(s => s.JobId).ToList();
        JsonFile.WriteAtomic(_statePath, _states); // Write the updated state to the file
    }

    /// <summary>
    ///     Returns the existing state for a job, or creates it if missing.
    /// </summary>
    /// <param name="id">The ID of the backup job.</param>
    /// <param name="name">The name of the backup job.</param>
    /// <returns>The existing or newly created job state.</returns>
    public BackupJobState GetOrCreate(int id, string name)
    {
        var existing = _states.FirstOrDefault(s => s.JobId == id);
        if (existing != null)
            return existing;

        // Create a new state if none exists for the job
        existing = new BackupJobState
        {
            JobId = id,
            BackupName = name,
            LastActionTimestamp = DateTime.Now,
            State = JobRunState.Inactive
        };

        _states.Add(existing);
        JsonFile.WriteAtomic(_statePath, _states); // Persist the new state to the file
        return existing;
    }

    /// <summary>
    ///     Updates the state of the backup job with applied changes.
    /// </summary>
    /// <param name="state">The current job state to update.</param>
    /// <param name="apply">An action to modify the state.</param>
    public void UpdateState(BackupJobState state, Action<BackupJobState> apply)
    {
        apply(state); // Apply changes through the provided action
        state.LastActionTimestamp = DateTime.Now; // Update last action timestamp
        Update(state); // Persist updated state
    }
}