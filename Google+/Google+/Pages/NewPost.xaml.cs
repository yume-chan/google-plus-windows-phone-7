using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google_Plus.DataObject;
using Google_Plus.Types;
using ImageTools.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using System.Windows.Navigation;

namespace Google_Plus.Pages
{
    public partial class NewPost : AnimatedRotatePhoneApplicationPage
    {
        public NewPost()
            : base()
        {
            InitializeComponent();
        }

        JObject uploadImageObject;
        string reshareId;
        bool shareFromLibary;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            IDictionary<string, string> queryStrings = NavigationContext.QueryString;
            if (queryStrings.ContainsKey("FileId"))
            {
                shareFromLibary = true;
                ApplicationTitle.Text = "Share a picture";

                MediaLibrary library = new MediaLibrary();
                string filename = queryStrings["FileId"];
                Picture picture = library.GetPictureFromToken(filename);

                Action upload = () => uploadImage(picture.GetImage());
                if (App.Settings.GetValueOrDefault<bool>("Logged", false))
                    upload();
                else
                    App.SignedInCallback += upload;
            }
            else if (queryStrings.ContainsKey("pid"))
            {
                appbar.IsVisible = false;
                App.Progress.IsIndeterminate = true;
                App.Progress.Text = "Loading...";
                App.Progress.IsVisible = true;

                reshareId = queryStrings["pid"];
                new Request("POST", APIs.GetActivity, result =>
                {
                    if (result.Success)
                    {
                        Post orginalPost = new Post(JArray.Parse(result.RawData.Substring(6))[0][1][1], fullInfo: false);
                        if (orginalPost.IsReshare)
                        {
                            orginalPost.AuthorPhoto = orginalPost.OriginalAuthorPhoto;
                            orginalPost.Author = orginalPost.OriginalAuthor;
                            orginalPost.Content = orginalPost.OriginalContent;
                        }
                        DataContext = orginalPost;
                        App.Progress.IsVisible = false;
                        appbarShare.IsEnabled = true;
                        appbarAddPhoto.IsEnabled = false;
                        appbarAddLink.IsEnabled = false;
                        ApplicationBar.IsVisible = true;
                    }
                    else
                    {
                        MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                        throw new Exception("ExitAppException");
                    }
                }, new GetActivityObject(reshareId).ToString());
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            switch (App.Settings.GetValueOrDefault<int>("DefaultShareRange", 1))
            {
                case 3:
                    ShareRange.SelectedIndex = 2;
                    break;
                case 4:
                    ShareRange.SelectedIndex = 1;
                    break;
            }
        }

        private void appbarShare_Click(object sender, EventArgs e)
        {
            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Sharing post...";
            App.Progress.IsVisible = true;
            NewPostObject payload;
            string content = NewPostContent.Text;
            // .Replace(",", " @104172795430787913441 " /* +Xsummer Xia */).Replace("，", " @104172795430787913441 " /* +Xsummer Xia */)
            if (uploadImageObject == null)
                if (reshareId == null)
                    if (link == null)
                        payload = new NewPostObject(content);
                    else
                        payload = new NewPostObject(content, link);
                else
                    payload = new NewPostObject(content, reshareId);
            else
                payload = new NewPostObject(content, uploadImageObject);
            switch ((string)ShareRange.SelectedItem)
            {
                case "Extended circles":
                    payload.SetShareRange(4);
                    App.Settings.AddOrUpdateValue("DefaultShareRange", 4);
                    App.Settings.Save();
                    break;
                case "Your circles":
                    payload.SetShareRange(3);
                    App.Settings.AddOrUpdateValue("DefaultShareRange", 3);
                    App.Settings.Save();
                    break;
                default:
                    App.Settings.AddOrUpdateValue("DefaultShareRange", 1);
                    App.Settings.Save();
                    break;
            }
            new Request("POST", APIs.NewPost, result =>
            {
                if (result.Success)
                {
                    if (shareFromLibary)
                    {
                        MessageBox.Show("Post shared!");
                        throw new Exception("ExitAppException");
                    }

                    App.Progress.IsIndeterminate = false;
                    App.Progress.Text = "Post shared.";
                    App.Progress.IsVisible = true;
                    ((MainViewModel)App.ViewModel).NeedRefreash = true;
                }
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google, app will exit now.", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            }, payload.ToString());
            if (!shareFromLibary)
                NavigationService.GoBack();
        }

        private void NewPostContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uploadImageObject == null && reshareId == null && link == null)
                appbarShare.IsEnabled = !string.IsNullOrWhiteSpace(NewPostContent.Text);
        }

        private void appbarAddPhoto_Click(object sender, EventArgs e)
        {
            if (uploadImageObject == null)
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.ShowCamera = true;
                task.Completed += new EventHandler<PhotoResult>(choosePhoto_Completed);
                task.Show();
                appbarShare.IsEnabled = false;
                appbarAddPhoto.IsEnabled = false;
                appbarAddPhoto.Text = "Uploading";
                appbarAddLink.IsEnabled = false;
            }
            else
            {
                uploadImageObject = null;
                PreviewBitmap.UriSource = null;
                appbarShare.IsEnabled = !string.IsNullOrWhiteSpace(NewPostContent.Text);
                appbarAddPhoto.IsEnabled = true;
                appbarAddPhoto.IconUri = new Uri("//Images/appbar.addpicture.rest.png", UriKind.Relative);
                appbarAddPhoto.Text = "Add Photo";
                appbarAddLink.IsEnabled = true;
            }
        }

        void choosePhoto_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
                uploadImage(e.ChosenPhoto);
            else
            {
                appbarShare.IsEnabled = !string.IsNullOrWhiteSpace(NewPostContent.Text);
                appbarAddPhoto.IsEnabled = true;
                appbarAddPhoto.IconUri = new Uri("//Images/appbar.addpicture.rest.png", UriKind.Relative);
                appbarAddPhoto.Text = "Add Photo";
                appbarAddLink.IsEnabled = true;
            }
        }

        private void uploadImage(Stream imageStream)
        {
            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Uploading image...";
            App.Progress.IsVisible = true;

            appbarShare.IsEnabled = false;
            appbarAddPhoto.IsEnabled = false;
            appbarAddPhoto.Text = "Uploading";
            appbarAddLink.IsEnabled = false;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-GUploader-Client-Info", "mechanism=scotty xhr resumable; clientVersion=30546681");
            new Request("POST", APIs.InitImageUpload, result =>
            {
                if (result.Success)
                    initUploadImageCallback(imageStream, result);
                else
                    MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
            }, new InitImageUploadObject(imageStream.Length), headers);
        }

        private void initUploadImageCallback(Stream imageStream, RequestCompletedArgs result)
        {
            JObject obj = JObject.Parse(result.RawData);

            if (obj["sessionStatus"]["upload_id"].Type != JTokenType.Undefined)
            {
                new Request("POST", APIs.UploadImage.Replace("{UPLOAD_ID}", (string)obj["sessionStatus"]["upload_id"]), result2 =>
                {
                    if (result2.Success)
                        finishUploadImage(imageStream, result2);
                    else
                        MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                }, imageStream);
            }
        }

        private void finishUploadImage(Stream imageStream, RequestCompletedArgs result)
        {
            App.Progress.IsVisible = false;

            uploadImageObject = (JObject)JObject.Parse(result.RawData)["sessionStatus"]["additionalInfo"]["uploader_service.GoogleRupioAdditionalInfo"]["completionInfo"]["customerSpecificInfo"];
            PreviewBitmap.SetSource(imageStream);
            appbarShare.IsEnabled = true;
            appbarAddPhoto.IconUri = new Uri("//Images/appbar.delete.rest.png", UriKind.Relative);
            appbarAddPhoto.Text = "Delete Photo";
            appbarAddPhoto.IsEnabled = true;
            appbarAddLink.IsEnabled = false;
        }

        bool showPreview = false;
        Media link;

        private void appbarAddLink_Click(object sender, EventArgs e)
        {
            if (!showPreview)
            {
                appbarAddPhoto.IsEnabled = false;
                appbarAddLink.IconUri = new Uri("//Images/appbar.delete.rest.png", UriKind.Relative);
                appbarAddLink.Text = "Delete Link";
                AddLinkPanel.Visibility = Visibility.Visible;
                showPreview = true;
                LinkLocation.Text = "";
                LinkLocation.IsEnabled = true;
                GetPreview.IsEnabled = true;
            }
            else
            {
                appbarShare.IsEnabled = !string.IsNullOrWhiteSpace(NewPostContent.Text);
                appbarAddPhoto.IsEnabled = true;
                appbarAddLink.IconUri = new Uri("//Images/appbar.linkto.rest.png", UriKind.Relative);
                appbarAddLink.Text = "Add Link";
                AddLinkPanel.Visibility = Visibility.Collapsed;
                showPreview = false;
                link = null;
                DataContext = null;
            }
        }

        private void GetPreview_Click(object sender, RoutedEventArgs e)
        {
            GetLinkPreview();
        }

        private void LinkLocation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                GetLinkPreview();
        }

        private void GetLinkPreview()
        {
            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Loading...";
            App.Progress.IsVisible = true;
            LinkLocation.IsEnabled = false;
            GetPreview.IsEnabled = false;
            new Request("POST", APIs.LinkPreview.Replace("{LINK_LOCATION}", Uri.EscapeDataString(LinkLocation.Text)), result =>
            {
                if (result.Success)
                {
                    link = new Media(JArray.Parse(result.RawData.Substring(6))[0][1][5][0]);
                    DataContext = new Post() { Media = link };
                    AddLinkPanel.Visibility = Visibility.Collapsed;
                    appbarShare.IsEnabled = true;
                    App.Progress.IsVisible = false;
                }
                else
                    MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
            }, new LinkPreviewObject().ToString());
        }

        private void Page_BackKeyPress(object sender, CancelEventArgs e)
        {
            if (shareFromLibary == true)
                if (MessageBox.Show("Do you want to exist?", "Comfirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    e.Cancel = true;
                else
                    throw new Exception("ExitAppException");
            else if (NewPostContent.Text != "" || reshareId != null || uploadImageObject != null || link != null)
                if (MessageBox.Show("Do you want to go back without saving your post?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    e.Cancel = true;
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)sender;
            if (((AnimatedImage)stackPanel.Children[1]).Source == null)
                ((AnimatedImage)stackPanel.Children[0]).MaxWidth = ActualWidth - 20;
            else
                ((AnimatedImage)stackPanel.Children[0]).MaxWidth = ((AnimatedImage)stackPanel.Children[1]).MaxWidth = ActualWidth / 2 - 10;
        }
    }
}