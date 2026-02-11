using EasySave.Application.Abstractions;

namespace EasySave.Application.Services;

/// <summary>
///     Writes log entries to multiple outputs.
/// </summary>
public sealed class CompositeLogWriter<T> : ILogWriter<T>
{
    private readonly IReadOnlyList<ILogWriter<T>> _writers;

    public CompositeLogWriter(IEnumerable<ILogWriter<T>> writers)
    {
        _writers = (writers ?? throw new ArgumentNullException(nameof(writers))).ToList();
    }

    public void Log(T entry)
    {
        foreach (var writer in _writers)
            writer.Log(entry);
    }
}
