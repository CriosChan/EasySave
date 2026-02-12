namespace EasySave.Core.Models;

/// <summary>
///     Describes the execution state of a backup job.
/// </summary>
public sealed class BackupJobState
{
    public int JobId { get; set; }
    public string BackupName { get; set; } = "";
    public DateTime LastActionTimestamp { get; set; }
    public JobRunState State { get; set; } = JobRunState.Inactive;

    public int TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }

    // Progress as a percentage (0..100)
    public double ProgressPercent { get; set; }

    public int RemainingFiles { get; set; }
    public long RemainingSizeBytes { get; set; }

    public string? CurrentSourcePath { get; set; }
    public string? CurrentTargetPath { get; set; }
    public string? CurrentAction { get; set; }
}