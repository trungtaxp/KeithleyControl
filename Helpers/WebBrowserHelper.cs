using Microsoft.Web.WebView2.Wpf;
using System;
using System.Windows;

namespace KeithleyControl.Helpers
{
    public static class WebBrowserHelper
    {
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached(
                "BindableSource",
                typeof(string),
                typeof(WebBrowserHelper),
                new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        private static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is WebView2 webView)
            {
                string uri = e.NewValue as string;
                webView.Source = !string.IsNullOrEmpty(uri) ? new Uri(uri) : null;
            }
        }
    }
}