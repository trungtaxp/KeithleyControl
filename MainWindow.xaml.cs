using System;
using KeithleyControl.ViewModels;
using System.Windows;
using System.Windows.Navigation;


namespace KeithleyControl
{
    public partial class MainWindow : Window
    {
        internal MainWindowVM mainWindowVM = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowVM();
        }
        
        private void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var viewModel = DataContext as MainWindowVM;
            if (viewModel != null && !string.IsNullOrEmpty(viewModel.WebBrowserSource))
            {
                webBrowser.Source = new Uri(viewModel.WebBrowserSource);
            }
        }
        
    }
}
