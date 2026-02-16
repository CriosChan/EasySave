using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.Views;

public partial class MainScreen : UserControl
{
    public MainScreen()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}