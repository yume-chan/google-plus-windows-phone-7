using System;
using System.Windows;
using System.Windows.Controls;
using Google_Plus.DataObject;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using System.Windows.Input;
using Coding4Fun.Phone.Controls;
using Google_Plus.Pages;

namespace Google_Plus.Types
{
    public partial class CommentView : UserControl
    {
        public CommentView()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(CommentView_Loaded);
        }

        void CommentView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_rootVisual.Content is PostDetail)
                (DataContext as Comment).CanMention = true;
        }

        private static PhoneApplicationFrame _rootVisual = App.Current.RootVisual as PhoneApplicationFrame;

        private void btnEditComment_Click(object sender, RoutedEventArgs e)
        {
            ChatBubbleTextBox editbox = new ChatBubbleTextBox();
            editbox.ChatBubbleDirection = ChatBubbleDirection.UpperRight;
            editbox.AcceptsReturn = true;
            editbox.TextWrapping = TextWrapping.Wrap;
            InputScope inputScope = new InputScope();
            inputScope.Names.Add(new InputScopeName() { NameValue = InputScopeNameValue.Text });
            editbox.InputScope = inputScope;
            editbox.FontSize = (double)Application.Current.Resources["PhoneFontSizeMedium"];
            editbox.Text = ((Comment)DataContext).RawContent;
            editbox.Tap += (s, a) => a.Handled = true;
            cbContent.Visibility = Visibility.Collapsed;
            spContainer.Children.Add(editbox);

            PhoneApplicationPage currentPage = (PhoneApplicationPage)_rootVisual.Content;
            IApplicationBar appbarBackup = currentPage.ApplicationBar;
            ApplicationBar myAppbar = new ApplicationBar();

            Action restore = () =>
            {
                cbContent.Visibility = Visibility.Visible;
                spContainer.Children.Remove(editbox);
                currentPage.ApplicationBar = appbarBackup;
            };

            ApplicationBarIconButton save = new ApplicationBarIconButton(new Uri("//Images/appbar.check.rest.png", UriKind.Relative));
            save.Text = "Save";
            ApplicationBarIconButton delete = new ApplicationBarIconButton(new Uri("//Images/appbar.delete.rest.png", UriKind.Relative));
            delete.Text = "Delete";
            delete.Click += (s, a) => deleteComment(restore);
            ApplicationBarIconButton cancel = new ApplicationBarIconButton(new Uri("//Images/appbar.cancel.rest.png", UriKind.Relative));
            cancel.Text = "Cancel";
            cancel.Click += (s, a) => restore();
            save.Click += (s, a) =>
            {
                if (editbox.Text != ((Comment)DataContext).RawContent)
                {
                    editbox.IsEnabled = false;
                    save.IsEnabled = false;
                    delete.IsEnabled = false;
                    cancel.IsEnabled = false;

                    App.Progress.IsIndeterminate = true;
                    App.Progress.Text = "Saving comment...";
                    App.Progress.IsVisible = true;

                    Comment comment = DataContext as Comment;
                    new Request("POST", APIs.EditComment, result =>
                    {
                        if (result.Success)
                        {
                            DataContext = new Comment(JArray.Parse(result.RawData.Substring(6))[0][1][1]);
                            restore();
                            App.Progress.IsVisible = false;
                        }
                        else
                        {
                            MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                            throw new Exception("ExitAppException");
                        }
                    }, new EditCommentObject(comment.ID, editbox.Text));
                }
                else
                    restore();
            };
            myAppbar.Buttons.Add(save);
            myAppbar.Buttons.Add(delete);
            myAppbar.Buttons.Add(cancel);
            currentPage.ApplicationBar = myAppbar;
            currentPage.BackKeyPress += (s, a) =>
            {
                if (editbox.Text != ((Comment)DataContext).RawContent && MessageBox.Show("Do you want to go back without saving your comment?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    a.Cancel = true;
                else
                    restore();
            };
            editbox.Focus();
        }

        private void btnPlusOneComment_Click(object sender, RoutedEventArgs e)
        {
            Comment comment = (Comment)DataContext;
            bool doIPlusOne = comment.DoIPlusOne;
            new Request("POST", APIs.PlusOne, result =>
            {
                if (result.Success)
                {
                    comment.PlusOneCount += !doIPlusOne ? 1 : -1;
                    comment.DoIPlusOne = !doIPlusOne;
                }
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google, app will exit now.", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            }, new PlusOneCommentObject(comment.ID, !doIPlusOne).ToString());
        }

        private void btnDeleteComment_Click(object sender, RoutedEventArgs e)
        {
            deleteComment(() => { });
        }

        private void deleteComment(Action callback)
        {
            Comment comment = DataContext as Comment;
            new Request("POST", APIs.DeleteComment, result =>
            {
                if (result.Success)
                {
                    callback();
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google, app will exit now.", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            }, new DeleteCommentObject(comment.ID).ToString());
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuViewProfile_Click(object sender, RoutedEventArgs e)
        {
            _rootVisual.Navigate(new Uri("//Pages/ProfilePage.xaml?uid={0}".FormatWith(((Comment)DataContext).AuthorID), UriKind.Relative));
        }

        private void menuMention_Click(object sender, RoutedEventArgs e)
        {
            (_rootVisual.Content as PostDetail).CommentContent.Text += " @{0} ".FormatWith(((Comment)DataContext).AuthorID);
        }
    }
}
