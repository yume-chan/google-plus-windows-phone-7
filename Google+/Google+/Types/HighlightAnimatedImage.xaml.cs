using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Google_Plus.Types
{
    public partial class HighlightAnimatedImage : UserControl
    {
        public HighlightAnimatedImage()
        {
            InitializeComponent();
        }

        public bool Highlight
        {
            get { return (bool)GetValue(HighlightProperty); }
            set { SetValue(HighlightProperty, value); }
        }

        public static readonly DependencyProperty HighlightProperty = DependencyProperty.Register("Highlight", typeof(bool), typeof(HighlightAnimatedImage), new PropertyMetadata(false, OnHighlightChanged));

        private static void OnHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HighlightAnimatedImage).Border.BorderBrush = new SolidColorBrush((bool)e.NewValue ? Colors.White : Colors.Transparent);
        }
    }
}
