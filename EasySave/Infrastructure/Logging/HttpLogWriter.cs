using System.Net.Http.Json;
using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.Logging;

/// <summary>
///     Sends each log entry to a centralized HTTP endpoint.
/// </summary>
public sealed class HttpLogWriter<T> : ILogWriter<T>, IDisposable
{
    private readonly HttpClient _client;
    private readonly string _endpoint;

    public HttpLogWriter(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Central log endpoint must be provided.", nameof(endpoint));

        _endpoint = endpoint.Trim();
        _client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public void Log(T entry)
    {
        try
        {
            _client.PostAsJsonAsync(_endpoint, entry).GetAwaiter().GetResult();
        }
        catch
        {
            // Centralized logging is best effort and must not block backups.
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
