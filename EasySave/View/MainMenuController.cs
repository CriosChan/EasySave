using EasySave.View.Console;

namespace EasySave.View;

/// <summary>
/// Builds and displays the main interactive menu.
/// </summary>
internal sealed class MainMenuController
{
    private readonly IConsole _console;
    private readonly JobListView _jobList;
    private readonly JobCreationView _jobCreation;
    private readonly JobRemovalView _jobRemoval;
    private readonly JobLaunchView _jobLaunch;

    public MainMenuController(
        IConsole console,
        JobListView jobList,
        JobCreationView jobCreation,
        JobRemovalView jobRemoval,
        JobLaunchView jobLaunch)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _jobList = jobList ?? throw new ArgumentNullException(nameof(jobList));
        _jobCreation = jobCreation ?? throw new ArgumentNullException(nameof(jobCreation));
        _jobRemoval = jobRemoval ?? throw new ArgumentNullException(nameof(jobRemoval));
        _jobLaunch = jobLaunch ?? throw new ArgumentNullException(nameof(jobLaunch));
    }

    public void Show()
    {
        ListWidget.ShowList(
        [
            new Option(Ressources.UserInterface.Menu_ListJobs, _jobList.Show),
            new Option(Ressources.UserInterface.Menu_AddBackup, _jobCreation.Show),
            new Option(Ressources.UserInterface.Menu_RemoveBackup, _jobRemoval.Show),
            new Option(Ressources.UserInterface.Menu_LaunchBackupJob, _jobLaunch.Show)
        ], _console);
    }
}
