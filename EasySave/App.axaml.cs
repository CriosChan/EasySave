using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.ViewModels;
using EasySave.Views;

namespace EasySave;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var runtime = Program.Runtime
                          ?? throw new InvalidOperationException("Application runtime is not initialized.");

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(runtime)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
