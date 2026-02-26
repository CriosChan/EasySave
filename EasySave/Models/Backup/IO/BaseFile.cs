using EasySave.Core.Models;
using EasySave.Models.Logger;

namespace EasySave.Models.Backup.IO;

/// <summary>
///     Base class for backup file implementations.
///     Holds common properties and shared helper logic for <see cref="NormalFile"/> and <see cref="CryptedFile"/>.
/// </summary>
public abstract class BaseFile : IFile
{
    /// <summary>
    ///     Log writer shared by the instance. Injected or created on first use.
    /// </summary>
    protected readonly ConfigurableLogWriter<LogEntry> Logger;

    /// <summary>
    ///     Initializes the base file with paths, a backup name, and an optional logger.
    /// </summary>
    /// <param name="sourceFile">The path of the source file to back up.</param>
    /// <param name="targetFile">The path where the backup file will be stored.</param>
    /// <param name="backupName">The name of the backup process.</param>
    /// <param name="logger">Optional shared log writer. A new instance is created when null.</param>
    protected BaseFile(string sourceFile, string targetFile, string backupName,
        ConfigurableLogWriter<LogEntry>? logger = null)
    {
        SourceFile = sourceFile;
        TargetFile = targetFile;
        BackupName = backupName;
        Logger = logger ?? new ConfigurableLogWriter<LogEntry>();
    }

    /// <inheritdoc />
    public string SourceFile { get; }

    /// <inheritdoc />
    public string TargetFile { get; }

    /// <summary>Gets the name of the backup job this file belongs to.</summary>
    public string BackupName { get; }

    /// <inheritdoc />
    public abstract void Copy();

    /// <summary>
    ///     Copies the file asynchronously. Offloads the synchronous <see cref="Copy"/> to a thread-pool thread
    ///     unless overridden by a subclass.
    /// </summary>
    public virtual Task CopyAsync() => Task.Run(Copy);

    /// <inheritdoc />
    public long GetSize() => new FileInfo(SourceFile).Length;
}


