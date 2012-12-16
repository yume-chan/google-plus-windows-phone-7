using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using Google_Plus.DataObject;
using Google_Plus.Types;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using System.Windows.Navigation;

namespace Google_Plus
{
    public partial class Main : PhoneApplicationPage
    {
        public Main()
            : base()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            IDictionary<string, string> queryStrings = NavigationContext.QueryString;
            Action navigation;
            if (queryStrings.ContainsKey("FileId"))
                navigation = () => NavigationService.Navigate(new Uri("//Pages/NewPost.xaml?FileId={0}".FormatWith(queryStrings["FileId"]), UriKind.Relative));
            else
                navigation = () => NavigationService.Navigate(new Uri("//Pages/StreamPage.xaml", UriKind.Relative));
            if (!App.Settings.GetValueOrDefault<bool>("Logged", false))
                App.SignedInCallback += navigation;
            else
                navigation();
            App.SignIn();
        }
    }
}