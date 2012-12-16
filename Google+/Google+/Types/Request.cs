using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Google_Plus.DataObject;
using Newtonsoft.Json.Linq;

namespace Google_Plus.Types
{
    public class RequestCompletedArgs
    {
        public bool Success { get; private set; }
        public string RawData { get; private set; }
        public HttpWebResponse Response { get; private set; }
        public JToken ResponseObject { get; private set; }

        public RequestCompletedArgs(bool success, string rawData, HttpWebResponse response, JToken responseObject = null)
        {
            Success = success;
            RawData = rawData;
            Response = response;
            ResponseObject = responseObject;
        }
    }

    public class Request
    {
        private HttpWebRequest _request;
        private object _data;
        private Action<RequestCompletedArgs> _callback;

        public Request(string method, string uri, Action<RequestCompletedArgs> callback, object data = null, Dictionary<string, string> headers = null, bool allowAutoRedirect = true)
        {
            _data = data;
            _callback = callback;

            _request = (HttpWebRequest)WebRequest.Create(uri.Replace("{REQUEST_TIME}", (Utils.Time % 1000000).ToString()));
            _request.CookieContainer = App.Cookies;
            _request.AllowAutoRedirect = allowAutoRedirect;
            _request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64; rv:18.0) Gecko/18.0 Firefox/18.0";

            if (headers != null)
                foreach (string key in headers.Keys)
                    _request.Headers[key] = headers[key];

            BeginGetResponse(method.ToUpper() == "POST");
        }

        private void BeginGetResponse(bool isPost)
        {
            if (isPost)
            {
                _request.Method = "POST";
                _request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                _request.BeginGetRequestStream(new AsyncCallback(PostRequest), null);
            }
            else
                _request.BeginGetResponse(new AsyncCallback(HandleResponse), null);
        }

        private void PostRequest(IAsyncResult result)
        {
            using (Stream requestStream = _request.EndGetRequestStream(result))
            {
                if (_data is string)
                    using (StreamWriter writer = new StreamWriter(requestStream))
                        writer.Write((string)_data);
                else if (_data is RequestObjectBase)
                    using (StreamWriter writer = new StreamWriter(requestStream))
                        writer.Write(((RequestObjectBase)_data).ToString());
                else if (_data is Stream)
                    ((Stream)_data).CopyTo(requestStream);
            }
            _request.BeginGetResponse(new AsyncCallback(HandleResponse), null);
        }

        private void HandleResponse(IAsyncResult result)
        {
            try
            {
                if (_callback != null)
                    using (HttpWebResponse response = (HttpWebResponse)_request.EndGetResponse(result))
                    using (Stream responseStream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string rawData = reader.ReadToEnd();
                        RequestCompletedArgs args;
                        if (rawData.StartsWith(")]}'"))
                            args = new RequestCompletedArgs(true, rawData, response, JArray.Parse(rawData.Substring(6)));
                        else if (rawData.StartsWith("["))
                            args = new RequestCompletedArgs(true, rawData, response, JArray.Parse(rawData));
                        else
                            args = new RequestCompletedArgs(true, rawData, response);
                        App.UIDispatcher.BeginInvoke(() => _callback(args));
                    }
            }
            catch (WebException ex)
            {
                if (_callback != null && ex.Status == WebExceptionStatus.UnknownError)
                    using (Stream responseStream = ex.Response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string rawData = reader.ReadToEnd();
                        App.UIDispatcher.BeginInvoke(() => _callback(new RequestCompletedArgs(false, rawData, (HttpWebResponse)ex.Response)));
                    }
            }
        }
    }
}