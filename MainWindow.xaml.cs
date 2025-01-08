using KeithleyControl.ViewModels;
using System.Windows;

namespace KeithleyControl
{
    public partial class MainWindow : Window
    {
        internal MainWindowVM mainWindowVM = null;

        public MainWindow()
        {
            InitializeComponent();
            Height = 500;
            Width = 1400;
            mainWindowVM = new MainWindowVM();
            DataContext = mainWindowVM;
        }
    }
}