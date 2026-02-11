using EasySave.Application.Models;

namespace EasySave.Application.Abstractions;

/// <summary>
///     Persists and exposes advanced runtime settings.
/// </summary>
public interface IGeneralSettingsStore
{
    GeneralSettings Current { get; }
    event EventHandler<GeneralSettings>? Changed;
    void Save(GeneralSettings settings);
}
