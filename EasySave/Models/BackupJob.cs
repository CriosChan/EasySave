namespace EasySave.Models;

public sealed class BackupJob
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string SourceDirectory { get; set; } = "";
    public string TargetDirectory { get; set; } = "";
    public BackupType Type { get; set; } = BackupType.Complete;
}
