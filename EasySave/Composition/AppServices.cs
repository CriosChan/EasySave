using EasySave.Core.Contracts;
using EasySave.Services;

namespace EasySave.Composition;

internal sealed class AppServices
{
    public required IJobService JobService { get; init; }
    public required IBackupService BackupService { get; init; }
    public required IStateService StateService { get; init; }
    public required IJobValidator JobValidator { get; init; }
    public required IStateSynchronizer StateSynchronizer { get; init; }
    public required IUserPreferences UserPreferences { get; init; }
    public required ILocalizationApplier LocalizationApplier { get; init; }
    public required IPathService PathService { get; init; }
    public UiProgressReporter? UiProgressReporter { get; init; }
}
