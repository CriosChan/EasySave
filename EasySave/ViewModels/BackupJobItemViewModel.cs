using EasySave.Core.Models;

namespace EasySave.ViewModels;

public sealed class BackupJobItemViewModel
{
    public BackupJobItemViewModel(BackupJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        Id = job.Id;
        Name = job.Name;
        SourceDirectory = job.SourceDirectory;
        TargetDirectory = job.TargetDirectory;
        Type = job.Type;
    }

    public int Id { get; }
    public string Name { get; }
    public string SourceDirectory { get; }
    public string TargetDirectory { get; }
    public BackupType Type { get; }

    public string DisplayName => $"[{Id}] {Name}";
    public string DisplayPath => $"{SourceDirectory} -> {TargetDirectory}";

    public BackupJob ToModel()
    {
        return new BackupJob(Id, Name, SourceDirectory, TargetDirectory, Type);
    }
}
