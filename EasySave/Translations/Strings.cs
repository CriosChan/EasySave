using System.Globalization;
using System.IO;
using Tlumach;
using Tlumach.Base;
using TlumachAvalonia = Tlumach.Avalonia;

namespace EasySave.Translations;

/// <summary>
///     Provides static <see cref="TlumachAvalonia.TranslationUnit" /> instances for all GUI-facing strings.
///     Use with the Tlumach markup extension:
///     <code>
///         &lt;TextBlock Text="{tlumach:Translate {x:Static t:Strings.GuiWindowTitle}}" /&gt;
///     </code>
/// </summary>
public static class Strings
{
    private static readonly TranslationManager _manager;

    // ── Window ────────────────────────────────────────────────────────────
    public static readonly TlumachAvalonia.TranslationUnit GuiWindowTitle;

    // ── Navigation ───────────────────────────────────────────────────────
    public static readonly TlumachAvalonia.TranslationUnit GuiNavigationBack;

    // ── Menu ─────────────────────────────────────────────────────────────
    public static readonly TlumachAvalonia.TranslationUnit GuiMenuRoot;
    public static readonly TlumachAvalonia.TranslationUnit GuiMenuSettings;
    public static readonly TlumachAvalonia.TranslationUnit GuiMenuManageBusinessSoftware;

    // ── Jobs section ─────────────────────────────────────────────────────
    public static readonly TlumachAvalonia.TranslationUnit JobsHeader;

    // ── Add Job section ──────────────────────────────────────────────────
    public static readonly TlumachAvalonia.TranslationUnit AddHeader;
    public static readonly TlumachAvalonia.TranslationUnit AddPromptName;
    public static readonly TlumachAvalonia.TranslationUnit AddPromptSource;
    public static readonly TlumachAvalonia.TranslationUnit AddPromptTarget;
    public static readonly TlumachAvalonia.TranslationUnit AddPromptType;

    // ── Buttons ───────────────────────────────────────────────────────────
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonBrowseSource;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonBrowseTarget;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonAddJob;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonRemoveSelected;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonRunSelectedJob;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonRunAllJobs;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonFrench;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonEnglish;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonJson;
    public static readonly TlumachAvalonia.TranslationUnit GuiButtonXml;

    // ── Settings ─────────────────────────────────────────────────────────
    public static readonly TlumachAvalonia.TranslationUnit GuiSettingsScreenTitle;
    public static readonly TlumachAvalonia.TranslationUnit GuiSettingsSectionLanguage;
    public static readonly TlumachAvalonia.TranslationUnit GuiSettingsSectionLogType;
    public static readonly TlumachAvalonia.TranslationUnit GuiSettingsCryptoSoftKey;
    public static readonly TlumachAvalonia.TranslationUnit GuiSettingsExtensionToCrypt;
    public static readonly TlumachAvalonia.TranslationUnit GuiAddExtension;
    public static readonly TlumachAvalonia.TranslationUnit GuiTooltipDeleteExtension;

    // ── Business Software ────────────────────────────────────────────────
    public static readonly TlumachAvalonia.TranslationUnit GuiBusinessSoftwareCatalogTitle;
    public static readonly TlumachAvalonia.TranslationUnit GuiBusinessSoftwareAddedTitle;
    public static readonly TlumachAvalonia.TranslationUnit GuiBusinessSoftwareSearchLabel;
    public static readonly TlumachAvalonia.TranslationUnit GuiBusinessSoftwareSearchPlaceholder;
    public static readonly TlumachAvalonia.TranslationUnit GuiBusinessSoftwareButtonAddSelected;
    public static readonly TlumachAvalonia.TranslationUnit GuiBusinessSoftwareButtonViewAdded;
    public static readonly TlumachAvalonia.TranslationUnit GuiBusinessSoftwareButtonRemoveSelected;

    static Strings()
    {
        ResxParser.Use();

        var translationsDir = Path.Combine(AppContext.BaseDirectory, "Views", "Resources");
        var defaultFile = Path.Combine(translationsDir, "UserInterface.resx");

        var config = new TranslationConfiguration(null, defaultFile, "en-US", TextFormat.DotNet);
        _manager = new TranslationManager(config);
        _manager.LoadFromDisk = true;
        _manager.TranslationsDirectory = translationsDir;

        var cfg = _manager.DefaultConfiguration!;

        GuiWindowTitle                          = new(_manager, cfg, "Gui.Window.Title", false);
        GuiNavigationBack                       = new(_manager, cfg, "Gui.Navigation.Back", false);
        GuiMenuRoot                             = new(_manager, cfg, "Gui.Menu.Root", false);
        GuiMenuSettings                         = new(_manager, cfg, "Gui.Menu.Settings", false);
        GuiMenuManageBusinessSoftware           = new(_manager, cfg, "Gui.Menu.ManageBusinessSoftware", false);
        JobsHeader                              = new(_manager, cfg, "Jobs.Header", false);
        AddHeader                               = new(_manager, cfg, "Add.Header", false);
        AddPromptName                           = new(_manager, cfg, "Add.PromptName", false);
        AddPromptSource                         = new(_manager, cfg, "Add.PromptSource", false);
        AddPromptTarget                         = new(_manager, cfg, "Add.PromptTarget", false);
        AddPromptType                           = new(_manager, cfg, "Add.PromptType", false);
        GuiButtonBrowseSource                   = new(_manager, cfg, "Gui.Button.BrowseSource", false);
        GuiButtonBrowseTarget                   = new(_manager, cfg, "Gui.Button.BrowseTarget", false);
        GuiButtonAddJob                         = new(_manager, cfg, "Gui.Button.AddJob", false);
        GuiButtonRemoveSelected                 = new(_manager, cfg, "Gui.Button.RemoveSelected", false);
        GuiButtonRunSelectedJob                 = new(_manager, cfg, "Gui.Button.RunSelectedJob", false);
        GuiButtonRunAllJobs                     = new(_manager, cfg, "Gui.Button.RunAllJobs", false);
        GuiButtonFrench                         = new(_manager, cfg, "Gui.Button.French", false);
        GuiButtonEnglish                        = new(_manager, cfg, "Gui.Button.English", false);
        GuiButtonJson                           = new(_manager, cfg, "Gui.Button.Json", false);
        GuiButtonXml                            = new(_manager, cfg, "Gui.Button.Xml", false);
        GuiSettingsScreenTitle                  = new(_manager, cfg, "Gui.Settings.Screen.Title", false);
        GuiSettingsSectionLanguage              = new(_manager, cfg, "Gui.Settings.Section.Language", false);
        GuiSettingsSectionLogType               = new(_manager, cfg, "Gui.Settings.Section.LogType", false);
        GuiSettingsCryptoSoftKey                = new(_manager, cfg, "Gui.Settings.CryptoSoftKey", false);
        GuiSettingsExtensionToCrypt             = new(_manager, cfg, "Gui.Settings.ExtensionToCrypt", false);
        GuiAddExtension                         = new(_manager, cfg, "Gui.AddExtension", false);
        GuiTooltipDeleteExtension               = new(_manager, cfg, "Gui.Tooltip.DeleteExtension", false);
        GuiBusinessSoftwareCatalogTitle         = new(_manager, cfg, "Gui.BusinessSoftware.Catalog.Title", false);
        GuiBusinessSoftwareAddedTitle           = new(_manager, cfg, "Gui.BusinessSoftware.Added.Title", false);
        GuiBusinessSoftwareSearchLabel          = new(_manager, cfg, "Gui.BusinessSoftware.Search.Label", false);
        GuiBusinessSoftwareSearchPlaceholder    = new(_manager, cfg, "Gui.BusinessSoftware.Search.Placeholder", false);
        GuiBusinessSoftwareButtonAddSelected    = new(_manager, cfg, "Gui.BusinessSoftware.Button.AddSelected", false);
        GuiBusinessSoftwareButtonViewAdded      = new(_manager, cfg, "Gui.BusinessSoftware.Button.ViewAdded", false);
        GuiBusinessSoftwareButtonRemoveSelected = new(_manager, cfg, "Gui.BusinessSoftware.Button.RemoveSelected", false);
    }

    // ── Shared TranslationManager ──────────────────────────────────────────
    /// <summary>Gets the shared translation manager. Use <see cref="SetCulture"/> to change the language.</summary>
    public static TranslationManager TranslationManager => _manager;

    /// <summary>Changes the active locale for all translation units.</summary>
    public static void SetCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return;
        try { _manager.CurrentCulture = new CultureInfo(cultureName); }
        catch (CultureNotFoundException) { }
    }
}
