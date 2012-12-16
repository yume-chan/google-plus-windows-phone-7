using System;
using System.Windows;
using System.Windows.Controls;
using Google_Plus.Pages;
using ImageTools.Controls;
using ImageTools.IO;
using ImageTools.IO.Gif;
using Microsoft.Phone.Controls;

namespace Google_Plus.Types
{
    public partial class PostView : UserControl
    {
        public PostView()
        {
            InitializeComponent();

            Decoders.AddDecoder<GifDecoder>();

            Loaded += new RoutedEventHandler(PostView_Loaded);
        }

        void PostView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_rootVisual.Content is PostDetail)
                (DataContext as Post).CanMention = true;
        }

        private static PhoneApplicationFrame _rootVisual = App.Current.RootVisual as PhoneApplicationFrame;

        private void btnPlusOne_Click(object sender, RoutedEventArgs e)
        {
            ((Post)DataContext).PlusOne();
        }

        private void btnReshare_Click(object sender, RoutedEventArgs e)
        {
            _rootVisual.Navigate(new Uri("//Pages/NewPost.xaml?pid={0}".FormatWith(((Post)DataContext).ID), UriKind.Relative));
        }

        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void btnMenuSwitcher_Click(object sender, RoutedEventArgs e)
        {
            cmPostMenu.IsOpen = !cmPostMenu.IsOpen;
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*
            StackPanel stackPanel = (StackPanel)sender;
            if (Image2.Source == null)
                Image1.MaxWidth = ActualWidth - 20;
            else
                Image2.MaxWidth = Image1.MaxWidth = ActualWidth / 2 - 10;
            */
        }

        private void menuViewProfile_Click(object sender, RoutedEventArgs e)
        {
            _rootVisual.Navigate(new Uri("//Pages/ProfilePage.xaml?uid={0}".FormatWith(((Post)DataContext).AuthorID), UriKind.Relative));
        }

        private void menuMention_Click(object sender, RoutedEventArgs e)
        {
            (_rootVisual.Content as PostDetail).CommentContent.Text += " @{0} ".FormatWith(((Post)DataContext).AuthorID);
        }
    }
}
