using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace WPExtensions
{
    public partial class Extensions
    {
        public static DependencyProperty FocusOnLoadProperty = DependencyProperty.RegisterAttached("FocusOnLoad", typeof(string), typeof(Extensions), new PropertyMetadata(String.Empty));

        public static String GetFocusOnLoad(Page element)
        {
            return (String)element.GetValue(FocusOnLoadProperty);
        }

        public static void SetFocusOnLoad(Page element, string value)
        {
            if (GetFocusOnLoad(element) != value)
            {
                element.Loaded += (sender, e) =>
                {
                    var textBox = UIHelper.FindChildItem(element, value) as TextBox;
                    element.SetValue(FocusOnLoadProperty, value);
                    textBox.Focus();
                };
            }
        }
    }
}