using System;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using System.Windows;
using Google_Plus.DataObject;

namespace Google_Plus.Types
{
    public class Comment : INotifyPropertyChanged
    {
        public string ID { get; set; }
        public bool IsMine { get; set; }

        public Uri AuthorPhoto { get; set; }
        public string Author { get; set; }
        public string AuthorID { get; set; }

        public string TimeStamp { get; set; }
        public string Information { get { return "{0}{1}{2} - ".FormatWith(PlusOneCount > 0 ? " - " : "", TimeStamp, Edited ? " (edited)" : ""); } }

        public Paragraph Content { get; set; }
        public string RawContent { get; set; }
        public bool Edited { get; set; }

        private int plusOneCount = 0;
        public int PlusOneCount
        {
            get { return plusOneCount; }
            set
            {
                bool hasPlusOne = plusOneCount > 0;
                if (plusOneCount != value)
                {
                    plusOneCount = value;
                    NotifyPropertyChanged("PlusOneCount");
                    if (plusOneCount > 0 != hasPlusOne)
                    {
                        NotifyPropertyChanged("HasPlusOne");
                        NotifyPropertyChanged("Information");
                    }
                }
            }
        }
        private bool doIPlusOne = false;
        public bool DoIPlusOne
        {
            get { return doIPlusOne; }
            set
            {
                if (doIPlusOne != value)
                {
                    doIPlusOne = value;
                    NotifyPropertyChanged("DoIPlusOne");
                    NotifyPropertyChanged("PlusOneButtonBrush");
                }
            }
        }
        public bool HasPlusOne { get { return plusOneCount != 0; } }
        public static readonly SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red), DefaultBrush = (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
        public SolidColorBrush PlusOneButtonBrush { get { return doIPlusOne ? RedBrush : DefaultBrush; } }

        public bool CanMention { get; set; }

        public Comment() { }

        public Comment(JToken comment, int maxContentLength = -1, int maxContentLineCount = -1)
        {
            DateTime timeStamp;
            if ((double)comment[14] != 0)
            {
                Edited = true;
                timeStamp = Utils.ToDateTime((double)comment[14]);
            }
            else
            {
                Edited = false;
                timeStamp = Utils.ToDateTime((double)comment[3]);
            }

            ID = (string)comment[4];
            if (comment[12].Type == JTokenType.Integer)
                IsMine = true;

            AuthorPhoto = new Uri(((string)comment[16]).Replace("photo.", "s48-c-k/photo."), UriKind.Absolute);
            Author = (string)comment[1];
            AuthorID = (string)comment[6];

            TimeStamp = timeStamp.Date == DateTime.Now.Date ? timeStamp.ToShortTimeString() : timeStamp.ToShortDateString();

            Content = Utils.ProcessRawContent((string)comment[2], maxContentLength, maxContentLineCount);
            if (IsMine)
                RawContent = (string)comment[5];

            PlusOneCount = comment[15][16].Type != JTokenType.Undefined ? (int)comment[15][16] : 0;
            if (PlusOneCount > 0 && (int)comment[15][13] == 1)
                DoIPlusOne = true;
        }

        public void PlusOne()
        {
            new Request("POST", APIs.PlusOne, result =>
            {
                if (result.Success)
                {
                    PlusOneCount += !doIPlusOne ? 1 : -1;
                    DoIPlusOne = !doIPlusOne;
                }
                else
                {
                    MessageBox.Show("Please check your network", "Can't reach Google, app will exit now.", MessageBoxButton.OK);
                    throw new Exception("ExitAppException");
                }
            }, new PlusOneCommentObject(ID, !doIPlusOne).ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
                App.UIDispatcher.BeginInvoke(() => handler(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
