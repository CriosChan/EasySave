using EasySave.Models;
using EasySave.Utils;

namespace EasySave.Services;

/// <summary>
/// Persists user-specific settings to a JSON file (userSettings.json).
/// </summary>
/// <remarks>
/// In v1.x the settings file currently stores the preferred UI language.
/// The format is expected to evolve in future versions, therefore the repository keeps a
/// small, stable API.
/// </remarks>
public sealed class SettingsRepository
{
    private readonly string _settingsPath;

    /// <summary>
    /// Creates a repository that stores settings under the given configuration directory.
    /// </summary>
    /// <param name="configDirectory">Absolute directory where userSettings.json will be stored.</param>
    public SettingsRepository(string configDirectory)
    {
        _settingsPath = Path.Combine(configDirectory, "userSettings.json");
    }

    /// <summary>
    /// Loads settings from disk.
    /// </summary>
    public UserSettings Load()
    {
        return JsonFile.ReadOrDefault(_settingsPath, new UserSettings());
    }

    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    /// <param name="settings">Settings to write.</param>
    public void Save(UserSettings settings)
    {
        JsonFile.WriteAtomic(_settingsPath, settings);
    }
}
