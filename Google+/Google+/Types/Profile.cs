using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace Google_Plus.Types
{
    public class Profile : INotifyPropertyChanged
    {
        public string FullName { get; set; }
        public string Tagline { get; set; }
        public string Occupation { get; set; }
        public ThreadSafeObservableCollection<Education> Education { get; set; }
        public string Introduction { get; set; }
        public string PlacesLivedMap { get; set; }
        public string Birthday { get; set; }
        public string Gender { get; set; }
        public ThreadSafeObservableCollection<OtherProfile> OtherProfiles { get; set; }

        public ThreadSafeObservableCollection<Post> Posts { get; set; }

        public Profile(JToken profile)
        {
            FullName = (string)profile[2][4][3];
            Tagline = (string)profile[2][33][1];
            Occupation = (string)profile[2][6][1];
            Education = new ThreadSafeObservableCollection<Education>();
            foreach (JToken education in profile[2][8][1])
                Education.Add(new Education(education));
            Introduction = (string)profile[2][14][1];
            PlacesLivedMap = "https:" + (string)profile[2][10];
            Birthday = (string)profile[2][16][1];
            if (profile[2][17][1].Type != JTokenType.Undefined)
            {
                switch ((int)profile[2][17][1])
                {
                    case 1:
                        Gender = "Male";
                        break;
                    case 2:
                        Gender = "Female";
                        break;
                    case 3:
                        Gender = "Other";
                        break;
                }
            }
            
            OtherProfiles = new ThreadSafeObservableCollection<OtherProfile>();
            foreach (JToken otherProfile in profile[2][51][0])
                OtherProfiles.Add(new OtherProfile(otherProfile));

            Posts = new ThreadSafeObservableCollection<Post>();
            foreach (JToken post in profile[4][0])
                Posts.Add(new Post(post, 200, 10));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Education
    {
        public string Name { get; set; }
        public string Major { get; set; }
        public string YearFrom { get; set; }
        public string YearTo { get; set; }
        public string Description { get { return "{0}{1}".FormatWith(Major != "" ? Major + ", " : "", YearFrom != null ? " {0} - {1}".FormatWith(YearFrom, YearTo) : ""); } }

        public Education(JToken education)
        {
            Name = (string)education[0];
            Major = (string)education[1];
            if (((JArray)education[2]).Count > 0)
            {
                YearFrom = ((int)education[2][0][2]).ToString();
                if (education[2][2].Type == JTokenType.Integer)
                    YearTo = "present";
                else
                    YearTo = ((int)education[2][1][2]).ToString();
            }
        }
    }

    public class OtherProfile
    {
        public string Icon { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }

        public OtherProfile(JToken profile)
        {
            Icon = "https:{0}".FormatWith(profile[2]);
            Link = (string)profile[1];
            Name = (string)profile[3];
        }
    }
}
