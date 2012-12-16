using System;
using System.Net;
using System.Windows;
using Google_Plus.DataObject;
using Google_Plus.Types;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.Collections.Generic;
using Coding4Fun.Phone.Controls;
using System.Windows.Controls;

namespace Google_Plus.Pages
{
    public partial class StreamPage : AnimatedRotatePhoneApplicationPage
    {
        public StreamPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            posts = ((MainViewModel)DataContext).Posts;
        }

        static ThreadSafeObservableCollection<Post> posts;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
        }

        private void loadStream()
        {
            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Loading...";
            App.Progress.IsVisible = true;
            appbarRefresh.IsEnabled = false;
            string circleId = ((MainViewModel)DataContext).Circles[CircleSelector.SelectedIndex].Value;
            GetActivitiesObject payload;
            if (circleId == "1")
                payload = new GetActivitiesObject(true);
            else
                payload = new GetActivitiesObject(circleId);
            new Request("POST", APIs.GetActivities, parseNewPosts, payload);
            new Request("POST", APIs.GUC, parseNotifications, new EmptyObject());
        }

        private void loadCircles()
        {
            CircleSelector.SelectedIndex = 0;
            new Request("GET", APIs.LookupCircles, result =>
            {
                if (result.Success)
                    foreach (JToken circle in result.ResponseObject[0][1][1])
                        if ((string)circle[0][0] != "15")
                            ((MainViewModel)DataContext).Circles.Add(new KeyValuePair<string, string>((string)circle[1][0], (string)circle[0][0]));
            });
            loadStream();
        }

        private void parseNewPosts(RequestCompletedArgs result)
        {
            if (result.Success)
            {
                posts.Clear();
                list = (JArray)result.ResponseObject[0][1][1][0];
                addPosts();
                App.Progress.IsVisible = false;
                appbar.IsVisible = true;
                appbarRefresh.IsEnabled = true;
                App.Settings.AddOrUpdateValue("LastUpdate", DateTime.Now);
            }
            else
            {
                MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                throw new Exception("ExitAppException");
            }
        }

        JArray list;
        private void addPosts(int index = 0)
        {
            /*
            foreach (JToken post in list)
                posts.Add(new Post(post, 200, 10));
            */
            
            App.UIDispatcher.BeginInvoke(() =>
            {
                posts.Add(new Post(list[index], 200, 10));
                if (++index < list.Count)
                    addPosts(index);
            });
            
        }

        private void parseNotifications(RequestCompletedArgs result)
        {
            if (result.Success)
            {
                int unreadCount = (int)result.ResponseObject[0][1][1];
                if (unreadCount > 0)
                    new ToastPrompt { Title = "You have {0} new notificaion{1}.".FormatWith(unreadCount, unreadCount > 1 ? "s" : "") }.Show();
                if (unreadCount > 9)
                    unreadCount = 10;
                appbarNotification.IconUri = new Uri("//Images/simon.notification.{0}.png".FormatWith(unreadCount.ToString()), UriKind.Relative);
            }
            else
            {
                MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                throw new Exception("ExitAppException");
            }
        }

        private void appbarNewPost_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("//Pages/NewPost.xaml", UriKind.Relative));
        }

        private void appbarRefresh_Click(object sender, EventArgs e)
        {
            loadStream();
        }

        private void appbarProfile_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("//Pages/ProfilePage.xaml?uid={0}".FormatWith(App.Session.UserID), UriKind.Relative));
        }

        private void appbarNotification_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("//Pages/Notifications.xaml", UriKind.Relative));
        }

        private void appbarLogout_Click(object sender, EventArgs e)
        {
            App.Settings.AddOrUpdateValue("email", "");
            App.Settings.AddOrUpdateValue("password", "");
            App.Settings.Save();
            App.Cookies = new CookieContainer();
            NavigationService.Navigate(new Uri("//Pages/LoginPage.xaml", UriKind.Relative));
        }

        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("//Pages/PostDetail.xaml?pid=" + ((Google_Plus.Controls.PostView)sender).Tag, UriKind.Relative));
        }

        private void Pivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0:
                    PostsList.ItemsSource = ((MainViewModel)DataContext).Posts;
                    InYourCircles.ItemsSource = null;
                    break;
                case 1:
                    InYourCircles.ItemsSource = ((MainViewModel)DataContext).InYourCircles;
                    PostsList.ItemsSource = null;
                    break;
            }
        }

        private void ListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataLoaded)
                loadStream();
        }

        bool DataLoaded = false;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - App.Settings.GetValueOrDefault<DateTime>("LastUpdate", DateTime.MinValue)).TotalSeconds > 30 || ((MainViewModel)DataContext).NeedRefreash)
            {
                Action loadData = loadStream;
                if (!DataLoaded)
                {
                    loadData = loadCircles;
                    DataLoaded = true;
                }
                if (App.Settings.GetValueOrDefault<bool>("Logged", false))
                    loadData();
                else
                    App.SignedInCallback += loadData;
                ((MainViewModel)DataContext).NeedRefreash = false;
            }
        }
    }
}