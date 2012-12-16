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
using Microsoft.Phone.Shell;

namespace Google_Plus.Pages
{
    public partial class ProfilePage : AnimatedRotatePhoneApplicationPage
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("//Pages/PostDetail.xaml?pid=" + ((PostView)sender).Tag, UriKind.Relative));
        }

        private void AnimatedRotatePhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Loading...";
            App.Progress.IsVisible = true;
            string userId = NavigationContext.QueryString["uid"];
            new Request("GET", APIs.GetProfile.FormatWith(userId), result =>
            {
                if (result.Success)
                {
                    DataContext = new Profile(JArray.Parse(result.RawData.Substring(6))[0][1]);
                    App.Progress.IsVisible = false;
                }
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            });
        }
    }
}