using System;
using System.ComponentModel;
using Google_Plus.Types;
using System.Collections.Generic;


namespace Google_Plus
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Posts = new ThreadSafeObservableCollection<Post>();
            Notifications = new ThreadSafeObservableCollection<Notification>();
            Circles = new ThreadSafeObservableCollection<KeyValuePair<string, string>>();
            Circles.Add(new KeyValuePair<string, string>("Main", "1"));
            InYourCircles = new ThreadSafeObservableCollection<KeyValuePair<string, string>>();
        }

        public ThreadSafeObservableCollection<Post> Posts { get; set; }
        public ThreadSafeObservableCollection<Notification> Notifications { get; set; }
        public ThreadSafeObservableCollection<KeyValuePair<string, string>> Circles { get; set; }
        public ThreadSafeObservableCollection<KeyValuePair<string, string>> InYourCircles { get; set; }
        public bool NeedRefreash { get; set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}