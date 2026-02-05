using EasySave.Application.Services;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

public class LanguageView
{
    private IConsole _console;
    private LanguageService _lang;

    public LanguageView(IConsole console)
    {
        _console = console;
        _lang = new LanguageService();
    }
    
    public void Show()
    {
        ListWidget.ShowList(
        [
            new Option("Français", () =>
            {
                _lang.SetLanguage("fr");
                UserInterface.ShowMenu();
            }),
            new Option("English", () =>
            {
                _lang.SetLanguage("en");
                UserInterface.ShowMenu();
            }),
            new Option(Resources.UserInterface.Menu_Quit, UserInterface.ShowMenu)
        ], _console, Resources.UserInterface.Menu_Title_Lang);
    }
}