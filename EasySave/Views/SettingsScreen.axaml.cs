using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.Views;

public partial class SettingsScreen : UserControl
{
    public SettingsScreen()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}