using System.Text.RegularExpressions;
using System.Windows;
using System;
using System.Net;
using System.Collections.Generic;

namespace Google_Plus.Types
{
    public class Session
    {
        public string UserID { get; private set; }
        public string SessionID { get; private set; }

        private Action<bool> _callback;

        public Session(Action<bool> callback)
        {
            _callback = callback;
            new Request("GET", "https://plus.google.com/", HandleResponse, "");
        }

        void HandleResponse(RequestCompletedArgs result)
        {
            if (result.Success)
            {
                SessionID = Uri.EscapeDataString(Regex.Match(result.RawData, "AObGSA.*:\\d+").Value);
                UserID = Regex.Match(result.RawData, "oid=\"(\\d+)\"").Groups[1].Value;
                if (SessionID != "" && UserID != "")
                {
                    App.Settings.AddOrUpdateValue("Logged", true);
                    App.UIDispatcher.BeginInvoke(() => _callback(true));
                }
                else
                    App.UIDispatcher.BeginInvoke(() => _callback(false));
            }
            else
            {
                MessageBox.Show("Please check your network", "Can't reach Google, app will exit now.", MessageBoxButton.OK);
                throw new Exception("ExitAppException");
            }
        }
    }
}
