using System.Diagnostics;
using EasySave.Core.Models;
using EasySave.Models.Logger;

namespace EasySave.Models.Backup.IO;

public class NormalFile : BaseFile
{
    /// <summary>
    ///     Initializes a new instance of the NormalFile class.
    /// </summary>
    /// <param name="sourceFile">The path of the source file to back up.</param>
    /// <param name="targetFile">The path where the backup file will be stored.</param>
    /// <param name="backupName">The name of the backup process.</param>
    /// <param name="logger">Optional shared log writer. A new instance is created when null.</param>
    public NormalFile(string sourceFile, string targetFile, string backupName,
        ConfigurableLogWriter<LogEntry>? logger = null)
        : base(sourceFile, targetFile, backupName, logger)
    {
    }

    /// <summary>
    ///     Copies the file from the source location to the target location.
    ///     Logs the operation details including file size and transfer time.
    /// </summary>
    public override void Copy()
    {
        string? errorMessage = null;

        var fileSize = GetSize();
        var sw = Stopwatch.StartNew();
        File.Copy(SourceFile, TargetFile, true);
        sw.Stop();

        Logger.Log(new LogEntry
        {
            BackupName = BackupName,
            SourcePath = SourceFile,
            TargetPath = TargetFile,
            FileSizeBytes = fileSize,
            TransferTimeMs = sw.ElapsedMilliseconds,
            ErrorMessage = errorMessage
        });
    }
}

