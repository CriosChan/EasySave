using EasySave.Models.Backup;
using EasySave.Models.Utils;

namespace EasySave.Models.State;

/// <summary>
///     Writes the real-time backup state file (<c>state.json</c>).
/// </summary>
/// <remarks>
///     The state store is shared by all running jobs and is protected by a synchronization lock.
///     Each mutation rewrites state.json atomically so readers always observe a valid file.
/// </remarks>
public sealed class StateFileSingleton
{
    // Singleton instance for the StateFileSingleton class
    private static readonly Lazy<StateFileSingleton> _instance = new(() => new StateFileSingleton());

    private readonly object _sync = new();
    private bool _isInitialized;
    private string _statePath; // Path to the state file
    private List<BackupJobState> _states = new(); // In-memory list of job states

    /// <summary>
    ///     Private constructor to prevent instantiation.
    ///     Initializes the state path.
    /// </summary>
    private StateFileSingleton()
    {
        _statePath = string.Empty; // Initialize to empty; should be set through the Init method
        _isInitialized = false;
    }

    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static StateFileSingleton Instance => _instance.Value;

    /// <summary>
    ///     Initializes the state file with the provided directory and job list.
    /// </summary>
    /// <param name="stateDirectory">Directory where state.json is written.</param>
    public void Initialize(string stateDirectory)
    {
        if (string.IsNullOrWhiteSpace(stateDirectory))
            throw new ArgumentException("State directory cannot be null or empty.", nameof(stateDirectory));

        var statePath = Path.Combine(stateDirectory, "state.json");
        var configuredJobs = new JobService().GetAll()
            .OrderBy(j => j.Id)
            .ToList();

        lock (_sync)
        {
            if (!string.Equals(_statePath, statePath, StringComparison.OrdinalIgnoreCase))
            {
                _statePath = statePath;
                _states = File.Exists(_statePath)
                    ? JsonFile.ReadOrDefault(_statePath, new List<BackupJobState>())
                    : new List<BackupJobState>();
            }

            SynchronizeConfiguredJobs(configuredJobs);
            PersistLocked();
            _isInitialized = true;
        }
    }

    /// <summary>
    ///     Updates the in-memory state for a job and rewrites state.json.
    /// </summary>
    /// <param name="updated">The updated job state to apply.</param>
    public void Update(BackupJobState updated)
    {
        ArgumentNullException.ThrowIfNull(updated);

        lock (_sync)
        {
            EnsureInitializedLocked();

            var idx = _states.FindIndex(s => s.JobId == updated.JobId);
            if (idx >= 0)
                _states[idx] = updated; // Update existing state
            else
                _states.Add(updated); // Add new state if it doesn't exist

            _states = _states.OrderBy(s => s.JobId).ToList(); // Keep list ordered by JobId
            PersistLocked(); // Write the updated state to the file
        }
    }

    /// <summary>
    ///     Returns the existing state for a job, or creates it if missing.
    /// </summary>
    /// <param name="id">The ID of the backup job.</param>
    /// <param name="name">The name of the backup job.</param>
    /// <returns>The existing or newly created job state.</returns>
    public BackupJobState GetOrCreate(int id, string name)
    {
        lock (_sync)
        {
            EnsureInitializedLocked();

            var existing = _states.FirstOrDefault(s => s.JobId == id);
            if (existing != null)
            {
                if (!string.Equals(existing.BackupName, name, StringComparison.Ordinal))
                {
                    existing.BackupName = name;
                    existing.LastActionTimestamp = DateTime.Now;
                    PersistLocked();
                }

                return existing;
            }

            existing = CreateInactiveState(id, name);
            _states.Add(existing);
            _states = _states.OrderBy(s => s.JobId).ToList();
            PersistLocked(); // Persist the new state to the file
            return existing;
        }
    }

    /// <summary>
    ///     Updates the state of the backup job with applied changes.
    /// </summary>
    /// <param name="state">The current job state to update.</param>
    /// <param name="apply">An action to modify the state.</param>
    public void UpdateState(BackupJobState state, Action<BackupJobState> apply)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(apply);

        lock (_sync)
        {
            EnsureInitializedLocked();

            var target = _states.FirstOrDefault(s => s.JobId == state.JobId);
            if (target == null)
            {
                target = state;
                _states.Add(target);
            }

            apply(target); // Apply changes through the provided action
            target.LastActionTimestamp = DateTime.Now; // Update last action timestamp
            _states = _states.OrderBy(s => s.JobId).ToList();
            PersistLocked(); // Persist updated state
        }
    }

    /// <summary>
    ///     Ensures all configured jobs have a state entry while preserving existing runtime entries.
    /// </summary>
    /// <param name="configuredJobs">Configured jobs from persistence.</param>
    private void SynchronizeConfiguredJobs(IReadOnlyList<BackupJob> configuredJobs)
    {
        foreach (var job in configuredJobs)
        {
            var existing = _states.FirstOrDefault(s => s.JobId == job.Id);
            if (existing != null)
            {
                existing.BackupName = job.Name;
                continue;
            }

            _states.Add(CreateInactiveState(job.Id, job.Name));
        }

        _states = _states.OrderBy(s => s.JobId).ToList();
    }

    /// <summary>
    ///     Persists current in-memory states to state.json.
    /// </summary>
    private void PersistLocked()
    {
        JsonFile.WriteAtomic(_statePath, _states);
    }

    /// <summary>
    ///     Validates that state storage has been initialized before usage.
    /// </summary>
    private void EnsureInitializedLocked()
    {
        if (_isInitialized)
            return;

        throw new InvalidOperationException(
            "State storage is not initialized. Call Initialize(stateDirectory) before accessing states.");
    }

    /// <summary>
    ///     Creates a fresh inactive state entry for the given job.
    /// </summary>
    /// <param name="jobId">Job identifier.</param>
    /// <param name="backupName">Backup display name.</param>
    /// <returns>Inactive state entry.</returns>
    private static BackupJobState CreateInactiveState(int jobId, string backupName)
    {
        return new BackupJobState
        {
            JobId = jobId,
            BackupName = backupName,
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
        };
    }
}
