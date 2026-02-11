using EasySave.Application.Abstractions;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     Represents a view for managing language settings in the application.
/// </summary>
internal sealed class LanguageView
{
    private readonly IConsole _console;
    private readonly IUserPreferences _preferences;
    private readonly ILocalizationApplier _localization;
    private readonly IMenuNavigator _navigator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageView" /> class.
    /// </summary>
    /// <param name="console">An instance of <see cref="IConsole" /> used for displaying output.</param>
    public LanguageView(IConsole console, IUserPreferences preferences, ILocalizationApplier localization,
        IMenuNavigator navigator)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
    }

    /// <summary>
    ///     Displays the language selection menu to the user.
    /// </summary>
    /// <remarks>
    ///     The method shows a list of available languages, allowing the user to select
    ///     their preferred language. Once a language is selected, it updates the language
    ///     setting and then returns to the main menu.
    /// </remarks>
    public void Show()
    {
        ListWidget.ShowList(
        [
            new Option("Français", () =>
            {
                _preferences.SetLocalization("fr-FR");
                _localization.Apply("fr-FR");
                _navigator.ShowMainMenu();
            }),

            new Option("English", () =>
            {
                _preferences.SetLocalization("en-US");
                _localization.Apply("en-US");
                _navigator.ShowMainMenu();
            }),

            new Option(Resources.UserInterface.Return, _navigator.ShowMainMenu)
        ], _console, Resources.UserInterface.Menu_Title_Lang);
    }
}
