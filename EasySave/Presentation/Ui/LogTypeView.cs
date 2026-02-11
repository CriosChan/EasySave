using EasySave.Application.Abstractions;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     Represents a view for managing language settings in the application.
/// </summary>
internal sealed class LogTypeView
{
    private readonly IConsole _console;
    private readonly IMenuNavigator _navigator;
    private readonly IUserPreferences _preferences;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LogTypeView" /> class.
    /// </summary>
    /// <param name="console">An instance of <see cref="IConsole" /> used for displaying output.</param>
    public LogTypeView(IConsole console, IUserPreferences preferences, IMenuNavigator navigator)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
    }

    /// <summary>
    ///     Displays the log type selection menu to the user.
    /// </summary>
    /// <remarks>
    ///     The method shows a list of available log types, allowing the user to select
    ///     their preferred log type. Once a log type is selected, it updates the log type
    ///     setting and then returns to the main menu.
    /// </remarks>
    public void Show()
    {
        var logType = _preferences.LogType;
        ListWidget.ShowList(
        [
            new Option("JSON" + (logType == "json" ? " (Selected)" : ""), () =>
            {
                _preferences.SetLogType("json");
                _navigator.ShowMainMenu();
            }),

            new Option("XML" + (logType == "xml" ? " (Selected)" : ""), () =>
            {
                _preferences.SetLogType("xml");
                _navigator.ShowMainMenu();
            }),

            new Option(Resources.UserInterface.Return, _navigator.ShowMainMenu)
        ], _console, Resources.UserInterface.Menu_Title_LogType);
    }
}
