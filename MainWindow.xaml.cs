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
            Height = 800;
            Width = Height / 0.6;
            mainWindowVM = new MainWindowVM();
            DataContext = mainWindowVM;
        }
    }
}