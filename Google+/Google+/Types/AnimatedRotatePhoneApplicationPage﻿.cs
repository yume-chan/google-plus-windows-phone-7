using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;

namespace Google_Plus.Types
{
    public class AnimatedRotatePhoneApplicationPage﻿ : PhoneApplicationPage
    {
        PageOrientation lastOrientation;
        ApplicationBarMenuItem lockOrirentation = new ApplicationBarMenuItem();
        ApplicationBarMenuItem captureScreen = new ApplicationBarMenuItem();

        public AnimatedRotatePhoneApplicationPage()
            : base()
        {
            NavigationInTransition navigateInTransition = new NavigationInTransition();
            navigateInTransition.Backward = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardIn };
            navigateInTransition.Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn };
            TransitionService.SetNavigationInTransition(this, navigateInTransition);

            NavigationOutTransition navigateOutTransition = new NavigationOutTransition();
            navigateOutTransition.Backward = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardOut };
            navigateOutTransition.Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut };
            TransitionService.SetNavigationOutTransition(this, navigateOutTransition);

            OrientationChanged += new EventHandler<OrientationChangedEventArgs>(Page_OrientationChanged);
            lastOrientation = this.Orientation;

            Loaded += new RoutedEventHandler(Page_Loaded);

            // Fix bug with capturing screen in white theme
            Background = (Brush)Application.Current.Resources["PhoneBackgroundBrush"];
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.IsInDesignTool)
                return;
            
            SystemTray.SetProgressIndicator(this, App.Progress);

            if (ApplicationBar != null)
            {
                lockOrirentation.Text = App.Settings.GetValueOrDefault<bool>("OrientationLocked", false) ? "Unlock orientation" : "Lock orientation";
                lockOrirentation.Click += new EventHandler(lockOrirentation_Click);
                ApplicationBar.MenuItems.Add(lockOrirentation);

                captureScreen.Text = "Capture Screen";
                captureScreen.Click += new EventHandler(captureScreen_Click);
                ApplicationBar.MenuItems.Add(captureScreen);
            }
        }

        void lockOrirentation_Click(object sender, EventArgs e)
        {
            if (!App.Settings.GetValueOrDefault<bool>("OrientationLocked", false))
            {
                if (Orientation.IsPortrait())
                    SupportedOrientations = SupportedPageOrientation.Portrait;
                else
                    SupportedOrientations = SupportedPageOrientation.Landscape;
                App.Settings.AddOrUpdateValue("OrientationLocked", true);
                lockOrirentation.Text = "Unlock orientation";
            }
            else
            {
                SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
                App.Settings.AddOrUpdateValue("OrientationLocked", false);
                lockOrirentation.Text = "Lock orientation";
            }
            App.Settings.Save();
        }

        void captureScreen_Click(object sender, EventArgs e)
        {
            WriteableBitmap bitmap = new WriteableBitmap(this, null);
            string tempName = "temp.jpg";
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (store.FileExists(tempName))
                store.DeleteFile(tempName);
            using (IsolatedStorageFileStream fs = store.CreateFile(tempName))
            {
                bitmap.SaveJpeg(fs, bitmap.PixelWidth, bitmap.PixelHeight, 0, 80);
                fs.Seek(0, SeekOrigin.Begin);
                MediaLibrary library = new MediaLibrary();
                DateTime now = DateTime.Now;
                library.SavePicture("Google+ Capture {0} {1}.jpg".FormatWith(now.ToShortDateString(), now.ToShortTimeString()), fs);
            }
        }

        void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageOrientation newOrientation = e.Orientation;

            // Orientations are (clockwise) 'PortraitUp', 'LandscapeRight', 'LandscapeLeft'

            RotateTransition transitionElement = new RotateTransition();

            switch (newOrientation)
            {
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeRight:
                    // Come here from PortraitUp (i.e. clockwise) or LandscapeLeft?
                    if (lastOrientation == PageOrientation.PortraitUp)
                        transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In180Clockwise;
                    break;
                case PageOrientation.LandscapeLeft:
                    // Come here from LandscapeRight or PortraitUp?
                    if (lastOrientation == PageOrientation.LandscapeRight)
                        transitionElement.Mode = RotateTransitionMode.In180Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In90Clockwise;
                    break;
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                    // Come here from LandscapeLeft or LandscapeRight?
                    if (lastOrientation == PageOrientation.LandscapeLeft)
                        transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In90Clockwise;
                    break;
                default:
                    break;
            }

            // Execute the transition
            PhoneApplicationPage phoneApplicationPage = (PhoneApplicationPage)(((PhoneApplicationFrame)Application.Current.RootVisual)).Content;
            ITransition transition = transitionElement.GetTransition(phoneApplicationPage);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();

            lastOrientation = newOrientation;
        }
    }
}
