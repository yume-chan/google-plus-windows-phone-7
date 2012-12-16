using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Google_Plus.DataObject;
using Newtonsoft.Json.Linq;

namespace Google_Plus.Types
{
    public class Post : INotifyPropertyChanged
    {
        public string ID { get; set; }

        public Uri AuthorPhoto { get; set; }
        public string Author { get; set; }
        public string AuthorID { get; set; }

        public string TimeStamp { get; set; }
        public string Client { get; set; }
        public string ShareRange { get; set; }
        public string Information { get { return "{0}{1} - {2} - {3}".FormatWith(TimeStamp, Edited ? " (edited)" : "", Client, ShareRange); } }
        public string Tag { get; set; }

        public Paragraph Content { get; set; }
        public bool HasContent { get; set; }
        public string RawContent { get; set; }
        public bool Edited { get; set; }

        public bool IsReshare { get; set; }
        public Uri OriginalAuthorPhoto { get; set; }
        public string OriginalAuthor { get; set; }
        public string OriginalAuthorID { get; set; }
        public bool HasOriginalContent { get; set; }
        public Paragraph OriginalContent { get; set; }

        public Media Media { get; set; }

        private int plusOneCount = 0;
        public int PlusOneCount
        {
            get { return plusOneCount; }
            set { if (plusOneCount != value) { plusOneCount = value; NotifyPropertyChanged("PlusOneCount"); } }
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
        public static readonly SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red), DefaultBrush = (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
        public SolidColorBrush PlusOneButtonBrush { get { return doIPlusOne ? RedBrush : DefaultBrush; } }
        public int ReshareCount { get; set; }
        public int CommentCount { get; set; }

        public ThreadSafeObservableCollection<Comment> Replies { get; set; }

        public bool CanMention { get; set; }

        public Post()
        {
            Replies = new ThreadSafeObservableCollection<Comment>();
        }

        public Post(JToken post, int maxContentLength = -1, int maxContentLineCount = -1, bool fullInfo = true)
            : this()
        {
            DateTime timeStamp;
            if (post[70].Type != JTokenType.Undefined)
            {
                Edited = true;
                timeStamp = Utils.ToDateTime((double)post[70] / 1000);
            }
            else
            {
                Edited = false;
                timeStamp = Utils.ToDateTime((double)post[5]);
            }

            ID = (string)post[8];
            AuthorPhoto = new Uri(((string)post[18]).Replace("photo.", "s64-c-k/photo."), UriKind.Absolute);
            Author = (string)post[3];
            AuthorID = (string)post[16];

            if (fullInfo)
            {
                TimeStamp = timeStamp.Date == DateTime.Now.Date ? timeStamp.ToShortTimeString() : timeStamp.ToShortDateString();
                Client = (string)post[2] == "Google+" ? "Desktop" : (string)post[2];
                ShareRange = (int)post[32] == 0 ? "Limited" : "Public";
                if (post[82][2][7][0][0].Type != JTokenType.Undefined)
                    Tag = (string)post[82][2][7][0][0];
            }

            if (post[44].Type != JTokenType.Undefined)
            {
                string rawContent = (string)post[47];
                HasContent = rawContent != "";
                Content = Utils.ProcessRawContent(rawContent, maxContentLength, maxContentLineCount);
                IsReshare = true;
                OriginalAuthorPhoto = new Uri(((string)post[44][4]).Replace("photo.", "s48-c-k/photo."), UriKind.Absolute);
                OriginalAuthor = (string)post[44][0];
                OriginalAuthorID = (string)post[44][1];
                rawContent = (string)post[4];
                HasOriginalContent = rawContent != "";
                OriginalContent = Utils.ProcessRawContent(rawContent, maxContentLength, maxContentLineCount);
            }
            else
            {
                string rawContent = post[47].Type == JTokenType.Undefined ? (string)post[4] : (string)post[47];
                HasContent = rawContent != "";
                Content = Utils.ProcessRawContent(rawContent, maxContentLength, maxContentLineCount);
                IsReshare = false;
            }

            Media = new Media(post[97]);

            if (fullInfo)
            {
                PlusOneCount = (int)post[73][16];
                if (PlusOneCount > 0 && (int)post[73][13] == 1)
                    DoIPlusOne = true;
                ReshareCount = ((JArray)post[25]).Count;
                CommentCount = (int)post[93];

                foreach (JToken reply in post[7])
                    Replies.Add(new Comment(reply, maxContentLength, maxContentLineCount));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
                App.UIDispatcher.BeginInvoke(() => handler(this, new PropertyChangedEventArgs(propertyName)));
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
            }, new PlusOnePostObject(ID, !doIPlusOne).ToString());
        }
    }
}
