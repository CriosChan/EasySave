using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasySave.Data.Configuration;

/// <summary>
///     Persists mutable application settings in appsettings.json.
/// </summary>
public sealed class ApplicationSettingsService
{
    private readonly string _appSettingsPath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationSettingsService" /> class.
    /// </summary>
    /// <param name="configFileName">Configuration file name.</param>
    public ApplicationSettingsService(string configFileName = "appsettings.json")
    {
        _appSettingsPath = Path.Combine(AppContext.BaseDirectory, configFileName);
    }

    /// <summary>
    ///     Saves the default localization value.
    /// </summary>
    /// <param name="localization">Localization value (for example, fr-FR or en-US).</param>
    public void SetLocalization(string localization)
    {
        if (string.IsNullOrWhiteSpace(localization))
            throw new ArgumentException("Localization cannot be empty.", nameof(localization));

        Update(root => root["Localization"] = localization);
    }

    /// <summary>
    ///     Saves the log output type.
    /// </summary>
    /// <param name="logType">Log type value (json or xml).</param>
    public void SetLogType(string logType)
    {
        if (string.IsNullOrWhiteSpace(logType))
            throw new ArgumentException("Log type cannot be empty.", nameof(logType));

        Update(root => root["LogType"] = logType);
    }

    /// <summary>
    ///     Updates appsettings.json by applying a mutation on the JSON root node.
    /// </summary>
    /// <param name="mutate">Mutation action.</param>
    private void Update(Action<JsonObject> mutate)
    {
        if (mutate == null)
            throw new ArgumentNullException(nameof(mutate));

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

        mutate(root);

        var json = root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Directory.CreateDirectory(Path.GetDirectoryName(_appSettingsPath) ?? ".");
        File.WriteAllText(_appSettingsPath, json);
    }
}
