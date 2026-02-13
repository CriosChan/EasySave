using System.Text.Json;
using System.Text.Json.Nodes;
using EasySave.Data.Configuration;

namespace EasySave.Models.BusinessSoftware;

/// <summary>
///     Saves and loads configured business software process names in appsettings.json.
/// </summary>
public sealed class BusinessSoftwareSettingsService : IBusinessSoftwareSettingsService
{
    private readonly string _appSettingsPath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BusinessSoftwareSettingsService" /> class.
    /// </summary>
    /// <param name="configFileName">Configuration file name.</param>
    public BusinessSoftwareSettingsService(string configFileName = "appsettings.json")
    {
        _appSettingsPath = Path.Combine(AppContext.BaseDirectory, configFileName);
    }

    /// <summary>
    ///     Loads the configured process names used to block backups.
    /// </summary>
    /// <returns>Read-only list of configured process names.</returns>
    public IReadOnlyList<string> LoadConfiguredProcessNames()
    {
        return ApplicationConfiguration.Load().BusinessSoftwareProcessNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    ///     Saves the configured process names used to block backups.
    /// </summary>
    /// <param name="processNames">Process names to persist.</param>
    public void SaveConfiguredProcessNames(IEnumerable<string> processNames)
    {
        if (processNames == null)
            throw new ArgumentNullException(nameof(processNames));

        var normalizedNames = processNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        JsonObject root;
        if (File.Exists(_appSettingsPath))
        {
            var content = File.ReadAllText(_appSettingsPath);
            root = JsonNode.Parse(content) as JsonObject ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        var processArray = new JsonArray();
        foreach (var name in normalizedNames)
            processArray.Add(name);

        root["BusinessSoftwareProcessNames"] = processArray;

        var json = root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Directory.CreateDirectory(Path.GetDirectoryName(_appSettingsPath) ?? ".");
        File.WriteAllText(_appSettingsPath, json);
    }
}
