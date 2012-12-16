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
using Microsoft.Phone.Controls;
using System.Windows.Navigation;

namespace Google_Plus.Types
{
    public class OrientationLockableTransitionFrame : TransitionFrame
    {
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            PhoneApplicationPage _newPage = (PhoneApplicationPage)newContent;
            if (_newPage is AnimatedRotatePhoneApplicationPage)
            {
                if (App.Settings.GetValueOrDefault<bool>("OrientationLocked", false))
                    if (oldContent != null)
                        _newPage.SupportedOrientations = ((PhoneApplicationPage)oldContent).SupportedOrientations;
                    else
                        _newPage.SupportedOrientations = SupportedPageOrientation.Portrait;
                else
                    _newPage.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
            }
        }
    }
}
