using EasySave.Application.Services;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     Represents a view for managing language settings in the application.
/// </summary>
public class LanguageView
{
    private readonly IConsole _console;
    private readonly LanguageService _lang;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageView" /> class.
    /// </summary>
    /// <param name="console">An instance of <see cref="IConsole" /> used for displaying output.</param>
    public LanguageView(IConsole console)
    {
        _console = console;
        _lang = new LanguageService();
    }

    /// <summary>
    ///     Displays the language selection menu to the user.
    /// </summary>
    /// <remarks>
    ///     The method shows a list of available languages, allowing the user to select
    ///     their preferred language. Once a language is selected, it updates the language
    ///     setting using the <see cref="LanguageService" /> and then returns to the main menu.
    /// </remarks>
    public void Show()
    {
        ListWidget.ShowList(
        [
            new Option("Français", () =>
            {
                _lang.SetLanguage("fr-FR");
                UserInterface.ShowMenu();
            }),

            new Option("English", () =>
            {
                _lang.SetLanguage("en-US");
                UserInterface.ShowMenu();
            }),

            new Option(Resources.UserInterface.Return, UserInterface.ShowMenu)
        ], _console, Resources.UserInterface.Menu_Title_Lang);
    }
}