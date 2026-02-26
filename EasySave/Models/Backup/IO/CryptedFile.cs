using System.Diagnostics;
using EasySave.Core.Models;
using EasySave.Models.Logger;

namespace EasySave.Models.Backup.IO;

public class CryptedFile : BaseFile
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    private static readonly string CryptoSoftPath =
        Path.Combine(AppContext.BaseDirectory, "Tools", "CryptoSoft.exe");

    /// <summary>
    ///     Initializes a new instance of the CryptedFile class.
    /// </summary>
    /// <param name="sourceFile">The path of the source file to back up.</param>
    /// <param name="targetFile">The path where the backup file will be stored.</param>
    /// <param name="backupName">The name of the backup process.</param>
    /// <param name="logger">Optional shared log writer. A new instance is created when null.</param>
    public CryptedFile(string sourceFile, string targetFile, string backupName,
        ConfigurableLogWriter<LogEntry>? logger = null)
        : base(sourceFile, targetFile, backupName, logger)
    {
    }

    /// <summary>
    ///     Copies and encrypts the file synchronously via CryptoSoft.
    ///     Serializes access to CryptoSoft via a semaphore (one process at a time).
    /// </summary>
    public override void Copy()
    {
        string? errorMessage = null;
        var fileSize = GetSize();

        _semaphore.Wait();
        var sw = Stopwatch.StartNew(); // Start timer only after acquiring the semaphore
        try
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = CryptoSoftPath,
                    Arguments = $"\"{SourceFile}\" \"{TargetFile}\""
                }
            };
            process.Start();
            process.WaitForExit();
            sw.Stop();

            var log = new LogEntry
            {
                BackupName = BackupName,
                SourcePath = SourceFile,
                TargetPath = TargetFile,
                FileSizeBytes = fileSize,
                TransferTimeMs = sw.ElapsedMilliseconds,
                ErrorMessage = errorMessage,
                CryptingTimeMs = process.ExitCode != 0 ? process.ExitCode : sw.ElapsedMilliseconds
            };
            Logger.Log(log);
        }
        catch (Exception e)
        {
            sw.Stop();
            Logger.Log(new LogEntry
            {
                BackupName = BackupName,
                SourcePath = SourceFile,
                TargetPath = TargetFile,
                FileSizeBytes = fileSize,
                TransferTimeMs = sw.ElapsedMilliseconds,
                ErrorMessage = e.Message,
                CryptingTimeMs = -1
            });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Copies and encrypts the file asynchronously via CryptoSoft.
    ///     Awaits the semaphore and the CryptoSoft process without blocking a thread-pool thread.
    /// </summary>
    public override async Task CopyAsync()
    {
        var fileSize = GetSize();
        var sw = Stopwatch.StartNew();

        await _semaphore.WaitAsync();
        try
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = CryptoSoftPath,
                    Arguments = $"\"{SourceFile}\" \"{TargetFile}\""
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            sw.Stop();

            Logger.Log(new LogEntry
            {
                BackupName = BackupName,
                SourcePath = SourceFile,
                TargetPath = TargetFile,
                FileSizeBytes = fileSize,
                TransferTimeMs = sw.ElapsedMilliseconds,
                CryptingTimeMs = process.ExitCode != 0 ? process.ExitCode : sw.ElapsedMilliseconds
            });
        }
        catch (Exception e)
        {
            sw.Stop();
            Logger.Log(new LogEntry
            {
                BackupName = BackupName,
                SourcePath = SourceFile,
                TargetPath = TargetFile,
                FileSizeBytes = fileSize,
                TransferTimeMs = sw.ElapsedMilliseconds,
                ErrorMessage = e.Message,
                CryptingTimeMs = -1
            });
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

