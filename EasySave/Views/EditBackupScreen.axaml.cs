using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.Views;

public partial class EditBackupScreen : UserControl
{
    public EditBackupScreen()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
