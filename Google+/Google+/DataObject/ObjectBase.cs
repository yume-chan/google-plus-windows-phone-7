using System;
using System.Collections.Generic;
using Google_Plus.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Google_Plus.DataObject
{
    public abstract class RequestObjectBase
    {
        protected JToken obj;

        public override string ToString()
        {
            return "f.req=" + Uri.EscapeDataString(JsonConvert.SerializeObject(obj)) + "&at=" + App.Session.SessionID;
        }
    }

    public class EmptyObject : RequestObjectBase
    {
        public EmptyObject()
        {
            obj = new JArray();
        }
    }

    public class GetActivitiesObject : RequestObjectBase
    {
        object[] array = new object[27];

        public GetActivitiesObject(bool mainStream)
        {
            array[1] = array[0] = 1;
            array[7] = "social.google.com";
            array[15] = array[8] = new JArray();
            array[26] = new JArray((object)new JArray(5, 3, 4, 1));
            if (mainStream)
                obj = new JArray(new JArray(array), null, null, true);
        }

        public GetActivitiesObject(string circleId)
            : this(false)
        {
            array[1] = 2;
            array[3] = circleId;
            obj = new JArray((object)new JArray(array));
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class GetNotificationsDataObject : RequestObjectBase
    {
        public override string ToString()
        {
            return "f.req=%5Bnull%2C%5B%5D%2C5%2Cnull%2C%5B%5D%2Cnull%2Ctrue%2C%5B%5D%2Cnull%2Cnull%2Cnull%2Cnull%2C2%5D&at=" + App.Session.SessionID;
        }
    }

    public class UpdateLastReadTimeObject : RequestObjectBase
    {
        public UpdateLastReadTimeObject(long time)
        {
            obj = new JArray(time);
        }
    }

    public class PlusOnePostObject : RequestObjectBase
    {
        string array;

        public PlusOnePostObject(string postId, bool set)
        {
            array = "itemId=buzz%3A" + postId + "&set=" + set.ToString() + "&cdcx=b&cdcy=a&cdsx=16&cdsy=e&at=" + App.Session.SessionID;
        }

        public override string ToString()
        {
            return array;
        }
    }

    public class PlusOneCommentObject : RequestObjectBase
    {
        string array;

        public PlusOneCommentObject(string commentId, bool set)
        {
            array = "itemId=comment%3A" + commentId + "&set=" + set.ToString() + "&cdcx=b&cdcy=3&cdsx=18&cdsy=f&at=" + App.Session.SessionID;
        }

        public override string ToString()
        {
            return array;
        }
    }

    public class EditCommentObject : RequestObjectBase
    {
        public EditCommentObject(string commentId, string content)
        {
            obj = new JArray(null, commentId, content, null, null, 3);
        }
    }

    public class DeleteCommentObject : RequestObjectBase
    {
        string array;

        public DeleteCommentObject(string commmentId)
        {
            array = "commentId=" + Uri.EscapeDataString(commmentId) + "&at=" + App.Session.SessionID;
        }

        public override string ToString()
        {
            return array;
        }
    }

    public class GetActivityObject : RequestObjectBase
    {
        public GetActivityObject(string postId)
        {
            obj = new JArray(postId, null, null, null, 5);
        }
    }

    public class InitImageUploadObject : RequestObjectBase
    {
        public InitImageUploadObject(long length)
        {
            obj = new JObject(
                    new JProperty("protocolVersion", "0.8"),
                    new JProperty("createSessionRequest", new JObject(
                        new JProperty("fields", new JArray(
                            new JObject(
                                new JProperty("external", new JObject(
                                    new JProperty("name", "file"),
                                    new JProperty("filename", "image.jpg"),
                                    new JProperty("put", new JObject()),
                                    new JProperty("size", length)))),
                            new JObject(
                                new JProperty("inlined", new JObject(
                                    new JProperty("name", "batchid"),
                                    new JProperty("content", Utils.Time.ToString()),
                                    new JProperty("contentType", "text/plain")))),
                            new JObject(
                                new JProperty("inlined", new JObject(
                                    new JProperty("name", "client"),
                                    new JProperty("content", "sharebox"),
                                    new JProperty("contentType", "text/plain")))),
                            new JObject(
                                new JProperty("inlined", new JObject(
                                    new JProperty("name", "disable_asbe_notification"),
                                    new JProperty("content", "true"),
                                    new JProperty("contentType", "text/plain")))),
                            new JObject(
                                new JProperty("inlined", new JObject(
                                    new JProperty("name", "streamid"),
                                    new JProperty("content", "updates"),
                                    new JProperty("contentType", "text/plain")))),
                            new JObject(
                                new JProperty("inlined", new JObject(
                                    new JProperty("name", "use_upload_size_pref"),
                                    new JProperty("content", "true"),
                                    new JProperty("contentType", "text/plain")))),
                            new JObject(
                                new JProperty("inlined", new JObject(
                                    new JProperty("name", "album_abs_position"),
                                    new JProperty("content", "0"),
                                    new JProperty("contentType", "text/plain")))))))));
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(obj);
        }
    }

    public class NewPostObject : RequestObjectBase
    {
        object[] array = new object[38];

        public NewPostObject(string content)
        {
            array[0] = content;
            array[1] = "oz:{0}.{1}.1".FormatWith(App.Session.UserID, Utils.Time.ToString("x"));
            array[6] = "[]";
            array[9] = true;
            array[10] = array[14] = array[36] = new JArray();
            array[11] = array[16] = array[27] = array[28] = array[29] = false;

            object[] tem = new object[8];
            tem[7] = new JArray(new JArray("#GooglePlusForWP7"), 1, new JArray("#GooglePlusForWP7"));
            array[21] = new JArray(13, null, new JArray(tem));

            array[37] = new JArray(new JArray((object)new JArray(null, null, 1)), null);
        }

        public NewPostObject(string content, string reshare)
            : this(content)
        {
            array[2] = reshare;
            array[27] = array[28] = null;
        }

        public NewPostObject(string content, JObject imageData)
            : this(content)
        {
            string title = (string)imageData["title"];
            string url = (string)imageData["url"];
            int width = (int)imageData["width"];
            int height = (int)imageData["height"];
            string photoPageUrl = (string)imageData["photoPageUrl"];
            string albumID = (string)imageData["albumid"];
            string photoID = (string)imageData["photoid"];

            object[] image = new object[48];
            image[3] = "";
            image[5] = new JArray(null, url, width, height);
            image[9] = new JArray();
            image[21] = title;
            image[24] = new JArray(null, photoPageUrl, null, "image/jpeg", "image");
            image[41] = new JArray(new JArray(null, url, width, height), new JArray(null, url, width, height));
            image[47] = new JArray((object)new JArray(null, "albumid=" + albumID + "&photoid=" + photoID, "http://google.com/profiles/media/onepick_media_id", ""));

            array[4] = true;
            array[6] = JsonConvert.SerializeObject(new JArray(JsonConvert.SerializeObject(image)));
            array[32] = array[12] = null;
            array[34] = new JArray(new JArray(249, 18, 1, 0),
                null, null, null,
                new JObject(
                    new JProperty("27639957",
                        new JArray((object)new JArray(
                            photoPageUrl, title, "", url, null,
                            new JArray(url, width, height, null, null, null, null, height,
                                new JArray(1, url)),
                            null, width.ToString(), height.ToString(), width, height, null, "picasaweb.google.com"),
                            App.Session.UserID, null, photoID, null, null, url, photoPageUrl, null, photoPageUrl, null,
                            "albumid=" + albumID + "&photoid=" + photoID))));
        }

        public NewPostObject(string content, List<JObject> imageDatas)
            : this(content)
        {
            foreach (JObject data in imageDatas)
            {

            }
        }

        public NewPostObject(string content, Media linkData)
            : this(content)
        {
            string title = linkData.LineOne;
            string description = linkData.LineTwo;
            string location = linkData.LinkLocation.ToString();
            string iconUrl = linkData.Icon.ToString().Substring(6);
            string imageUrl = linkData.Images[0].ToString();

            object[] link = new object[48];
            link[3] = title;
            link[9] = new JArray();
            link[21] = description;
            link[24] = new JArray(null, location, null, "text/html", "document");
            link[41] = new JArray(new JArray(null, iconUrl, null, null),
                new JArray(null, iconUrl, null, null));
            link[47] = new JArray((object)new JArray(null, "", "http://google.com/profiles/media/provider", ""));

            object[] image = new object[48];
            image[5] = new JArray(null, imageUrl);
            image[9] = new JArray();
            object[] imageInfo = new object[14];
            imageInfo[1] = location;
            imageInfo[3] = "image/jpeg";
            imageInfo[12] = 800;
            imageInfo[13] = 470;
            image[24] = imageInfo;
            image[41] = new JArray(new JArray(null, imageUrl, null, null), new JArray(null, imageUrl, null, null));
            image[47] = new JArray((object)new JArray(null, "images", "http://google.com/profiles/media/provider", ""));

            array[6] = JsonConvert.SerializeObject(new JArray(JsonConvert.SerializeObject(link), JsonConvert.SerializeObject(image)));
            array[34] = new JArray(new JArray(35, 1, 0),
                location, null, null,
                new JObject(new JProperty("29646191",
                    new JArray(location, imageUrl, title, description, null,
                        new JArray("//images1-focus-opensocial.googleusercontent.com/gadgets/proxy?url=" + imageUrl,
                            300, 300, null, null, null, null, null, new JArray(3, "//images1-focus-opensocial.googleusercontent.com/gadgets/proxy?url=" + imageUrl)),
                            iconUrl, new JArray()))));
        }

        public void SetShareRange(int shareRange)
        {
            array[37] = new JArray(new JArray((object)new JArray(null, null, shareRange)), null);
        }

        public override string ToString()
        {
            obj = new JArray(array);
            return base.ToString();
        }
    }

    public class CommentObject : RequestObjectBase
    {
        public CommentObject(string postId, string commentContent)
        {
            obj = new JArray(postId, "os:" + postId + ":" + Utils.Time, commentContent, Utils.Time + 4000, null, null, 4);
        }
    }

    public class LinkPreviewObject : RequestObjectBase
    {
        public override string ToString()
        {
            return "susp=false&at=" + App.Session.SessionID;
        }
    }
}
