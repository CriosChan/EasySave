using EasySave.Presentation.Resources;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

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

    /// <summary>
    /// Construit le controleur du menu principal.
    /// </summary>
    /// <param name="console">Console cible.</param>
    /// <param name="jobList">Vue de liste des jobs.</param>
    /// <param name="jobCreation">Vue de creation.</param>
    /// <param name="jobRemoval">Vue de suppression.</param>
    /// <param name="jobLaunch">Vue de lancement.</param>
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

    /// <summary>
    /// Affiche le menu principal et gere la navigation.
    /// </summary>
    public void Show()
    {
        ListWidget.ShowList(
        [
            new Option(Resources.UserInterface.Menu_ListJobs, _jobList.Show),
            new Option(Resources.UserInterface.Menu_AddBackup, _jobCreation.Show),
            new Option(Resources.UserInterface.Menu_RemoveBackup, _jobRemoval.Show),
            new Option(Resources.UserInterface.Menu_LaunchBackupJob, _jobLaunch.Show)
        ], _console);
    }
}

