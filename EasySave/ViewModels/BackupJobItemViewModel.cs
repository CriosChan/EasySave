using EasySave.Models.Backup;

namespace EasySave.ViewModels;

/// <summary>
///     ViewModel wrapper for BackupJob model, providing UI-specific properties.
/// </summary>
public sealed class BackupJobItemViewModel : ViewModelBase
{
    private readonly BackupJob _job;

    /// <summary>
    ///     Initializes a new instance of the BackupJobItemViewModel class.
    /// </summary>
    /// <param name="job">The BackupJob model to wrap.</param>
    public BackupJobItemViewModel(BackupJob job)
    {
        _job = job ?? throw new ArgumentNullException(nameof(job));
    }

    /// <summary>
    ///     Gets the underlying BackupJob model.
    /// </summary>
    public BackupJob Job => _job;

    /// <summary>
    ///     Gets the display name for the job.
    /// </summary>
    public string DisplayName => $"[{_job.Id}] {_job.Name}";

    /// <summary>
    ///     Gets the backup type as a string.
    /// </summary>
    public string Type => _job.Type.ToString();

    /// <summary>
    ///     Gets the formatted path display (Source → Target).
    /// </summary>
    public string DisplayPath => $"{_job.SourceDirectory} → {_job.TargetDirectory}";
}
