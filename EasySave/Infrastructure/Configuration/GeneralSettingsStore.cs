using EasySave.Application.Abstractions;
using EasySave.Application.Models;
using EasySave.Infrastructure.IO;

namespace EasySave.Infrastructure.Configuration;

/// <summary>
///     Persists advanced runtime settings into general-settings.json.
/// </summary>
public sealed class GeneralSettingsStore : IGeneralSettingsStore
{
    private readonly object _sync = new();
    private readonly string _path;
    private GeneralSettings _settings;

    public GeneralSettingsStore(string configDirectory)
    {
        if (string.IsNullOrWhiteSpace(configDirectory))
            throw new ArgumentException("Configuration directory cannot be empty.", nameof(configDirectory));

        _path = Path.Combine(configDirectory, "general-settings.json");
        _settings = Normalize(JsonFile.ReadOrDefault(_path, new GeneralSettings()));
    }

    public GeneralSettings Current
    {
        get
        {
            lock (_sync)
            {
                return Clone(_settings);
            }
        }
    }

    public event EventHandler<GeneralSettings>? Changed;

    public void Save(GeneralSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        GeneralSettings normalized;
        lock (_sync)
        {
            normalized = Normalize(settings);
            _settings = normalized;
            JsonFile.WriteAtomic(_path, _settings);
        }

        Changed?.Invoke(this, Clone(normalized));
    }

    private static GeneralSettings Normalize(GeneralSettings settings)
    {
        settings.PriorityExtensions = NormalizeExtensions(settings.PriorityExtensions);
        settings.CryptoExtensions = NormalizeExtensions(settings.CryptoExtensions);
        settings.BusinessProcessName = (settings.BusinessProcessName ?? string.Empty).Trim();
        settings.CryptoSoftPath = (settings.CryptoSoftPath ?? string.Empty).Trim();
        settings.CryptoSoftArguments = string.IsNullOrWhiteSpace(settings.CryptoSoftArguments)
            ? "\"{0}\""
            : settings.CryptoSoftArguments.Trim();
        settings.CentralLogEndpoint = (settings.CentralLogEndpoint ?? string.Empty).Trim();
        settings.LogMode = NormalizeLogMode(settings.LogMode);
        settings.LargeFileThresholdKb = Math.Max(1, settings.LargeFileThresholdKb);
        settings.BusinessProcessCheckIntervalMs = Math.Max(100, settings.BusinessProcessCheckIntervalMs);
        return settings;
    }

    private static string NormalizeLogMode(string? mode)
    {
        var value = string.IsNullOrWhiteSpace(mode) ? "local" : mode.Trim().ToLowerInvariant();
        return value switch
        {
            "centralized" => "centralized",
            "both" => "both",
            _ => "local"
        };
    }

    private static List<string> NormalizeExtensions(IEnumerable<string>? extensions)
    {
        return (extensions ?? Enumerable.Empty<string>())
            .Select(x => (x ?? string.Empty).Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.StartsWith('.') ? x : "." + x)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static GeneralSettings Clone(GeneralSettings source)
    {
        return new GeneralSettings
        {
            PriorityExtensions = source.PriorityExtensions.ToList(),
            CryptoExtensions = source.CryptoExtensions.ToList(),
            LargeFileThresholdKb = source.LargeFileThresholdKb,
            BusinessProcessName = source.BusinessProcessName,
            EnableBusinessProcessMonitor = source.EnableBusinessProcessMonitor,
            BusinessProcessCheckIntervalMs = source.BusinessProcessCheckIntervalMs,
            CryptoSoftPath = source.CryptoSoftPath,
            CryptoSoftArguments = source.CryptoSoftArguments,
            LogMode = source.LogMode,
            CentralLogEndpoint = source.CentralLogEndpoint
        };
    }
}
