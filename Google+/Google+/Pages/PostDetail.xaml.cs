using System;
using System.Windows;
using Google_Plus.DataObject;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using System.Net;
using Google_Plus.Types;

namespace Google_Plus.Pages
{
    public partial class PostDetail : AnimatedRotatePhoneApplicationPage
    {
        public PostDetail()
            : base()
        {
            InitializeComponent();
        }

        string postId;

        private void loadPost()
        {
            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Loading...";
            App.Progress.IsVisible = true;
            postId = NavigationContext.QueryString["pid"];
            new Request("POST", APIs.GetActivity, result =>
            {
                if (result.Success)
                {
                    DataContext = new Post(JArray.Parse(result.RawData.Substring(6))[0][1][1]);
                    if (((Post)DataContext).DoIPlusOne)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IconUri = new Uri("//Images/simon.unplusone.rest.png", UriKind.Relative);
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = "Un +1";
                    }
                    else
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IconUri = new Uri("//Images/simon.plusone.rest.png", UriKind.Relative);
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = "+1";
                    }
                    App.Progress.IsVisible = false;
                    PostContainer.Content = new Google_Plus.Controls.PostView();
                    CommentContent.Visibility = Visibility.Visible;
                    ApplicationBar.IsVisible = true;
                }
                else if (result.Response.StatusCode != HttpStatusCode.NotFound)
                {
                    MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            }, new GetActivityObject(postId).ToString());
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            loadPost();
        }

        private void btnComment_Click(object sender, EventArgs e)
        {
            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Sending comment...";
            App.Progress.IsVisible = true;
            CommentContent.IsEnabled = false;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
            new Request("POST", APIs.Comment, result =>
            {
                if (result.Success)
                {
                    CommentContent.Text = "";
                    CommentContent.IsEnabled = true;
                    loadPost();
                    ((MainViewModel)App.ViewModel).NeedRefreash = true;
                }
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google, app will exit now.", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            }, new CommentObject(postId, CommentContent.Text));
            // .Replace(",", " @104172795430787913441 " /* +Xsummer Xia */).Replace("，", " @104172795430787913441 " /* +Xsummer Xia */)
        }

        private void commentContent_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = !string.IsNullOrWhiteSpace(CommentContent.Text);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !string.IsNullOrWhiteSpace(CommentContent.Text) && MessageBox.Show("Do you want to go back without saving your comment?", "Comfirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel;
        }

        private void btnPlusOne_Click(object sender, EventArgs e)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
            new Request("POST", APIs.PlusOne, result =>
            {
                if (result.Success)
                {
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                    loadPost();
                }
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google, app will exit now.", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            }, new PlusOnePostObject(postId, !((Post)DataContext).DoIPlusOne).ToString());
        }

        private void btnReshare_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("//Pages/NewPost.xaml?pid=" + postId, UriKind.Relative));
        }
    }
}