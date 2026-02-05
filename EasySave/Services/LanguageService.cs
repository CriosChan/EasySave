using EasySave.Utils;

namespace EasySave.Services;

public class LanguageService
{
    public void SetLanguage(string lang)
    {
        ApplicationConfiguration cfg = ApplicationConfiguration.Instance;
        cfg.Localization = lang;
        
        // Apply localization early so menus/prompts pick the right resource.
        Lang.TryApplyCulture(cfg.Localization);
    }
}