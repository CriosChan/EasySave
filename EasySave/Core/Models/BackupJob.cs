using System.Text.Json.Serialization;

namespace EasySave.Core.Models;

/// <summary>
///     Describes a backup job.
/// </summary>
public sealed class BackupJob
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string SourceDirectory { get; private set; }
    public string TargetDirectory { get; private set; }
    public BackupType Type { get; private set; }

    [JsonConstructor]
    public BackupJob(int id, string name, string sourceDirectory, string targetDirectory, BackupType type)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be negative.");

        Name = ValidateNonEmpty(name, nameof(name));
        SourceDirectory = ValidateNonEmpty(sourceDirectory, nameof(sourceDirectory));
        TargetDirectory = ValidateNonEmpty(targetDirectory, nameof(targetDirectory));

        if (!Enum.IsDefined(typeof(BackupType), type))
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid backup type.");

        Id = id;
        Type = type;
    }

    public BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType type)
        : this(0, name, sourceDirectory, targetDirectory, type)
    {
    }

    public void AssignId(int id)
    {
        if (Id != 0)
            throw new InvalidOperationException("Id has already been assigned.");

        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be positive.");

        Id = id;
    }

    private static string ValidateNonEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty.", paramName);

        return value.Trim();
    }
}
