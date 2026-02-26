using EasySave.Data.Configuration;
using EasySave.Models.Backup.Interfaces;

namespace EasySave.Models.Backup;

/// <summary>
///     Enforces the v3 rule that only one large file (> n Ko) can be transferred at a time.
/// </summary>
public sealed class GlobalLargeFileTransferLimiter : ILargeFileTransferLimiter
{
    private readonly SemaphoreSlim _exclusiveLargeFileSemaphore = new(1, 1);
    private readonly Func<int> _thresholdKoProvider;

    /// <summary>
    ///     Shared limiter instance used by default when no custom limiter is injected.
    /// </summary>
    public static GlobalLargeFileTransferLimiter Shared { get; } = new();

    /// <summary>
    ///     Initializes a new instance of <see cref="GlobalLargeFileTransferLimiter" />.
    /// </summary>
    /// <param name="thresholdKoProvider">
    ///     Optional provider returning the current threshold in Ko.
    ///     Defaults to <see cref="ApplicationConfiguration.LargeFileThresholdKo" />.
    /// </param>
    public GlobalLargeFileTransferLimiter(Func<int>? thresholdKoProvider = null)
    {
        _thresholdKoProvider = thresholdKoProvider ?? (() => ApplicationConfiguration.Load().LargeFileThresholdKo);
    }

    /// <inheritdoc />
    public bool RequiresExclusiveSlot(long fileSizeBytes)
    {
        if (fileSizeBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(fileSizeBytes), "File size cannot be negative.");

        return fileSizeBytes > GetThresholdBytes();
    }

    /// <inheritdoc />
    public bool TryAcquireExclusiveSlot(TimeSpan timeout)
    {
        if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be non-negative or infinite.");

        return _exclusiveLargeFileSemaphore.Wait(timeout);
    }

    /// <inheritdoc />
    public void ReleaseExclusiveSlot()
    {
        _exclusiveLargeFileSemaphore.Release();
    }

    /// <summary>
    ///     Converts the configured threshold (Ko) into bytes.
    /// </summary>
    private long GetThresholdBytes()
    {
        var thresholdKo = Math.Max(1, _thresholdKoProvider());
        return thresholdKo * 1024L;
    }
}
