﻿using KeithleyControl.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
