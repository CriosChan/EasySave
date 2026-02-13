using Avalonia.Controls;
using EasySave.ViewModels;

namespace EasySave.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Inject StorageProvider into ViewModel
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SetStorageProvider(StorageProvider);
        }
    }
}