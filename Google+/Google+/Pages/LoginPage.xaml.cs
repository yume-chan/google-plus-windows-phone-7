using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Google_Plus.Types;
using Microsoft.Phone.Shell;
using System.Collections.Generic;

namespace Google_Plus
{
    public partial class LoginPage : AnimatedRotatePhoneApplicationPage
    {
        public LoginPage()
            : base()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
            IDictionary<string, string> queryString = NavigationContext.QueryString;
            if (queryString.ContainsKey("wrong"))
                tbWrong.Visibility = Visibility.Visible;
            inEmail.Text = App.Settings.GetValueOrDefault<string>("email", "");
            if (App.Settings.GetValueOrDefault<bool?>("staySignedIn", null) == false)
                cbStay.IsChecked = false;
        }

        public static void Login(string email, string password, Action<bool> callback)
        {
            new Request("GET", "https://accounts.google.com/ServiceLogin?hl=en&continue=http://www.google.com/", result =>
            {
                if (result.Success)
                {
                    int index = result.RawData.IndexOf("id=\"dsh\"") + 16;
                    string dsh = result.RawData.Substring(index, result.RawData.IndexOf("\"", index) - index);
                    index = result.RawData.IndexOf("name=\"GALX\"", index) + 28;
                    string galx = result.RawData.Substring(index, result.RawData.IndexOf("\"", index) - index);
                    string payload = "dsh=" + dsh +
                        "&GALX=" + galx +
                        "&Email=" + email +
                        "&Passwd=" + password +
                        "&PersistentCookie=yes&signIn=Sign in";

                    App.Cookies.Add(new Uri("https://accounts.google.com"), new Cookie("GALX", galx, "/", "google.com"));

                    new Request("POST", "https://accounts.google.com/ServiceLoginAuth", result2 =>
                    {
                        if (result2.Success)
                            callback(result2.Response.StatusCode == HttpStatusCode.Found);
                        else
                        {
                            MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                            throw new Exception("ExitAppException");
                        }
                    }, payload, allowAutoRedirect: false);
                }
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            });
        }

        private void login()
        {
            inEmail.IsEnabled = false;
            inPasswd.IsEnabled = false;
            cbStay.IsEnabled = false;
            btnSignin.IsEnabled = false;
            appbarSignin.IsEnabled = false;

            App.Progress.IsIndeterminate = true;
            App.Progress.Text = "Signing in...";
            App.Progress.IsVisible = true;

            Login(inEmail.Text, inPasswd.Password, success =>
            {
                App.Settings.AddOrUpdateValue("email", inEmail.Text);
                if (!(bool)cbStay.IsChecked)
                    App.Settings.AddOrUpdateValue("staySignedIn", false);
                App.Settings.Save();
                if (success)
                {
                    if ((bool)cbStay.IsChecked)
                        App.Settings.AddOrUpdateValue("password", inPasswd.Password);
                    App.Settings.Save();

                    App.Session = new Session(LoginCallback);
                }
                else
                {
                    inPasswd.Password = "";
                    tbWrong.Visibility = Visibility.Visible;
                    inEmail.IsEnabled = true;
                    inPasswd.IsEnabled = true;
                    cbStay.IsEnabled = true;
                    btnSignin.IsEnabled = true;
                    appbarSignin.IsEnabled = true;
                    App.Progress.IsVisible = false;
                }
            });
        }

        private void LoginCallback(bool success)
        {
            if (success)
            {
                App.Progress.IsVisible = false;
                ((MainViewModel)App.ViewModel).NeedRefreash = true;
                if (App.SignedInCallback != null)
                {
                    App.UIDispatcher.BeginInvoke(App.SignedInCallback);
                    App.SignedInCallback = null;
                }
            }
            else
            {
                inPasswd.Password = "";
                tbWrong.Visibility = Visibility.Visible;
                inEmail.IsEnabled = true;
                inPasswd.IsEnabled = true;
                cbStay.IsEnabled = true;
                btnSignin.IsEnabled = true;
                App.Progress.IsVisible = false;
            }
        }

        private void btnSignin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            login();
        }

        private void inPasswd_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.PlatformKeyCode == 10)
                login();
        }

        private void inEmail_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.PlatformKeyCode == 10)
                inPasswd.Focus();
        }

        private void cbStay_Checked(object sender, RoutedEventArgs e)
        {
            if (tbMsg != null)
                tbMsg.Visibility = Visibility.Visible;
        }

        private void cbStay_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tbMsg != null)
                tbMsg.Visibility = Visibility.Collapsed;
        }

        private void AnimatedRotatePhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Do you want to exist?", "Comfirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                e.Cancel = true;
        }

        private void TextChanged(object sender, RoutedEventArgs e)
        {
            appbarSignin.IsEnabled = btnSignin.IsEnabled = !string.IsNullOrWhiteSpace(inEmail.Text) && !string.IsNullOrWhiteSpace(inPasswd.Password);
        }

        private void appbarSignin_Click(object sender, EventArgs e)
        {
            login();
        }
    }
}