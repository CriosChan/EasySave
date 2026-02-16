using System.Text.Json.Serialization;
using EasySave.Core.Models;
using EasySave.Data.Configuration;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.Data.Configuration;
using EasySave.Models.State;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup;

public class BackupJob
{
    [JsonIgnore] public long TotalSize;
    [JsonIgnore] public long TransferredSize;

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
    [JsonIgnore] public int FilesCount { get; private set; }

    [JsonIgnore]
    public double CurrentProgress
    {
        get;
        private set
        {
            field = value;
            OnProgressChanged();
        }
    } // Progress percentage of the backup job

    /// <summary>
    ///     Gets or sets the monitor used to detect business software activity.
    ///     Exposed for testability and dependency inversion.
    /// </summary>
    [JsonIgnore]
    public IBusinessSoftwareMonitor BusinessSoftwareMonitor { get; set; } = new BusinessSoftwareMonitor();

    /// <summary>
    ///     Gets a value indicating whether the current job run was stopped because business software was detected.
    /// </summary>
    [JsonIgnore]
    public bool WasStoppedByBusinessSoftware { get; private set; }

    public event EventHandler? ProgressChanged;

    /// <summary>
    ///     Checks if the source and target directories exist and are accessible.
    /// </summary>
    /// <param name="errorMessage">Error message if directories are not accessible.</param>
    /// <returns>True if both directories exist and are accessible, otherwise false.</returns>
    private bool Check(out string errorMessage)
    {
        errorMessage = string.Empty;
        
        // Check source directory
        if (!PathService.IsDirectoryAccessible(SourceDirectory, out var sourceError))
        {
            errorMessage = $"Source directory error: {sourceError}";
            Console.WriteLine($"[ERROR] {errorMessage}");
            return false;
        }

        // Check target directory
        if (!PathService.IsDirectoryAccessible(TargetDirectory, out var targetError))
        {
            errorMessage = $"Target directory error: {targetError}";
            Console.WriteLine($"[ERROR] {errorMessage}");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Starts the backup process.
    ///     Mirrors the folder structure, selects files to backup, and transfers them.
    ///     Logs the progress and state of the backup job.
    /// </summary>
    public void StartBackup()
    {
        WasStoppedByBusinessSoftware = false;
        // Save Key to file for cryptosoft just in case file doesn't exists
        CryptoSoftConfiguration.Load().Save();
        StateFileSingleton.Instance.Initialize(ApplicationConfiguration.Load().LogPath);
        var state = StateFileSingleton.Instance.GetOrCreate(Id, Name);
        var businessSoftwareStopHandler = new BusinessSoftwareStopHandler(BusinessSoftwareMonitor, Name);
        
        if (!Check(out var errorMessage))
        {
            Console.WriteLine($"[ERROR] Backup job '{Name}' (ID: {Id}) failed: {errorMessage}");
            StateLogger.SetStateFailed(state);
            return;
        }

        if (businessSoftwareStopHandler.ShouldStopBackup(state, null))
        {
            WasStoppedByBusinessSoftware = true;
            return;
        }

        // Create the backup folder structure
        new BackupFolder(SourceDirectory, TargetDirectory, Name).MirrorFolder();
        var selector = TypeSelectorHelper.GetSelector(Type, SourceDirectory, TargetDirectory, Name);
        Files = selector.GetFilesToBackup();
        TotalSize = Files.GetAllSize();
        TransferredSize = 0;
        FilesCount = Files.Count();
        var hadError = false;

        // Set the state of the backup as active
        StateLogger.SetStateActive(state, FilesCount, TotalSize);
        for (var i = 0; i < FilesCount; i++)
        {
            if (i > 0 && businessSoftwareStopHandler.ShouldStopBackup(state, Files[i]))
            {
                WasStoppedByBusinessSoftware = true;
                break;
            }

            var i1 = i;
            StateLogger.SetStateStartTransfer(state, Files[i1]);
            CurrentFileIndex = i1;
            try
            {
                Files[i1].Copy(); // Execute the file copy
            }
            catch (Exception)
            {
                hadError = true; // Log an error if the copy fails
            }

            TransferredSize += Files[i1].GetSize();
            CurrentProgress = MathUtil.Percentage(TransferredSize, TotalSize);

            // Log the end of the transfer for the current file
            StateLogger.SetStateEndTransfer(state, FilesCount, i1, TotalSize, TransferredSize, CurrentProgress);
        }

        if (WasStoppedByBusinessSoftware)
            return;

        // Finalize the backup job state
        StateLogger.SetStateEnd(state, hadError);
        CurrentProgress = 100; // Set progress to 100% at completion
    }

    protected virtual void OnProgressChanged()
    {
        ProgressChanged?.Invoke(this, EventArgs.Empty);
    }
}