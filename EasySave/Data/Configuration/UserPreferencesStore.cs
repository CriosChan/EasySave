using EasySave.Core.Contracts;
using EasySave.Platform.IO;

namespace EasySave.Data.Configuration;

/// <summary>
///     Persists and exposes user preferences (log type, localization).
/// </summary>
public sealed class UserPreferencesStore : IUserPreferences
{
    private readonly string _path;
    private readonly object _sync = new();
    private UserPreferencesData _data;

    public UserPreferencesStore(string configDirectory, string defaultLocalization, string defaultLogType)
    {
        if (string.IsNullOrWhiteSpace(configDirectory))
            throw new ArgumentException("Config directory cannot be empty.", nameof(configDirectory));

        _path = Path.Combine(configDirectory, "user-preferences.json");
        _data = LoadOrDefault(defaultLocalization, defaultLogType);
    }

    public string LogType => _data.LogType;

    public string Localization => _data.Localization;

    public void SetLogType(string logType)
    {
        var normalized = NormalizeLogType(logType);
        lock (_sync)
        {
            if (string.Equals(_data.LogType, normalized, StringComparison.Ordinal))
                return;

            _data.LogType = normalized;
            Save();
        }
    }

    public void SetLocalization(string localization)
    {
        var normalized = NormalizeLocalization(localization);
        lock (_sync)
        {
            if (string.Equals(_data.Localization, normalized, StringComparison.Ordinal))
                return;

            _data.Localization = normalized;
            Save();
        }
    }

    private UserPreferencesData LoadOrDefault(string defaultLocalization, string defaultLogType)
    {
        var defaults = new UserPreferencesData
        {
            LogType = NormalizeLogType(defaultLogType),
            Localization = NormalizeLocalization(defaultLocalization)
        };

        var loaded = JsonFile.ReadOrDefault(_path, defaults);
        loaded.LogType = NormalizeLogType(loaded.LogType);
        loaded.Localization = NormalizeLocalization(loaded.Localization);
        return loaded;
    }

    private void Save()
    {
        JsonFile.WriteAtomic(_path, _data);
    }

    private static string NormalizeLogType(string? logType)
    {
        if (string.IsNullOrWhiteSpace(logType))
            return "json";

        var normalized = logType.Trim().ToLowerInvariant();
        return normalized == "xml" ? "xml" : "json";
    }

    private static string NormalizeLocalization(string? localization)
    {
        return string.IsNullOrWhiteSpace(localization) ? string.Empty : localization.Trim();
    }

    private sealed class UserPreferencesData
    {
        public string LogType { get; set; } = "json";
        public string Localization { get; set; } = "";
    }
}
