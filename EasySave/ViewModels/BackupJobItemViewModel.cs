using EasySave.Models.Backup;

namespace EasySave.ViewModels;

/// <summary>
///     ViewModel wrapper for BackupJob model, providing UI-specific properties.
/// </summary>
public sealed class BackupJobItemViewModel : ViewModelBase
{
    /// <summary>
    ///     Initializes a new instance of the BackupJobItemViewModel class.
    /// </summary>
    /// <param name="job">The BackupJob model to wrap.</param>
    public BackupJobItemViewModel(BackupJob job)
    {
        Job = job ?? throw new ArgumentNullException(nameof(job));
    }

    /// <summary>
    ///     Gets the underlying BackupJob model.
    /// </summary>
    public BackupJob Job { get; }

    /// <summary>
    ///     Gets the display name for the job.
    /// </summary>
    public string DisplayName => $"[{Job.Id}] {Job.Name}";

    /// <summary>
    ///     Gets the backup type as a string.
    /// </summary>
    public string Type => Job.Type.ToString();

    /// <summary>
    ///     Gets the formatted path display (Source → Target).
    /// </summary>
    public string DisplayPath => $"{Job.SourceDirectory} → {Job.TargetDirectory}";
}