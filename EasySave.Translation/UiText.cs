using Tlumach.Avalonia;

namespace EasySave.Translation;

/// <summary>
///     Flat translation-unit facade intended for Avalonia XAML static bindings.
/// </summary>
public static class UiText
{
    public static TranslationUnit GuiWindowTitle => Strings.Gui.Window.Title;
    public static TranslationUnit GuiNavigationBack => Strings.Gui.Navigation.Back;

    public static TranslationUnit JobsHeader => Strings.Jobs.Header;

    public static TranslationUnit GuiMenuRoot => Strings.Gui.Menu.Root;
    public static TranslationUnit GuiMenuSettings => Strings.Gui.Menu.Settings;
    public static TranslationUnit GuiMenuManageBusinessSoftware => Strings.Gui.Menu.ManageBusinessSoftware;

    public static TranslationUnit AddHeader => Strings.Add.Header;
    public static TranslationUnit AddPromptName => Strings.Add.PromptName;
    public static TranslationUnit AddPromptSource => Strings.Add.PromptSource;
    public static TranslationUnit AddPromptTarget => Strings.Add.PromptTarget;
    public static TranslationUnit AddPromptType => Strings.Add.PromptType;

    public static TranslationUnit GuiButtonBrowseSource => Strings.Gui.Button.BrowseSource;
    public static TranslationUnit GuiButtonBrowseTarget => Strings.Gui.Button.BrowseTarget;
    public static TranslationUnit GuiButtonAddJob => Strings.Gui.Button.AddJob;
    public static TranslationUnit GuiButtonRemoveSelected => Strings.Gui.Button.RemoveSelected;
    public static TranslationUnit GuiButtonRunSelectedJob => Strings.Gui.Button.RunSelectedJob;
    public static TranslationUnit GuiButtonRunAllJobs => Strings.Gui.Button.RunAllJobs;

    public static TranslationUnit GuiSettingsScreenTitle => Strings.Gui.Settings.Screen.Title;
    public static TranslationUnit GuiSettingsSectionLanguage => Strings.Gui.Settings.Section.Language;
    public static TranslationUnit GuiSettingsSectionLogType => Strings.Gui.Settings.Section.LogType;
    public static TranslationUnit GuiSettingsCryptoSoftKey => Strings.Gui.Settings.CryptoSoftKey;
    public static TranslationUnit GuiSettingsExtensionToCrypt => Strings.Gui.Settings.ExtensionToCrypt;
    public static TranslationUnit GuiSettingsPriorityExtensions => Strings.Gui.Settings.PriorityExtensions;

    public static TranslationUnit GuiButtonFrench => Strings.Gui.Button.French;
    public static TranslationUnit GuiButtonEnglish => Strings.Gui.Button.English;
    public static TranslationUnit GuiButtonJson => Strings.Gui.Button.Json;
    public static TranslationUnit GuiButtonXml => Strings.Gui.Button.Xml;

    public static TranslationUnit GuiAddExtension => Strings.Gui.AddExtension;
    public static TranslationUnit GuiAddPriorityExtension => Strings.Gui.AddPriorityExtension;
    public static TranslationUnit GuiTooltipDeleteExtension => Strings.Gui.Tooltip.DeleteExtension;
    public static TranslationUnit GuiTooltipDeletePriorityExtension => Strings.Gui.Tooltip.DeletePriorityExtension;

    public static TranslationUnit GuiBusinessSoftwareCatalogTitle => Strings.Gui.BusinessSoftware.Catalog.Title;
    public static TranslationUnit GuiBusinessSoftwareAddedTitle => Strings.Gui.BusinessSoftware.Added.Title;
    public static TranslationUnit GuiBusinessSoftwareSearchLabel => Strings.Gui.BusinessSoftware.Search.Label;
    public static TranslationUnit GuiBusinessSoftwareSearchPlaceholder => Strings.Gui.BusinessSoftware.Search.Placeholder;
    public static TranslationUnit GuiBusinessSoftwareButtonViewAdded => Strings.Gui.BusinessSoftware.Button.ViewAdded;
    public static TranslationUnit GuiBusinessSoftwareButtonAddSelected => Strings.Gui.BusinessSoftware.Button.AddSelected;
    public static TranslationUnit GuiBusinessSoftwareButtonRemoveSelected => Strings.Gui.BusinessSoftware.Button.RemoveSelected;
}
