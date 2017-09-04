using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Debug = UnityEngine.Debug;
using UnityEngine.Networking;

namespace Framework
{
    public class WebRequestImp : IHttpRequest
    {
        public Action<byte[]> OnSucceed { get; set; }
        public Action OnFailed { get; set; }

        public UnityWebRequest WebRequest;
        protected bool isDone;
        protected bool hasSend;
        protected Stopwatch clock = new Stopwatch();
        protected long responseTime;
        private static readonly string logFormat = @"[{6}] {0} [{1}] [{2} {3}] [{4}] [{5}ms]";
        private static readonly string[] sizes = { "B", "KB", "MB", "GB" };

        public string Url
        {
            get { return WebRequest.url; }
            set { WebRequest.url = value; }
        }

        public string Method
        {
            get { return WebRequest.method; }
            set { WebRequest.method = value; }
        }

        public void Reset()
        {
            hasSend = false;
            isDone = false;
            WebRequest = new UnityWebRequest
            {
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true
            };
        }

        public void Dispose()
        {
            WebRequest.Dispose();
            WebRequest = null;
        }

        public IEnumerator Send()
        {
            clock.Start();
            var uri = new Uri(WebRequest.url);
            string cookieHeader = Cookies.GetCookieHeader(uri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                AddHeader("cookie", cookieHeader);
            }
            if (!hasSend)
            {
                hasSend = true;
                yield return 1;
            }
            while (!isDone)
            {
                yield return WebRequest.Send();
                isDone = true;
            }
            if (!WebRequest.isError && Cookies != null)
            {
                string cookieStr = WebRequest.GetResponseHeader("Set-Cookie");
                if (!string.IsNullOrEmpty(cookieStr))
                    Cookies.SetCookies(uri, cookieStr);
            }
            responseTime = clock.ElapsedMilliseconds;
            clock.Reset();
            LogResponse();
        }

        public void AddHeader(string key, string value)
        {
            WebRequest.SetRequestHeader(key, value);
        }

        public void Abort()
        {
            WebRequest.Abort();
        }

        public CookieContainer Cookies { get; set; }

        public void CallBack()
        {
            if (!isDone)
                return;
            if (!WebRequest.isError)
            {
                if (OnSucceed != null)
                    OnSucceed(WebRequest.downloadHandler.data);
                OnSucceed = null;
            }
            else
            {
                if (OnFailed != null)
                    OnFailed();
                OnFailed = null;
            }
        }

        public void SetRawData(byte[] value)
        {
            WebRequest.uploadHandler = new UploadHandlerRaw(value);
        }


        private void LogResponse()
        {
            if (WebRequest == null || !WebRequest.isDone) return;
            double size = WebRequest.downloadedBytes;
            var order = 0;
            while (size >= 1024.0f && order + 1 < sizes.Length)
            {
                ++order;
                size /= 1024.0f;
            }
            var sizeString = String.Format("{0:0.##}{1}", size, sizes[order]);
            var status = WebRequest.responseCode;
            var message = string.IsNullOrEmpty(WebRequest.error) && status == 200 ? "OK" : WebRequest.error;
            if (WebRequest.isError || status != 200)
            {
                Debug.LogErrorFormat(logFormat, WebRequest.url, WebRequest.method, status, message, sizeString,
                    responseTime, "HTTPResponse");
            }
            else
            {
                Debug.LogFormat(logFormat, WebRequest.url, WebRequest.method, status, message, sizeString, responseTime, "HTTPResponse");
            }
        }
    }
}
