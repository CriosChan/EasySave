using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.Views;

public partial class SoftwareCatalogScreen : UserControl
{
    public SoftwareCatalogScreen()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}