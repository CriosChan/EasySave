namespace EasySave.Presentation.Ui;

/// <summary>
///     Connects views to the main menu controller without using globals.
/// </summary>
internal sealed class MenuNavigator : IMenuNavigator
{
    private MainMenuController? _menu;

    public void Attach(MainMenuController menu)
    {
        _menu = menu ?? throw new ArgumentNullException(nameof(menu));
    }

    public void ShowMainMenu()
    {
        if (_menu == null)
            throw new InvalidOperationException("MenuNavigator has not been initialized.");

        _menu.Show();
    }
}
