using EasySave.Core.Models;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.Logger;
using EasySave.Models.State;
using EasySave.Models.Utils;

namespace EasySave.Models.Backup;

/// <summary>
///     Handles backup interruption and logging when business software is detected.
/// </summary>
public sealed class BusinessSoftwareStopHandler
{
    private readonly string _backupName;
    private readonly IBusinessSoftwareMonitor _businessSoftwareMonitor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BusinessSoftwareStopHandler" /> class.
    /// </summary>
    /// <param name="businessSoftwareMonitor">Monitor used to detect running business software.</param>
    /// <param name="backupName">Name of the backup job being executed.</param>
    public BusinessSoftwareStopHandler(IBusinessSoftwareMonitor businessSoftwareMonitor, string backupName)
    {
        _businessSoftwareMonitor = businessSoftwareMonitor ?? throw new ArgumentNullException(nameof(businessSoftwareMonitor));
        _backupName = backupName.ValidateNonEmpty(nameof(backupName));
    }

    /// <summary>
    ///     Stops the current backup flow when business software is running and writes a stop log entry.
    /// </summary>
    /// <param name="state">Current job state to update.</param>
    /// <param name="blockedFile">
    ///     File that would have been processed next when the stop occurs.
    ///     Null when the backup is blocked before starting file processing.
    /// </param>
    /// <returns>True if backup execution must stop; otherwise, false.</returns>
    public bool ShouldStopBackup(BackupJobState state, IFile? blockedFile)
    {
        if (!_businessSoftwareMonitor.IsBusinessSoftwareRunning())
            return false;

        StateLogger.SetStateStoppedByBusinessSoftware(state);
        LogBusinessSoftwareStop(blockedFile);
        return true;
    }

    /// <summary>
    ///     Logs a dedicated entry describing a stop caused by business software detection.
    /// </summary>
    /// <param name="blockedFile">
    ///     File that was about to start when the stop was triggered, or null if blocked before file loop.
    /// </param>
    private void LogBusinessSoftwareStop(IFile? blockedFile)
    {
        var logger = new ConfigurableLogWriter<LogEntry>();
        var configuredSoftwareNames = _businessSoftwareMonitor.ConfiguredSoftwareNames;
        var softwareLabel = configuredSoftwareNames.Count == 0
            ? "configured business software"
            : string.Join(", ", configuredSoftwareNames);

        logger.Log(new LogEntry
        {
            BackupName = _backupName,
            SourcePath = blockedFile == null ? string.Empty : PathService.ToFullUncLikePath(blockedFile.SourceFile),
            TargetPath = blockedFile == null ? string.Empty : PathService.ToFullUncLikePath(blockedFile.TargetFile),
            FileSizeBytes = 0,
            TransferTimeMs = -1,
            ErrorMessage = $"Backup stopped because one of these business software processes is running: '{softwareLabel}'."
        });
    }
}
