using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.Views;

public partial class JobScreen : UserControl
{
    public JobScreen()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}