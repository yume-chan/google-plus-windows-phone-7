using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Google_Plus.Types
{
    public class RichTextHelper : RichTextBox
    {
        public const string RichTextPropertyName = "RichText";

        public static readonly DependencyProperty RichTextProperty =
            DependencyProperty.Register(RichTextPropertyName,
                                        typeof(Paragraph),
                                        typeof(RichTextBox),
                                        new PropertyMetadata(
                                            new PropertyChangedCallback
                                                (RichTextPropertyChanged)));

        public RichTextHelper()
        {
        }

        public string RichText
        {
            get { return (string)GetValue(RichTextProperty); }
            set { SetValue(RichTextProperty, value); }
        }

        private static void RichTextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Paragraph newBlock = (Paragraph)dependencyPropertyChangedEventArgs.NewValue;
            if (newBlock != null)
            {
                RichTextHelper box = (RichTextHelper)dependencyObject;
                box.Blocks.Clear();
                box.Blocks.Add(Clone(newBlock));
            }
        }

        public static Paragraph Clone(Paragraph org)
        {
            Paragraph result = new Paragraph();
            foreach (Inline inline in org.Inlines)
            {
                if (inline is Run)
                    result.Inlines.Add(Clone((Run)inline));
                else if (inline is Hyperlink)
                    result.Inlines.Add(Clone((Hyperlink)inline));
            }
            return result;
        }

        public static Run Clone(Run org)
        {
            return new Run()
            {
                Text = org.Text,
                TextDecorations = org.TextDecorations,
                FontStyle = org.FontStyle,
                FontWeight = org.FontWeight,
            };
        }

        public static Span Clone(Hyperlink org)
        {
            Span result = new Hyperlink()
                {
                    TextDecorations = org.TextDecorations,
                    FontStyle = org.FontStyle,
                    FontWeight = org.FontWeight,
                    Foreground = org.Foreground,
                    NavigateUri = org.NavigateUri
                };
            foreach (Inline inline in org.Inlines)
                if (inline is Run)
                    result.Inlines.Add(Clone((Run)inline));
            return result;
        }
    }
}
