using Avalonia.Controls;
using TVM_CalcUI.ViewModels;

namespace TVM_CalcUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}