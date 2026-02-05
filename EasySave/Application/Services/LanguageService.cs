using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.Lang;

namespace EasySave.Application.Services;

public class LanguageService
{
    public void SetLanguage(string lang)
    {
        ApplicationConfiguration cfg = ApplicationConfiguration.Instance;
        cfg.Localization = lang;

        // Apply localization early so menus/prompts pick the right resource.
        LangUtil.TryApplyCulture(cfg.Localization);
    }
}