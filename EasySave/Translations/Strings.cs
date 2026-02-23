using System.Globalization;
using System.IO;
using Tlumach;
using Tlumach.Base;

namespace EasySave.Translations;

/// <summary>
///     Provides static <see cref="Tlumach.Avalonia.TranslationUnit" /> instances for all GUI-facing strings.
///     Use with the Tlumach markup extension:
///     <code>
///         &lt;TextBlock Text="{tlumach:Translate {x:Static t:Strings.Gui_Window_Title}}" /&gt;
///     </code>
/// </summary>
public static class Strings
{
    // ── Window ────────────────────────────────────────────────────────────
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Window_Title;

    // ── Navigation ───────────────────────────────────────────────────────
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Navigation_Back;

    // ── Menu ─────────────────────────────────────────────────────────────
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Menu_Root;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Menu_Settings;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Menu_ManageBusinessSoftware;

    // ── Jobs section ─────────────────────────────────────────────────────
    public static readonly Tlumach.Avalonia.TranslationUnit Jobs_Header;

    // ── Add Job section ──────────────────────────────────────────────────
    public static readonly Tlumach.Avalonia.TranslationUnit Add_Header;
    public static readonly Tlumach.Avalonia.TranslationUnit Add_PromptName;
    public static readonly Tlumach.Avalonia.TranslationUnit Add_PromptSource;
    public static readonly Tlumach.Avalonia.TranslationUnit Add_PromptTarget;
    public static readonly Tlumach.Avalonia.TranslationUnit Add_PromptType;

    // ── Buttons ───────────────────────────────────────────────────────────
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_BrowseSource;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_BrowseTarget;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_AddJob;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_RemoveSelected;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_RunSelectedJob;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_RunAllJobs;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_French;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_English;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_Json;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Button_Xml;

    // ── Settings ─────────────────────────────────────────────────────────
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Settings_Screen_Title;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Settings_Section_Language;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Settings_Section_LogType;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Settings_CryptoSoftKey;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Settings_ExtensionToCrypt;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_AddExtension;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_Tooltip_DeleteExtension;

    // ── Business Software ────────────────────────────────────────────────
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_BusinessSoftware_Catalog_Title;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_BusinessSoftware_Added_Title;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_BusinessSoftware_Search_Label;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_BusinessSoftware_Search_Placeholder;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_BusinessSoftware_Button_AddSelected;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_BusinessSoftware_Button_ViewAdded;
    public static readonly Tlumach.Avalonia.TranslationUnit Gui_BusinessSoftware_Button_RemoveSelected;

    private static readonly TranslationManager _manager;

    static Strings()
    {
        ResxParser.Use();

        var translationsDir = Path.Combine(AppContext.BaseDirectory, "Views", "Resources");
        var defaultFile = Path.Combine(translationsDir, "UserInterface.resx");
        var cfg = new TranslationConfiguration(null, defaultFile, "en-US", TextFormat.DotNet);
        _manager = new TranslationManager(cfg) { LoadFromDisk = true, TranslationsDirectory = translationsDir };
        Gui_Window_Title                           = new(_manager, cfg, nameof(Gui_Window_Title), false);
        Gui_Navigation_Back                        = new(_manager, cfg, nameof(Gui_Navigation_Back), false);
        Gui_Menu_Root                              = new(_manager, cfg, nameof(Gui_Menu_Root), false);
        Gui_Menu_Settings                          = new(_manager, cfg, nameof(Gui_Menu_Settings), false);
        Gui_Menu_ManageBusinessSoftware            = new(_manager, cfg, nameof(Gui_Menu_ManageBusinessSoftware), false);
        Jobs_Header                                = new(_manager, cfg, nameof(Jobs_Header), false);
        Add_Header                                 = new(_manager, cfg, nameof(Add_Header), false);
        Add_PromptName                             = new(_manager, cfg, nameof(Add_PromptName), false);
        Add_PromptSource                           = new(_manager, cfg, nameof(Add_PromptSource), false);
        Add_PromptTarget                           = new(_manager, cfg, nameof(Add_PromptTarget), false);
        Add_PromptType                             = new(_manager, cfg, nameof(Add_PromptType), false);
        Gui_Button_BrowseSource                    = new(_manager, cfg, nameof(Gui_Button_BrowseSource), false);
        Gui_Button_BrowseTarget                    = new(_manager, cfg, nameof(Gui_Button_BrowseTarget), false);
        Gui_Button_AddJob                          = new(_manager, cfg, nameof(Gui_Button_AddJob), false);
        Gui_Button_RemoveSelected                  = new(_manager, cfg, nameof(Gui_Button_RemoveSelected), false);
        Gui_Button_RunSelectedJob                  = new(_manager, cfg, nameof(Gui_Button_RunSelectedJob), false);
        Gui_Button_RunAllJobs                      = new(_manager, cfg, nameof(Gui_Button_RunAllJobs), false);
        Gui_Button_French                          = new(_manager, cfg, nameof(Gui_Button_French), false);
        Gui_Button_English                         = new(_manager, cfg, nameof(Gui_Button_English), false);
        Gui_Button_Json                            = new(_manager, cfg, nameof(Gui_Button_Json), false);
        Gui_Button_Xml                             = new(_manager, cfg, nameof(Gui_Button_Xml), false);
        Gui_Settings_Screen_Title                  = new(_manager, cfg, nameof(Gui_Settings_Screen_Title), false);
        Gui_Settings_Section_Language              = new(_manager, cfg, nameof(Gui_Settings_Section_Language), false);
        Gui_Settings_Section_LogType               = new(_manager, cfg, nameof(Gui_Settings_Section_LogType), false);
        Gui_Settings_CryptoSoftKey                 = new(_manager, cfg, nameof(Gui_Settings_CryptoSoftKey), false);
        Gui_Settings_ExtensionToCrypt              = new(_manager, cfg, nameof(Gui_Settings_ExtensionToCrypt), false);
        Gui_AddExtension                           = new(_manager, cfg, nameof(Gui_AddExtension), false);
        Gui_Tooltip_DeleteExtension                = new(_manager, cfg, nameof(Gui_Tooltip_DeleteExtension), false);
        Gui_BusinessSoftware_Catalog_Title         = new(_manager, cfg, nameof(Gui_BusinessSoftware_Catalog_Title), false);
        Gui_BusinessSoftware_Added_Title           = new(_manager, cfg, nameof(Gui_BusinessSoftware_Added_Title), false);
        Gui_BusinessSoftware_Search_Label          = new(_manager, cfg, nameof(Gui_BusinessSoftware_Search_Label), false);
        Gui_BusinessSoftware_Search_Placeholder    = new(_manager, cfg, nameof(Gui_BusinessSoftware_Search_Placeholder), false);
        Gui_BusinessSoftware_Button_AddSelected    = new(_manager, cfg, nameof(Gui_BusinessSoftware_Button_AddSelected), false);
        Gui_BusinessSoftware_Button_ViewAdded      = new(_manager, cfg, nameof(Gui_BusinessSoftware_Button_ViewAdded), false);
        Gui_BusinessSoftware_Button_RemoveSelected = new(_manager, cfg, nameof(Gui_BusinessSoftware_Button_RemoveSelected), false);
    }

    /// <summary>Gets the shared translation manager.</summary>
    public static TranslationManager TranslationManager => _manager;

    /// <summary>Changes the active locale for all translation units.</summary>
    public static void SetCulture(CultureInfo culture) => _manager.CurrentCulture = culture;
}
