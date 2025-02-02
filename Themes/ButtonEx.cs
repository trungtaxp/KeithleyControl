﻿using System;
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

namespace ButtonEx
{
    /// <summary>
    /// Follow step 1a or 1b, and then step 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Use the custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root
    /// element of the markup file where you want to use the attribute:
    ///
    /// xmlns:MyNamespace="clr-namespace:WpfNumericUpDown"
    ///
    ///
    /// Step 1b) Use the custom control in a XAML file that exists in another project.
    /// Add this XmlNamespace attribute to the root
    /// element of the markup file where you want to use it:
    ///
    /// xmlns:MyNamespace="clr-namespace:WpfNumericUpDown;assembly=WpfNumericUpDown"
    ///
    /// You will also need to add a project reference to this project from the project where the XAML file is located,
    /// and rebuild to avoid compilation errors:
    ///
    /// Right-click the target project in Solution Explorer and click
    /// Add Reference -> Project -> [Browse to find and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use the control in your XAML file.
    ///
    ///     <MyNamespace:ButtonEx/>
    ///
    /// </summary>
    public class ButtonEx : Button
    {
        static ButtonEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonEx), new FrameworkPropertyMetadata(typeof(ButtonEx)));
        }


        public ButtonType ButtonType
        {
            get { return (ButtonType)GetValue(ButtonTypeProperty); }
            set { SetValue(ButtonTypeProperty, value); }
        }

        public static readonly DependencyProperty ButtonTypeProperty =
            DependencyProperty.Register("ButtonType", typeof(ButtonType), typeof(ButtonEx),
                new PropertyMetadata(ButtonType.Normal));


        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ButtonEx), new PropertyMetadata(null));


        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ButtonEx),
                new PropertyMetadata(new CornerRadius(0)));


        public Brush MouseOverForeground
        {
            get { return (Brush)GetValue(MouseOverForegroundProperty); }
            set { SetValue(MouseOverForegroundProperty, value); }
        }

        public static readonly DependencyProperty MouseOverForegroundProperty =
            DependencyProperty.Register("MouseOverForeground", typeof(Brush), typeof(ButtonEx), new PropertyMetadata());


        public Brush MousePressedForeground
        {
            get { return (Brush)GetValue(MousePressedForegroundProperty); }
            set { SetValue(MousePressedForegroundProperty, value); }
        }

        public static readonly DependencyProperty MousePressedForegroundProperty =
            DependencyProperty.Register("MousePressedForeground", typeof(Brush), typeof(ButtonEx),
                new PropertyMetadata());


        public Brush MouseOverBorderbrush
        {
            get { return (Brush)GetValue(MouseOverBorderbrushProperty); }
            set { SetValue(MouseOverBorderbrushProperty, value); }
        }

        public static readonly DependencyProperty MouseOverBorderbrushProperty =
            DependencyProperty.Register("MouseOverBorderbrush", typeof(Brush), typeof(ButtonEx),
                new PropertyMetadata());


        public Brush MouseOverBackground
        {
            get { return (Brush)GetValue(MouseOverBackgroundProperty); }
            set { SetValue(MouseOverBackgroundProperty, value); }
        }

        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(ButtonEx), new PropertyMetadata());


        public Brush MousePressedBackground
        {
            get { return (Brush)GetValue(MousePressedBackgroundProperty); }
            set { SetValue(MousePressedBackgroundProperty, value); }
        }

        public static readonly DependencyProperty MousePressedBackgroundProperty =
            DependencyProperty.Register("MousePressedBackground", typeof(Brush), typeof(ButtonEx),
                new PropertyMetadata());
    }

    public enum ButtonType
    {
        Normal,
        Icon,
        Text,
        IconText
    }
}
