using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.Logging;

/// <summary>
///     Routes logs dynamically according to current general settings.
/// </summary>
public sealed class AdaptiveLogWriter<T> : ILogWriter<T>, IDisposable
{
    private readonly Func<string, ILogWriter<T>> _centralFactory;
    private readonly ILogWriter<T> _localWriter;
    private readonly IGeneralSettingsStore _settings;
    private readonly object _sync = new();
    private string? _currentEndpoint;
    private ILogWriter<T>? _currentRemoteWriter;
    private IDisposable? _currentRemoteWriterDisposable;

    public AdaptiveLogWriter(
        ILogWriter<T> localWriter,
        IGeneralSettingsStore settings,
        Func<string, ILogWriter<T>> centralFactory)
    {
        _localWriter = localWriter ?? throw new ArgumentNullException(nameof(localWriter));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _centralFactory = centralFactory ?? throw new ArgumentNullException(nameof(centralFactory));
    }

    public void Log(T entry)
    {
        var cfg = _settings.Current;
        var mode = (cfg.LogMode ?? string.Empty).Trim().ToLowerInvariant();
        var endpoint = (cfg.CentralLogEndpoint ?? string.Empty).Trim();

        if (mode != "centralized" && mode != "both")
        {
            _localWriter.Log(entry);
            return;
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _localWriter.Log(entry);
            return;
        }

        if (mode == "both")
            _localWriter.Log(entry);

        GetRemoteWriter(endpoint).Log(entry);
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _currentRemoteWriterDisposable?.Dispose();
            _currentRemoteWriterDisposable = null;
            _currentRemoteWriter = null;
            _currentEndpoint = null;
        }
    }

    private ILogWriter<T> GetRemoteWriter(string endpoint)
    {
        lock (_sync)
        {
            if (_currentRemoteWriter != null && string.Equals(_currentEndpoint, endpoint, StringComparison.OrdinalIgnoreCase))
                return _currentRemoteWriter;

            _currentRemoteWriterDisposable?.Dispose();
            _currentRemoteWriterDisposable = null;
            _currentRemoteWriter = _centralFactory(endpoint);
            _currentEndpoint = endpoint;
            _currentRemoteWriterDisposable = _currentRemoteWriter as IDisposable;
            return _currentRemoteWriter;
        }
    }
}
