
namespace Google_Plus.DataObject
{
    public class APIs
    {
        public static string
            Home = "https://plus.google.com",
            NewPost = "https://plus.google.com/_/sharebox/post/?spam=23&_reqid={REQUEST_TIME}&rt=j",
            GetActivity = "https://plus.google.com/_/stream/getactivity/?_reqid={REQUEST_TIME}&rt=j",
            GetActivities = "https://plus.google.com/_/stream/getactivities/?_reqid={REQUEST_TIME}&rt=j",
            PushInit = "https://talkgadget.google.com/talkgadget/channel/test",
            PushReceive = "https://talkgadget.google.com/talkgadget/channel/bind",
            InitImageUpload = "https://plus.google.com/_/upload/photos/resumable?authuser=0",
            UploadImage = "https://plus.google.com/_/upload/photos/resumable?file_id=000&upload_id={UPLOAD_ID}",
            Comment = "https://plus.google.com/_/stream/comment/?rt=j&_reqid={REQUEST_TIME}",
            PlusOne = "https://plus.google.com/_/plusone?_reqid={REQUEST_TIME}&rt=j",
            LinkPreview = "https://plus.google.com/_/sharebox/linkpreview/?c={LINK_LOCATION}&_reqid={REQUEST_TIME}&rt=j",
            GetNotificationsData = "https://plus.google.com/_/notifications/getnotificationsdata?rt=j&_reqid={REQUEST_TIME}",
            UpdateLastReadTime = "https://plus.google.com/_/notifications/updatelastreadtime?rt=j&_reqid={REQUEST_TIME}",
            GUC = "https://plus.google.com/_/n/guc?_reqid={REQUEST_TIME}&rt=j",
            GetProfile = "https://plus.google.com/_/profiles/get/{0}",
            EditComment = "https://plus.google.com/_/stream/editcomment/?_reqid={REQUEST_TIME}&rt=j",
            DeleteComment = "https://plus.google.com/_/stream/deletecomment/?_reqid={REQUEST_TIME}&rt=j",
            LookupCircles = "https://plus.google.com/_/socialgraph/lookup/circles/?ct=2&rt=j&m=true&tag=fg&_reqid={REQUEST_TIME}";
    }
}
