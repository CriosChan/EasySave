using System.Diagnostics;
using System.Reflection;
using EasySave.Services;
using EasySave.View.Console;

namespace EasySave.View;

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
        ListWidget widget = new ListWidget();
        widget.ShowList(
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
            new Option(Ressources.UserInterface.Menu_Quit, () => widget.Return())
        ], Ressources.UserInterface.Menu_Header, _console);
    }
}