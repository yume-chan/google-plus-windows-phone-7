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
using System.Collections;
using ImageTools.Controls;
using System.ComponentModel;

namespace Google_Plus.Types
{
    public partial class LightBox : UserControl
    {
        public LightBox()
        {
            InitializeComponent();
        }

        int lastIndex = -1;
        int imageIndex = 0;
        bool DataLoaded = false;

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = SlideBox.SelectedIndex;
            IList imageList = ImageList.ItemsSource as IList;
            IEnumerable<HighlightAnimatedImage> ie = ImageList.GetLogicalChildrenByType<HighlightAnimatedImage>(true);
            if (lastIndex != -1)
            {
                if (imageList.Count > 1)
                    ie.ElementAt(imageIndex).Highlight = false;

                AnimatedImage target = null;
                int nextIndex = 0;
                switch (index)
                {
                    case 0:
                        if (lastIndex == 1)
                        {
                            target = Image3;
                            imageIndex--;
                            nextIndex = imageIndex - 1;
                        }
                        else
                        {
                            target = Image2;
                            imageIndex++;
                            nextIndex = imageIndex + 1;
                        }
                        break;
                    case 1:
                        if (lastIndex == 2)
                        {
                            target = Image1;
                            imageIndex--;
                            nextIndex = imageIndex - 1;
                        }
                        else
                        {
                            target = Image3;
                            imageIndex++;
                            nextIndex = imageIndex + 1;
                        }
                        break;
                    case 2:
                        if (lastIndex == 0)
                        {
                            target = Image2;
                            imageIndex--;
                            nextIndex = imageIndex - 1;
                        }
                        else
                        {
                            target = Image1;
                            imageIndex++;
                            nextIndex = imageIndex + 1;
                        }
                        break;
                }
                if (imageIndex < 0)
                    imageIndex += imageList.Count;
                else if (imageIndex >= imageList.Count)
                    imageIndex -= imageList.Count;
                if (nextIndex < 0)
                    nextIndex += imageList.Count;
                else if (nextIndex >= imageList.Count)
                    nextIndex -= imageList.Count;
                target.Source = imageList[nextIndex];
                if (imageList.Count > 1)
                    ie.ElementAt(imageIndex).Highlight = true;
            }
            lastIndex = index;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.IsInDesignTool)
                return;

            if (!DataLoaded)
            {
                IList imageList = ImageList.ItemsSource as IList;
                switch (imageList.Count)
                {
                    case 0:
                        break;
                    case 1:
                        ImageList.Visibility = Visibility.Collapsed;
                        Image1.Source = imageList[0];
                        SlideBox.IsEnabled = false;
                        break;
                    case 2:
                        Image1.Source = imageList[0];
                        Image3.Source = Image2.Source = imageList[1];
                        break;
                    default:
                        Image1.Source = imageList[0];
                        Image2.Source = imageList[1];
                        Image3.Source = imageList[imageList.Count - 1];
                        break;
                }
                SlideBox.MaxHeight = MaxHeight - 100;
                if (imageList.Count > 1)
                    ImageList.GetLogicalChildrenByType<HighlightAnimatedImage>(true).ElementAt(0).Highlight = true;
                DataLoaded = true;
            }
        }

        private void UserControl_Tap(object sender, GestureEventArgs e)
        {
            // e.Handled = true;
        }

        private void HighlightAnimatedImage_Tap(object sender, GestureEventArgs e)
        {
            IEnumerable<HighlightAnimatedImage> ie = ImageList.GetLogicalChildrenByType<HighlightAnimatedImage>(true);
            ie.ElementAt(imageIndex).Highlight = false;

            imageIndex = ImageList.SelectedIndex;

            IList imageList = ImageList.ItemsSource as IList;
            int previousIndex = imageIndex - 1;
            if (previousIndex < 0)
                previousIndex += imageList.Count;
            else if (previousIndex >= imageList.Count)
                previousIndex -= imageList.Count;

            int nextIndex = imageIndex + 1;
            if (nextIndex < 0)
                nextIndex += imageList.Count;
            else if (nextIndex >= imageList.Count)
                nextIndex -= imageList.Count;

            switch (SlideBox.SelectedIndex)
            {
                case 0:
                    Image3.Source = imageList[previousIndex];
                    Image1.Source = imageList[imageIndex];
                    Image2.Source = imageList[nextIndex];
                    break;
                case 1:
                    Image1.Source = imageList[previousIndex];
                    Image2.Source = imageList[imageIndex];
                    Image3.Source = imageList[nextIndex];
                    break;
                case 2:
                    Image2.Source = imageList[previousIndex];
                    Image3.Source = imageList[imageIndex];
                    Image1.Source = imageList[nextIndex];
                    break;
            }

            ie.ElementAt(imageIndex).Highlight = true;

            e.Handled = true;
        }
    }
}
