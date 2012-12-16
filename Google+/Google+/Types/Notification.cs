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
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Collections.Generic;

namespace Google_Plus.Types
{
    public class Notification : INotifyPropertyChanged
    {
        private bool unread = false;
        public bool Unread
        {
            get { return unread; }
            set
            {
                if (unread != value)
                {
                    unread = value;
                    NotifyPropertyChanged("Unread");
                    NotifyPropertyChanged("Background");
                }
            }
        }

        public NotificationType Type { get; set; }

        public string ID { get; set; }
        public Uri Image { get; set; }

        public static readonly SolidColorBrush GrayBrush = new SolidColorBrush(Colors.Gray), BlackBrush = new SolidColorBrush(Colors.Transparent);
        public SolidColorBrush Background { get { return unread ? GrayBrush : BlackBrush; } }

        public Paragraph LineOne { get; set; }
        public Paragraph LineTwo { get; set; }

        public ThreadSafeObservableCollection<string> Images { get; set; }

        public Notification(JToken notification, bool unread)
        {
            Unread = unread;
            Type = (NotificationType)(int)notification[1];
            ID = (string)notification[10];
            LineOne = new Paragraph();
            LineTwo = new Paragraph();
            Images = new ThreadSafeObservableCollection<string>();
            switch (Type)
            {
                case NotificationType.Post:
                    if (notification[18][0][0].Type != JTokenType.Undefined)
                    {
                        if (notification[18][0][7][1].Type != JTokenType.Undefined)
                        {
                            Post post = new Post(notification[18][0][0], 100, 1, false);
                            Image = post.AuthorPhoto;
                            LineOne.Inlines.Add(new Run() { Text = post.Author + ": " });
                            foreach (Inline inline in Utils.ProcessRawContent((string)notification[18][0][7][1]).Inlines)
                                if (inline is Run)
                                    LineOne.Inlines.Add(RichTextHelper.Clone((Run)inline));
                        }
                        if (notification[18][0][7][2].Type != JTokenType.Undefined)
                        {
                            string lineTwoId = (string)notification[18][0][7][3];
                            foreach (JToken block in notification[2])
                                foreach (JToken item in block[1])
                                {
                                    if ((string)item[2][3] == lineTwoId)
                                    {
                                        Image = new Uri("https:" + ((string)item[2][2]).Replace("photo.", "s64-c-k/photo."), UriKind.Absolute);
                                        LineTwo.Inlines.Add(new Run() { Text = (string)item[2][0], FontWeight = FontWeights.Bold });
                                        LineTwo.Inlines.Add(new Run() { Text = " - " });
                                        foreach (Inline inline in Utils.ProcessRawContent((string)notification[18][0][7][2]).Inlines)
                                            if (inline is Run)
                                                LineTwo.Inlines.Add(RichTextHelper.Clone((Run)inline));
                                        return;
                                    }
                                }
                        }
                        /*
                        foreach (JToken block in notification[2])
                        {
                            switch ((int)block[0])
                            {
                                case 4:
                                    foreach (JToken item in block[1])
                                    {
                                        switch ((int)item[1])
                                        {
                                            case 20:
                                                LineTwo.Inlines.Add(new Run() { Text = "+1  ", FontStyle = FontStyles.Italic, FontWeight = FontWeights.Bold });
                                                LineTwo.Inlines.Add(new Run() { Text = "your post by " + (string)item[2][0] + ". " });
                                                Image = "https:" + ((string)item[2][2]).Replace("photo.", "s64-c-k/photo.");
                                                break;
                                            case 21:
                                                LineTwo.Inlines.Add(new Run() { Text = "+1  ", FontStyle = FontStyles.Italic, FontWeight = FontWeights.Bold });
                                                LineTwo.Inlines.Add(new Run() { Text = "your comment by " + (string)item[2][0] + ". " });
                                                Image = "https:" + ((string)item[2][2]).Replace("photo.", "s64-c-k/photo.");
                                                break;
                                        }
                                    }
                                    break;
                                case 6:
                                    LineTwo.Inlines.Add(new Run() { Text = "1 new reshare ", FontStyle = FontStyles.Italic, FontWeight = FontWeights.Bold });
                                    LineTwo.Inlines.Add(new Run() { Text = "by " + (string)block[1][0][2][0] + "." });
                                    break;
                                case 11:
                                    foreach (JToken item in block[1])
                                    {
                                        switch ((int)item[1])
                                        {
                                            case 3:
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        */
                    }
                    else
                    {
                        LineOne.Inlines.Add(new Run() { Text = "{0} interacted on a post with you.".FormatWith(notification[2][0][1][0][2][0]) });
                        LineTwo.Inlines.Add(new Run() { Text = "The post no longer exists." });
                    }
                    break;
                case NotificationType.Circle:
                    foreach (JToken block in notification[2])
                    {
                        if ((int)block[0] != 7 && (int)block[0] != 21)
                            continue;
                        else
                        {
                            int count = ((JArray)block[1]).Count;
                            if ((int)block[0] == 7)
                                LineOne.Inlines.Add(new Run() { Text = "Added you on Google+ ({0}): ".FormatWith(count) });
                            else
                                LineOne.Inlines.Add(new Run() { Text = "Added you back on Google+ ({0}): ".FormatWith(count) });
                            Images.Clear();
                            if (count == 1)
                            {
                                LineOne.Inlines.Add(new Run() { Text = "{0}.".FormatWith(block[1][0][2][0]) });
                                Images.Add("https:{0}".FormatWith(((string)block[1][0][2][2]).Replace("photo.", "s64-c-k/photo.")));
                            }
                            else
                            {
                                for (int i = 0; i < count - 2; i++)
                                {
                                    LineOne.Inlines.Add(new Run() { Text = "{0}, ".FormatWith(block[1][i][2][0]) });
                                    Images.Add("https:{0}".FormatWith(((string)block[1][i][2][2]).Replace("photo.", "s64-c-k/photo.")));
                                }
                                LineOne.Inlines.Add(new Run() { Text = "{0} and {1}.".FormatWith(block[1][count - 2][2][0], block[1][count - 1][2][0]) });
                                Images.Add("https:{0}".FormatWith(((string)block[1][count - 2][2][2]).Replace("photo.", "s64-c-k/photo.")));
                                Images.Add("https:{0}".FormatWith(((string)block[1][count - 1][2][2]).Replace("photo.", "s64-c-k/photo.")));
                            }
                        }
                    }
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
                App.UIDispatcher.BeginInvoke(() => handler(this, new PropertyChangedEventArgs(propertyName)));
        }
    }

    public enum NotificationType
    {
        Post = 1,
        Circle = 2
    }
}
