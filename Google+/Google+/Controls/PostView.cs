using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Google_Plus.Types;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;
using System.ComponentModel;

namespace Google_Plus.Controls
{
    public class PostView : Control
    {
        private const string AuthorPhotoLink = "AuthorPhotoLink";
        private HyperlinkButton _authorPhotoLink;

        private const string AuthorPhoto = "AuthorPhoto";
        private Image _authorPhoto;

        private const string AuthorNameLink = "AuthorNameLink";
        private HyperlinkButton _authorNameLink;

        private const string AuthorName = "AuthorName";
        private TextBlock _authorName;

        private const string Information = "Information";
        private TextBlock _information;

        private const string ContentChatBubble = "ContentChatBubble";
        private ChatBubble _contentChatBubble;
        private RichTextBox _contentCache;

        private const string Copy = "Copy";
        private MenuItem _copy;

        private const string ShareBlock = "ShareBlock";
        private Grid _shareBlock;

        private const string OriginalAuthorPhotoLink = "OriginalAuthorPhotoLink";
        private HyperlinkButton _originalAuthorPhotoLink;

        private const string OriginalAuthorPhoto = "OriginalAuthorPhoto";
        private Image _originalAuthorPhoto;

        private const string OriginalAuthorNameLink = "OriginalAuthorNameLink";
        private HyperlinkButton _originalAuthorNameLink;

        private const string OriginalAuthorName = "OriginalAuthorName";
        private TextBlock _originalAuthorName;

        private const string OriginalContentChatBubble = "OriginalContentChatBubble";
        private ChatBubble _originalContentChatBubble;
        private RichTextBox _originalContentCache;

        private const string CopyOriginal = "CopyOriginal";
        private MenuItem _copyOriginal;

        private const string Link = "Link";
        private HyperlinkButton _link;

        private const string Icon = "Icon";
        private Image _icon;

        private const string LineOne = "LineOne";
        private TextBlock _lineOne;

        private const string LineTwo = "LineTwo";
        private TextBlock _lineTwo;

        private const string TagBlock = "TagBlock";
        private WrapPanel _tagBlock;

        private const string Tag = "Tag";
        private TextBlock _tag;

        private const string PlusOne = "PlusOne";
        private Button _plusOne;

        private const string Reshare = "Reshare";
        private Button _reshare;

        private const string ReshareCount = "ReshareCount";
        private TextBlock _reshareCount;

        private const string CommentCount = "CommentCount";
        private TextBlock _commentCount;

        private const string _profilePage = "//Pages/ProfilePage.xaml?uid={0}";

        private static PhoneApplicationFrame _rootVisual = App.Current.RootVisual as PhoneApplicationFrame;

        public PostView()
        {
            DefaultStyleKey = typeof(PostView);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Post dataContext = DataContext as Post;

            Uri authorLink = new Uri(_profilePage.FormatWith(dataContext.AuthorID), UriKind.Relative);

            _authorPhotoLink = GetTemplateChild(AuthorPhotoLink) as HyperlinkButton;
            _authorPhotoLink.NavigateUri = authorLink;

            _authorPhoto = GetTemplateChild(AuthorPhoto) as Image;
            _authorPhoto.Source = new BitmapImage(dataContext.AuthorPhoto);

            _authorNameLink = GetTemplateChild(AuthorNameLink) as HyperlinkButton;
            _authorNameLink.NavigateUri = authorLink;

            _authorName = GetTemplateChild(AuthorName) as TextBlock;
            _authorName.Text = dataContext.Author;

            _information = GetTemplateChild(Information) as TextBlock;
            _information.Text = dataContext.Information;

            if (dataContext.HasContent)
            {
                _contentChatBubble = GetTemplateChild(ContentChatBubble) as ChatBubble;
                _contentChatBubble.Visibility = Visibility.Visible;
                if (_contentCache == null)
                {
                    _contentCache = new RichTextBox();
                    _contentCache.FontSize = (double)Application.Current.Resources["PhoneFontSizeMedium"];
                    _contentCache.TextWrapping = TextWrapping.Wrap;
                    _contentCache.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    _contentCache.Blocks.Add(RichTextHelper.Clone(dataContext.Content));
                }
                _contentChatBubble.Content = _contentCache;

                _copy = GetTemplateChild(Copy) as MenuItem;
                _copy.Click += copy;
            }

            if (dataContext.IsReshare)
            {
                _shareBlock = GetTemplateChild(ShareBlock) as Grid;
                _shareBlock.Visibility = Visibility.Visible;

                Uri originalAuthorLink = new Uri(_profilePage.FormatWith(dataContext.OriginalAuthorID), UriKind.Relative);

                _originalAuthorPhotoLink = GetTemplateChild(OriginalAuthorPhotoLink) as HyperlinkButton;
                _originalAuthorPhotoLink.NavigateUri = originalAuthorLink;

                _originalAuthorPhoto = GetTemplateChild(OriginalAuthorPhoto) as Image;
                _originalAuthorPhoto.Source = new BitmapImage(dataContext.OriginalAuthorPhoto);

                _originalAuthorNameLink = GetTemplateChild(OriginalAuthorNameLink) as HyperlinkButton;
                _originalAuthorNameLink.NavigateUri = originalAuthorLink;

                _originalAuthorName = GetTemplateChild(OriginalAuthorName) as TextBlock;
                _originalAuthorName.Text = dataContext.OriginalAuthor;

                if (dataContext.HasOriginalContent)
                {
                    _originalContentChatBubble = GetTemplateChild(OriginalContentChatBubble) as ChatBubble;
                    _originalContentChatBubble.Visibility = Visibility.Visible;
                    if (_originalContentCache == null)
                    {
                        _originalContentCache = new RichTextBox();
                        _originalContentCache.FontSize = (double)Application.Current.Resources["PhoneFontSizeMedium"];
                        _originalContentCache.TextWrapping = TextWrapping.Wrap;
                        _originalContentCache.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        _originalContentCache.Blocks.Add(RichTextHelper.Clone(dataContext.OriginalContent));
                    }
                    _originalContentChatBubble.Content = _originalContentCache;

                    _copyOriginal = GetTemplateChild(CopyOriginal) as MenuItem;
                    _copyOriginal.Click += copyOriginal;
                }
            }

            if (dataContext.Media.HasLineOne)
            {
                _link = GetTemplateChild(Link) as HyperlinkButton;
                _link.Visibility = Visibility.Visible;
                _link.NavigateUri = dataContext.Media.LinkLocation;

                _icon = GetTemplateChild(Icon) as Image;
                _icon.Source = new BitmapImage(dataContext.Media.Icon);

                _lineOne = GetTemplateChild(LineOne) as TextBlock;
                _lineOne.Text = dataContext.Media.LineOne;
            }
            _lineTwo = GetTemplateChild(LineTwo) as TextBlock;
            _lineTwo.Text = dataContext.Media.LineTwo;

            if (dataContext.Tag != null)
            {
                _tagBlock = GetTemplateChild(TagBlock) as WrapPanel;
                _tagBlock.Visibility = Visibility.Visible;

                _tag = GetTemplateChild(Tag) as TextBlock;
                _tag.Text = dataContext.Tag;
            }

            _plusOne = GetTemplateChild(PlusOne) as Button;
            _plusOne.Content = "+{0}".FormatWith(dataContext.PlusOneCount);
            _plusOne.Click += (s, e) => dataContext.PlusOne();
            _plusOne.Tap += handleEvent;
            _plusOne.Foreground = dataContext.PlusOneButtonBrush;

            _reshare = GetTemplateChild(Reshare) as Button;
            _reshare.Click += (s, e) => _rootVisual.Navigate(new Uri("//Pages/NewPost.xaml?pid={0}".FormatWith(dataContext.ID), UriKind.Relative));
            _reshare.Tap += handleEvent;

            _reshareCount = GetTemplateChild(ReshareCount) as TextBlock;
            _reshareCount.Text = dataContext.ReshareCount.ToString();

            _commentCount = GetTemplateChild(CommentCount) as TextBlock;
            _commentCount.Text = dataContext.CommentCount.ToString();

            dataContext.PropertyChanged += dataContent_PropertyChanged;
        }

        void copy(object sender, RoutedEventArgs e)
        {
            _contentCache.SelectAll();
            Clipboard.SetText(_contentCache.Selection.Text);
            _contentCache.Selection.Select(_contentCache.ContentStart, _contentCache.ContentStart);

            new ToastPrompt { Title = "Copied to clipboard." }.Show();
        }

        void copyOriginal(object sender, RoutedEventArgs e)
        {
            _originalContentCache.SelectAll();
            Clipboard.SetText(_originalContentCache.Selection.Text);
            _contentCache.Selection.Select(_originalContentCache.ContentStart, _originalContentCache.ContentStart);

            new ToastPrompt { Title = "Copied to clipboard." }.Show();
        }

        void handleEvent(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
        }

        void dataContent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                default:
                    break;
            }
        }
    }
}
