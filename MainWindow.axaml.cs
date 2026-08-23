using Avalonia.Controls;
using Vereinskasse.ViewModels;

namespace Vereinskasse;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}