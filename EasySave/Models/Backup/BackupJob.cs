using System.Text.Json.Serialization;
using EasySave.Core.Models;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Facade that aggregates the four SRP sub-components of a backup job.
///     Public surface is unchanged so all consumers (ViewModels, engines, repositories) require no modification.
/// </summary>
public sealed class BackupJob
{
    private readonly BackupJobIdentity _identity;
    private readonly BackupProgressTracker _progressTracker;
    private readonly BackupJobController _controller;
    private readonly BackupTransferOrchestrator _transferOrchestrator;

    /// <summary>
    ///     Initializes a new instance of the BackupJob class with a specified ID.
    /// </summary>
    [JsonConstructor]
    public BackupJob(int id, string name, string sourceDirectory, string targetDirectory, BackupType type)
    {
        _identity = new BackupJobIdentity(id, name, sourceDirectory, targetDirectory, type);
        _progressTracker = new BackupProgressTracker();
        _controller = new BackupJobController();
        _transferOrchestrator = new BackupTransferOrchestrator(_identity, _progressTracker, _controller);

        // Forward sub-component events to facade events
        _progressTracker.ProgressChanged += (_, e) => ProgressChanged?.Invoke(this, e);
        _progressTracker.FilesCountEvent += (_, e) => FilesCountEvent?.Invoke(this, e);
        _controller.PauseEvent += (_, e) => PauseEvent?.Invoke(this, e);
        _controller.StopEvent += (_, e) => StopEvent?.Invoke(this, e);
        _transferOrchestrator.EndEvent += (_, e) => EndEvent?.Invoke(this, e);
    }

    /// <summary>
    ///     Initializes a new instance of the BackupJob class without an ID.
    /// </summary>
    public BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType type)
        : this(0, name, sourceDirectory, targetDirectory, type)
    {
    }

    // --- Identity pass-through ---

    /// <summary> Unique identifier for the backup job. </summary>
    public int Id
    {
        get => _identity.Id;
        set => _identity.Id = value;
    }

    /// <summary> Name of the backup job. </summary>
    public string Name => _identity.Name;

    /// <summary> Source directory to back up. </summary>
    public string SourceDirectory => _identity.SourceDirectory;

    /// <summary> Target directory for backup storage. </summary>
    public string TargetDirectory => _identity.TargetDirectory;

    /// <summary> Type of backup. </summary>
    public BackupType Type => _identity.Type;

    /// <summary> List of files to backup. </summary>
    [JsonIgnore]
    public List<IFile> Files
    {
        get => _identity.Files;
        set => _identity.Files = value;
    }

    // --- Progress pass-through ---

    /// <summary> The total size of files to be backed up. </summary>
    [JsonIgnore]
    public long TotalSize
    {
        get => _progressTracker.TotalSize;
        set => _progressTracker.TotalSize = value;
    }

    /// <summary> The size of files that have been transferred. </summary>
    [JsonIgnore]
    public long TransferredSize
    {
        get => _progressTracker.TransferredSize;
        set => _progressTracker.TransferredSize = value;
    }

    /// <summary> Current progress percentage. </summary>
    [JsonIgnore]
    public double CurrentProgress => _progressTracker.CurrentProgress;

    /// <summary> Total number of files in the backup. </summary>
    [JsonIgnore]
    public int FilesCount => _progressTracker.FilesCount;

    /// <summary> Index of the currently processed file. </summary>
    [JsonIgnore]
    public int CurrentFileIndex => _progressTracker.CurrentFileIndex;

    // --- Controller pass-through ---

    /// <summary> Gets a value indicating whether the job was stopped. </summary>
    [JsonIgnore]
    public bool WasStopped => _controller.WasStopped;

    /// <summary> Gets or sets a value indicating whether the job was stopped by business-software detection. </summary>
    [JsonIgnore]
    public bool WasStoppedByBusinessSoftware
    {
        get => _controller.WasStoppedByBusinessSoftware;
        set => _controller.WasStoppedByBusinessSoftware = value;
    }

    // --- Transfer orchestrator dependencies ---

    /// <summary>
    ///     Gets or sets the monitor used to detect business software activity.
    /// </summary>
    [JsonIgnore]
    public IBusinessSoftwareMonitor BusinessSoftwareMonitor
    {
        get => _transferOrchestrator.BusinessSoftwareMonitor;
        set => _transferOrchestrator.BusinessSoftwareMonitor = value;
    }

    /// <summary>
    ///     Gets or sets the global priority arbitrator shared across all jobs.
    /// </summary>
    [JsonIgnore]
    public IPriorityArbitrator? PriorityArbitrator
    {
        get => _transferOrchestrator.PriorityArbitrator;
        set => _transferOrchestrator.PriorityArbitrator = value;
    }

    // --- Events (add/remove pass-through) ---

    public event EventHandler? ProgressChanged;
    public event EventHandler? FilesCountEvent;
    public event EventHandler? PauseEvent;
    public event EventHandler? StopEvent;
    public event EventHandler? EndEvent;

    // --- Methods pass-through ---

    /// <summary> Starts the backup process. </summary>
    public void StartBackup() => _transferOrchestrator.Execute();

    /// <summary> Pauses the backup job. </summary>
    public void Pause() => _controller.Pause();

    /// <summary> Resumes the backup job. </summary>
    public void Resume() => _controller.Resume();

    /// <summary> Stops the backup job immediately. </summary>
    public void Stop() => _controller.Stop();

    /// <summary> Checks if the backup job is currently paused. </summary>
    public bool IsPaused() => _controller.IsPaused();
}

