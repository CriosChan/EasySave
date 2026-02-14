using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using EasySave.Data.Configuration;
using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.BusinessSoftware;
using EasySave.Models.Utils;
using EasySave.ViewModels;
using EasySave.ViewModels.Services;
using EasySave.Views;

namespace EasySave;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            IJobService jobService = new JobService();
            ILocalizationApplier localizationApplier = new LocalizationApplier();
            IApplicationSettingsService applicationSettingsService = new ApplicationSettingsService();
            IBusinessSoftwareCatalogService businessSoftwareCatalogService = new BusinessSoftwareCatalogService();
            IBusinessSoftwareSettingsService businessSoftwareSettingsService = new BusinessSoftwareSettingsService();
            IUiTextService uiTextService = new ResxUiTextService();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(
                    jobService,
                    localizationApplier,
                    applicationSettingsService,
                    businessSoftwareCatalogService,
                    businessSoftwareSettingsService,
                    uiTextService)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var pluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in pluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}
