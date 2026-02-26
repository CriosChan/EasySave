using System.Text.Json.Serialization;
using EasySave.Core.Models;
using EasySave.Models.Backup.Abstractions;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup.Entities;

/// <summary>
///     Holds the immutable identity and configuration data of a backup job.
/// </summary>
public sealed class BackupJobIdentity
{
    /// <summary>
    ///     Initializes a new instance of <see cref="BackupJobIdentity" /> with an explicit identifier.
    /// </summary>
    /// <param name="id">Unique identifier. Cannot be negative.</param>
    /// <param name="name">Display name.</param>
    /// <param name="sourceDirectory">Source directory to back up.</param>
    /// <param name="targetDirectory">Target directory for backup storage.</param>
    /// <param name="type">Backup type.</param>
    [JsonConstructor]
    public BackupJobIdentity(int id, string name, string sourceDirectory, string targetDirectory, BackupType type)
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
    ///     Initializes a new instance of <see cref="BackupJobIdentity" /> without an explicit identifier.
    /// </summary>
    public BackupJobIdentity(string name, string sourceDirectory, string targetDirectory, BackupType type)
        : this(0, name, sourceDirectory, targetDirectory, type)
    {
    }

    /// <summary> Gets or sets the unique identifier for the backup job. </summary>
    public int Id { get; set; }

    /// <summary> Gets the display name of the backup job. </summary>
    public string Name { get; }

    /// <summary> Gets the source directory to back up. </summary>
    public string SourceDirectory { get; }

    /// <summary> Gets the target directory for backup storage. </summary>
    public string TargetDirectory { get; }

    /// <summary> Gets the type of backup. </summary>
    public BackupType Type { get; }

    /// <summary> Gets or sets the list of files to backup. </summary>
    [JsonIgnore]
    public List<IFile> Files { get; set; } = [];
}

