using System.Diagnostics;
using EasySave.Core.Models;
using EasySave.Models.Backup.Abstractions;
using EasySave.Models.Logger;

namespace EasySave.Models.Backup.IO;

public class CryptedFile : IFile
{
    
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    /// <summary>
    ///     Initializes a new instance of the CryptedFile class.
    /// </summary>
    /// <param name="sourceFile">The path of the source file to back up.</param>
    /// <param name="targetFile">The path where the backup file will be stored.</param>
    /// <param name="backupName">The name of the backup process.</param>
    public CryptedFile(string sourceFile, string targetFile, string backupName)
    {
        SourceFile = sourceFile;
        TargetFile = targetFile;
        BackupName = backupName;
    }

    public string BackupName { get; }

    // Properties for source and target file paths
    public string SourceFile { get; } // Read-only property for the source file path
    public string TargetFile { get; } // Read-only property for the target file path

    /// <summary>
    ///     Copies the file from the source location to the target location.
    ///     Logs the operation details including file size and transfer time.
    /// </summary>
    public void Copy()
    {
        var logger = new ConfigurableLogWriter<LogEntry>();
        long fileSize; // Size of the file to be copied
        long elapsedMs; // Time taken to copy the file
        string? errorMessage = null; // Placeholder for any error messages

        var fi = new FileInfo(SourceFile);
        fileSize = fi.Length; // Get the length of the file
        var sw = Stopwatch.StartNew(); // Start the stopwatch for timing the operation
        _semaphore.Wait();
        try
        {
            // Copy the file
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "Tools/CryptoSoft.exe",
                    Arguments = $"\"{SourceFile}\" \"{TargetFile}\""
                }
            };
            process.Start();
            process.WaitForExit();
            sw.Stop();
            elapsedMs = sw.ElapsedMilliseconds; // Get elapsed time in milliseconds
            var log = new LogEntry
            {
                BackupName = BackupName,
                SourcePath = SourceFile,
                TargetPath = TargetFile,
                FileSizeBytes = fileSize,
                TransferTimeMs = elapsedMs,
                ErrorMessage = errorMessage, // Log any error messages (currently unused)
                CryptingTimeMs = process.ExitCode
            };
            if (process.ExitCode != 0)
                log.CryptingTimeMs = process.ExitCode;
            else
                log.CryptingTimeMs = elapsedMs;
            logger.Log(log);
        }
        catch (Exception e)
        {
            sw.Stop();
            var log = new LogEntry
            {
                BackupName = BackupName,
                SourcePath = SourceFile,
                TargetPath = TargetFile,
                FileSizeBytes = fileSize,
                TransferTimeMs = sw.ElapsedMilliseconds,
                ErrorMessage = e.Message, // Log any error messages (currently unused)
                CryptingTimeMs = -1
            };
            logger.Log(log);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Gets the size of the source file in bytes.
    /// </summary>
    /// <returns>The size of the file in bytes.</returns>
    public long GetSize()
    {
        return new FileInfo(SourceFile).Length; // Return the size of the source file
    }

    /// <summary>
    ///     Copies and encrypts the file asynchronously via CryptoSoft.
    ///     Awaits the semaphore and the CryptoSoft process without blocking a thread-pool thread.
    /// </summary>
    public async Task CopyAsync()
    {
        var logger = new ConfigurableLogWriter<LogEntry>();
        var fi = new FileInfo(SourceFile);
        long fileSize = fi.Length;
        var sw = Stopwatch.StartNew();

        await _semaphore.WaitAsync();
        try
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "Tools/CryptoSoft.exe",
                    Arguments = $"\"{SourceFile}\" \"{TargetFile}\""
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            sw.Stop();

            var log = new LogEntry
            {
                BackupName = BackupName,
                SourcePath = SourceFile,
                TargetPath = TargetFile,
                FileSizeBytes = fileSize,
                TransferTimeMs = sw.ElapsedMilliseconds,
                CryptingTimeMs = process.ExitCode != 0 ? process.ExitCode : sw.ElapsedMilliseconds
            };
            logger.Log(log);
        }
        catch (Exception e)
        {
            sw.Stop();
            logger.Log(new LogEntry
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