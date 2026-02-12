using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using EasySave.Composition;
using EasySave.ViewModels;
using EasySave.Views;

namespace EasySave;

public partial class App : Application
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

            var services = AppRuntime.Services ?? AppServicesFactory.CreateForUi();
            AppRuntime.Services ??= services;

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(
                    services.JobService,
                    services.BackupService,
                    services.StateSynchronizer,
                    services.JobValidator,
                    services.UserPreferences,
                    services.LocalizationApplier,
                    services.PathService,
                    services.UiProgressReporter)
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
