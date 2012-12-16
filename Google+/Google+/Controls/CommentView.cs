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
using Google_Plus.Types;
using System.Windows.Media.Imaging;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Google_Plus.DataObject;
using Newtonsoft.Json.Linq;

namespace Google_Plus.Controls
{
    public class CommentView : Control
    {
        private const string AuthorPhotoLink = "AuthorPhotoLink";
        private HyperlinkButton _authorPhotoLink;

        private const string AuthorPhoto = "AuthorPhoto";
        private Image _authorPhoto;

        private const string PlusOneCount = "PlusOneCount";
        private TextBlock _plusOneCount;

        private const string Information = "Information";
        private TextBlock _information;

        private const string AuthorNameLink = "AuthorNameLink";
        private HyperlinkButton _authorNameLink;

        private const string AuthorName = "AuthorName";
        private TextBlock _authorName;

        private const string ContentChatBubble = "ContentChatBubble";
        private ChatBubble _contentChatBubble;
        private RichTextBox _contentCache;

        private const string Copy = "Copy";
        private MenuItem _copy;

        private const string PlusOne = "PlusOne";
        private MenuItem _plusOne;

        private const string Report = "Report";
        private MenuItem _report;

        private const string Edit = "Edit";
        private MenuItem _edit;

        private const string Delete = "Delete";
        private MenuItem _delete;

        private const string _profilePage = "//Pages/ProfilePage.xaml?uid={0}";

        private static PhoneApplicationFrame _rootVisual = App.Current.RootVisual as PhoneApplicationFrame;

        public CommentView()
        {
            DefaultStyleKey = typeof(CommentView);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Comment dataContent = DataContext as Comment;

            Uri authorLink = new Uri(_profilePage.FormatWith(dataContent.AuthorID), UriKind.Relative);

            _authorPhotoLink = GetTemplateChild(AuthorPhotoLink) as HyperlinkButton;
            _authorPhotoLink.NavigateUri = authorLink;

            _authorPhoto = GetTemplateChild(AuthorPhoto) as Image;
            _authorPhoto.Source = new BitmapImage(dataContent.AuthorPhoto);

            if (dataContent.HasPlusOne)
            {
                _plusOneCount = GetTemplateChild(PlusOneCount) as TextBlock;
                _plusOneCount.Text = "+{0}".FormatWith(dataContent.PlusOneCount);
                _plusOneCount.Foreground = dataContent.PlusOneButtonBrush;
            }

            _information = GetTemplateChild(Information) as TextBlock;
            _information.Text = dataContent.Information;

            _authorNameLink = GetTemplateChild(AuthorNameLink) as HyperlinkButton;
            _authorNameLink.NavigateUri = authorLink;

            _authorName = GetTemplateChild(AuthorName) as TextBlock;
            _authorName.Text = dataContent.Author;

            _contentChatBubble = GetTemplateChild(ContentChatBubble) as ChatBubble;
            if (_contentCache == null)
            {
                _contentCache = new RichTextBox();
                _contentCache.FontSize = (double)Application.Current.Resources["PhoneFontSizeMedium"];
                _contentCache.TextWrapping = TextWrapping.Wrap;
                _contentCache.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                _contentCache.Blocks.Add(RichTextHelper.Clone(dataContent.Content));
                _contentCache.Hold += select;
            }
            _contentChatBubble.Content = _contentCache;

            _copy = GetTemplateChild(Copy) as MenuItem;
            _copy.Click += copy;

            _plusOne = GetTemplateChild(PlusOne) as MenuItem;
            _plusOne.Click += (s, e) => dataContent.PlusOne();

            _report = GetTemplateChild(Report) as MenuItem;

            _edit = GetTemplateChild(Edit) as MenuItem;
            _edit.Click += edit;

            _delete = GetTemplateChild(Delete) as MenuItem;
            _delete.Click += (s, e) => delete();

            if (dataContent.IsMine)
            {
                _report.Visibility = Visibility.Collapsed;
                _edit.Visibility = Visibility.Visible;
                _delete.Visibility = Visibility.Visible;
            }
        }

        void select(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TextPointer holdPointer = _contentCache.GetPositionFromPoint(e.GetPosition(_contentCache));

            int offset = 3;
            TextPointer startPointer = holdPointer.GetPositionAtOffset(offset, LogicalDirection.Backward);
            while (startPointer == null && offset != -1)
                startPointer = holdPointer.GetPositionAtOffset(--offset, LogicalDirection.Backward);

            offset = 3;
            TextPointer endPointer = holdPointer.GetPositionAtOffset(offset, LogicalDirection.Forward);
            while (endPointer == null && offset != -1)
                endPointer = holdPointer.GetPositionAtOffset(--offset, LogicalDirection.Forward);

            _contentCache.Selection.Select(startPointer, endPointer);
        }

        void copy(object sender, RoutedEventArgs e)
        {
            _contentCache.SelectAll();
            Clipboard.SetText(_contentCache.Selection.Text);
            _contentCache.Selection.Select(_contentCache.ContentStart, _contentCache.ContentStart);

            new ToastPrompt { Title = "Copied to clipboard." }.Show();
        }

        private void edit(object sender, RoutedEventArgs e)
        {
            string rawContent = ((Comment)DataContext).RawContent;

            TextBox editbox = new ChatBubbleTextBox();
            editbox.AcceptsReturn = true;
            editbox.TextWrapping = TextWrapping.Wrap;

            InputScope inputScope = new InputScope();
            inputScope.Names.Add(new InputScopeName() { NameValue = InputScopeNameValue.Text });
            editbox.InputScope = inputScope;

            editbox.FontSize = (double)Application.Current.Resources["PhoneFontSizeMedium"];
            editbox.Text = rawContent;
            editbox.Tap += (s, a) => a.Handled = true;

            _contentChatBubble.Content = editbox;

            PhoneApplicationPage currentPage = (PhoneApplicationPage)_rootVisual.Content;
            IApplicationBar appbarBackup = currentPage.ApplicationBar;
            ApplicationBar myAppbar = new ApplicationBar();

            Action restore = () =>
            {
                _contentChatBubble.Content = _contentCache;
                currentPage.ApplicationBar = appbarBackup;
            };

            ApplicationBarIconButton appbarSave = new ApplicationBarIconButton(new Uri("//Images/appbar.check.rest.png", UriKind.Relative));
            appbarSave.Text = "Save";
            ApplicationBarIconButton appbarDelete = new ApplicationBarIconButton(new Uri("//Images/appbar.delete.rest.png", UriKind.Relative));
            appbarDelete.Text = "Delete";
            ApplicationBarIconButton appbarCancel = new ApplicationBarIconButton(new Uri("//Images/appbar.cancel.rest.png", UriKind.Relative));
            appbarCancel.Text = "Cancel";
            appbarCancel.Click += (s, a) => restore();

            appbarSave.Click += (s, a) =>
            {
                if (editbox.Text != rawContent)
                {
                    editbox.IsEnabled = false;
                    appbarSave.IsEnabled = false;
                    appbarDelete.IsEnabled = false;
                    appbarCancel.IsEnabled = false;

                    App.Progress.IsIndeterminate = true;
                    App.Progress.Text = "Saving comment...";
                    App.Progress.IsVisible = true;

                    Comment dataContext = DataContext as Comment;
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
                    }, new EditCommentObject(dataContext.ID, editbox.Text));
                }
                else
                    restore();
            };

            appbarDelete.Click += (s, a) =>
            {
                appbarSave.IsEnabled = false;
                appbarDelete.IsEnabled = false;
                appbarCancel.IsEnabled = false;

                delete();
                restore();
            };

            myAppbar.Buttons.Add(appbarSave);
            myAppbar.Buttons.Add(appbarDelete);
            myAppbar.Buttons.Add(appbarCancel);
            currentPage.ApplicationBar = myAppbar;
            currentPage.BackKeyPress += (s, a) =>
            {
                if (editbox.Text != rawContent &&
                    MessageBox.Show("Do you want to go back without saving your comment?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    a.Cancel = true;
                else
                    restore();
            };
            editbox.Focus();
        }

        private void delete()
        {
            new Request("POST", APIs.DeleteComment, result =>
            {
                if (result.Success)
                    Visibility = Visibility.Collapsed;
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google, app will exit now.", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            }, new DeleteCommentObject((DataContext as Comment).ID).ToString());
        }
    }
}
