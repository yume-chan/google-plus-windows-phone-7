using System.Windows;
using Newtonsoft.Json.Linq;
using System;

namespace Google_Plus.Types
{
    public class Media
    {
        public Uri Icon { get; set; }
        public ThreadSafeObservableCollection<Uri> Images { get; set; }

        public string LineOne { get; set; }
        public bool HasLineOne { get { return LineOne != null; } }

        public string LineTwo { get; set; }
        public Uri LinkLocation { get; set; }

        private const string prefix = "https:{0}";

        public Media() { }

        public Media(JToken media)
        {
            Images = new ThreadSafeObservableCollection<Uri>();
            if (media.Type != JTokenType.Undefined)
            {
                JToken mediaRoot;
                switch ((int)media[0][0])
                {
                    case 35:
                        mediaRoot = (JArray)media[2]["29646191"];
                        Icon = new Uri(prefix.FormatWith(mediaRoot[6]), UriKind.Absolute);
                        if (mediaRoot[5].Type == JTokenType.Array)
                            Images.Add(new Uri(prefix.FormatWith(((string)mediaRoot[5][0]).Replace("resize_h=150&resize_w=150", "resize_h=400&resize_w=400")), UriKind.Absolute));
                        LineOne = (string)mediaRoot[2];
                        LineTwo = (string)mediaRoot[3];
                        LinkLocation = new Uri((string)mediaRoot[0], UriKind.Absolute);
                        break;
                    case 249:
                        mediaRoot = (JArray)media[1]["27639957"];
                        Images.Add(new Uri(prefix.FormatWith(mediaRoot[0][5][0]), UriKind.Absolute));
                        break;
                    case 250:
                        if (media[1].Type == JTokenType.Object)
                            mediaRoot = (JArray)media[1]["27847199"];
                        else
                        {
                            mediaRoot = (JArray)media[2]["27847199"];
                            LineOne = (string)mediaRoot[1];
                            LinkLocation = new Uri("//Pages/Album.xaml?uid={0}&aid={1}".FormatWith(mediaRoot[4], mediaRoot[5]), UriKind.Relative);
                        }
                        foreach (JToken item in mediaRoot[3])
                            Images.Add(new Uri(prefix.FormatWith(item[0][5][0]), UriKind.Absolute));
                        break;
                }
            }
        }
    }
}
