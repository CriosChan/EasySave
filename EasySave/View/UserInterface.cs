namespace EasySave.View;

public static class UserInterface
{
    /// <summary>
    /// Show the main menu to the user.
    /// </summary>
    public static void ShowMenu()
    {
        ListWidget.ShowList(
            [
                new Option(Ressources.UserInterface.Menu_ListJobs, ListJobs),
                new Option(Ressources.UserInterface.Menu_AddBackup, () => throw new NotImplementedException()),
                new Option(Ressources.UserInterface.Menu_RemoveBackup, () => throw new NotImplementedException()),
                new Option(Ressources.UserInterface.Menu_LaunchBackupJob, () => throw new NotImplementedException())
            ]
            );
    }
        
    /// <summary>
    /// List all backup jobs.
    /// </summary>
    private static void ListJobs()
    {
        // TODO: Implement job listing functionality
        throw new NotImplementedException();
    }
}