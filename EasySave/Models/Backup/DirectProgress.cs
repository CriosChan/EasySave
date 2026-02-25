namespace EasySave.Models.Backup;

/// <summary>
///     Invokes the progress callback synchronously on the calling thread,
///     bypassing any <see cref="SynchronizationContext" /> capture.
///     Use this when the caller already handles thread dispatching (e.g., Dispatcher.UIThread.Post).
/// </summary>
/// <typeparam name="T">Type of the progress value.</typeparam>
public sealed class DirectProgress<T>(Action<T> handler) : IProgress<T>
{
    /// <inheritdoc />
    public void Report(T value)
    {
        handler(value);
    }
}