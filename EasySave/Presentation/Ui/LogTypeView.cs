using EasySave.Application.Services;
using EasySave.Infrastructure.Configuration;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     Represents a view for managing language settings in the application.
/// </summary>
public class LogTypeView
{
    private readonly IConsole _console;
    private readonly LoggerService _loggerService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LogTypeView" /> class.
    /// </summary>
    /// <param name="console">An instance of <see cref="IConsole" /> used for displaying output.</param>
    public LogTypeView(IConsole console)
    {
        _console = console;
        _loggerService = new LoggerService();
    }

    /// <summary>
    ///     Displays the log type selection menu to the user.
    /// </summary>
    /// <remarks>
    ///     The method shows a list of available log types, allowing the user to select
    ///     their preferred log type. Once a log type is selected, it updates the log type
    ///     setting using the <see cref="LoggerService" /> and then returns to the main menu.
    /// </remarks>
    public void Show()
    {
        var cfg = ApplicationConfiguration.Instance;
        ListWidget.ShowList(
        [
            new Option("JSON" + (cfg.LogType == "json" ? " (Selected)" : ""), () =>
            {
                _loggerService.SetLogger("json");
                UserInterface.ShowMenu();
            }),

            new Option("XML" + (cfg.LogType == "xml" ? " (Selected)" : ""), () =>
            {
                _loggerService.SetLogger("xml");
                UserInterface.ShowMenu();
            }),

            new Option(Resources.UserInterface.Return, UserInterface.ShowMenu)
        ], _console, Resources.UserInterface.Menu_Title_LogType);
    }
}