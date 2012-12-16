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
using Microsoft.Phone.Controls;
using Google_Plus.Types;
using Google_Plus.DataObject;
using Newtonsoft.Json.Linq;

namespace Google_Plus.Pages
{
    public partial class Notifications : AnimatedRotatePhoneApplicationPage
    {
        public Notifications()
            : base()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Notification notification = (Notification)((FrameworkElement)sender).DataContext;
            notification.Unread = false;
            if (notification.Type == NotificationType.Post)
                NavigationService.Navigate(new Uri("//Pages/PostDetail.xaml?pid=" + notification.ID, UriKind.Relative));
        }

        private void AnimatedRotatePhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Loading...";
            App.Progress.IsVisible = true;
            new Request("POST", APIs.GetNotificationsData, parseNotifications, new GetNotificationsDataObject().ToString());
        }

        private void parseNotifications(RequestCompletedArgs result)
        {
            if (result.Success)
            {
                JArray array = (JArray)JArray.Parse(result.RawData.Substring(6))[0][1][1];

                new Request("POST", APIs.UpdateLastReadTime, result2 => { }, new UpdateLastReadTimeObject((long)(float)array[2]));

                int unreadCount = (int)array[7];
                ((MainViewModel)DataContext).Notifications.Clear();
                for (int i = 0; i < ((JArray)array[0]).Count; i++)
                {
                    JToken notification = array[0][i];
                    Notification n = new Notification(notification, i < unreadCount);
                    ((MainViewModel)DataContext).Notifications.Add(n);
                }
                App.Progress.IsVisible = false;
            }
            else
            {
                MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                throw new Exception("ExitAppException");
            }
        }
    }
}