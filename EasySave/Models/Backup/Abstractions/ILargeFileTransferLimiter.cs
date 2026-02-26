namespace EasySave.Models.Backup.Abstractions;

/// <summary>
///     Coordinates global access to large-file transfers across all running backup jobs.
/// </summary>
public interface ILargeFileTransferLimiter
{
    /// <summary>
    ///     Determines whether a file must use the exclusive large-file transfer slot.
    /// </summary>
    /// <param name="fileSizeBytes">Size of the candidate file in bytes.</param>
    /// <returns>True when exclusive transfer is required; otherwise, false.</returns>
    bool RequiresExclusiveSlot(long fileSizeBytes);

    /// <summary>
    ///     Attempts to acquire the exclusive large-file transfer slot.
    /// </summary>
    /// <param name="timeout">Maximum wait time before giving up.</param>
    /// <returns>True if the slot was acquired; otherwise, false.</returns>
    bool TryAcquireExclusiveSlot(TimeSpan timeout);

    /// <summary>
    ///     Releases the previously acquired large-file transfer slot.
    /// </summary>
    void ReleaseExclusiveSlot();
}
