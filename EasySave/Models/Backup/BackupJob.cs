using System.Text.Json.Serialization;
using EasySave.Core.Models;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.State;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup;

public class BackupJob
{
    /// <summary>
    ///     Initializes a new instance of the BackupJob class with an ID.
    /// </summary>
    /// <param name="id">Unique identifier for the backup job. Cannot be negative.</param>
    /// <param name="name">Name of the backup job.</param>
    /// <param name="sourceDirectory">Source directory to backup.</param>
    /// <param name="targetDirectory">Target directory for backup storage.</param>
    /// <param name="type">Type of backup.</param>
    [JsonConstructor]
    public BackupJob(int id, string name, string sourceDirectory, string targetDirectory, BackupType type)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be negative.");

        Name = name.ValidateNonEmpty(nameof(name));
        SourceDirectory = sourceDirectory.ValidateNonEmpty(nameof(sourceDirectory));
        TargetDirectory = targetDirectory.ValidateNonEmpty(nameof(targetDirectory));

        if (!Enum.IsDefined(typeof(BackupType), type))
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid backup type.");

        Id = id;
        Type = type;
    }

    /// <summary>
    ///     Initializes a new instance of the BackupJob class without an ID.
    /// </summary>
    /// <param name="name">Name of the backup job.</param>
    /// <param name="sourceDirectory">Source directory to backup.</param>
    /// <param name="targetDirectory">Target directory for backup storage.</param>
    /// <param name="type">Type of backup.</param>
    public BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType type)
        : this(0, name, sourceDirectory, targetDirectory, type)
    {
    }

    // Properties for the backup job
    public int Id { get; set; } // Unique identifier for the backup job
    public string Name { get; } // Name of the backup job
    public string SourceDirectory { get; } // Source directory
    public string TargetDirectory { get; } // Target directory
    public BackupType Type { get; } // Backup type

    [JsonIgnore] public List<IFile> Files { get; private set; } = []; // List of files to backup
    [JsonIgnore] public int CurrentFileIndex { get; private set; } // Index of the currently processed file
    [JsonIgnore] public double CurrentProgress { get; private set; } // Progress percentage of the backup job

    /// <summary>
    ///     Checks if the source and target directories exist.
    /// </summary>
    /// <returns>True if both directories exist, otherwise false.</returns>
    private bool Check()
    {
        return Directory.Exists(SourceDirectory) && Directory.Exists(TargetDirectory);
    }

    /// <summary>
    ///     Starts the backup process.
    ///     Mirrors the folder structure, selects files to backup, and transfers them.
    ///     Logs the progress and state of the backup job.
    /// </summary>
    public void StartBackup()
    {
        var state = StateFileSingleton.Instance.GetOrCreate(Id, Name);
        if (!Check())
        {
            StateLogger.SetStateFailed(state);
            return;
        }

        // Create the backup folder structure
        new BackupFolder(SourceDirectory, TargetDirectory, Name).MirrorFolder();
        var selector = TypeSelectorHelper.GetSelector(Type, SourceDirectory, TargetDirectory, Name);
        Files = selector.GetFilesToBackup();
        var totalSize = Files.GetAllSize();
        long transferredSize = 0;
        var filesCount = Files.Count();
        var hadError = false;

        // Set the state of the backup as active
        StateLogger.SetStateActive(state, filesCount, totalSize);
        for (var i = 0; i < filesCount; i++)
        {
            var i1 = i;
            StateLogger.SetStateStartTransfer(state, Files[i1]);
            CurrentFileIndex = i1;
            try
            {
                Files[i1].Copy(); // Execute the file copy
            }
            catch (Exception e)
            {
                hadError = true; // Log an error if the copy fails
            }

            transferredSize += Files[i1].GetSize();
            CurrentProgress = MathUtil.Percentage(transferredSize, totalSize);

            // Log the end of the transfer for the current file
            StateLogger.SetStateEndTransfer(state, filesCount, i1, totalSize, transferredSize, CurrentProgress);
        }

        // Finalize the backup job state
        StateLogger.SetStateEnd(state, hadError);
        CurrentProgress = 100; // Set progress to 100% at completion
    }
}