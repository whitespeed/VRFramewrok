using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Framework
{
    public class HttpRequester : Singleton<HttpRequester>
    {
        public override void OnInitialize()
        {
        }

        public override void OnUninitialize()
        {
        }

        protected const int MaxReqSize = 10;

        protected ObjectPool<WebRequestImp> requestPool = new ObjectPool<WebRequestImp>(MaxReqSize, 1);
        protected List<IHttpRequest> sendingQueue = new List<IHttpRequest>(MaxReqSize);
        protected List<IHttpRequest> waitingQueue = new List<IHttpRequest>(MaxReqSize*2);
        protected CookieContainer cookieJar = new CookieContainer();

        public void AddRequest(string url,
            Action<byte[]> OnSucceed,
            Action OnFailed,
            string method = "GET",
            byte[] rawData = null)
        {
            var req = requestPool.Allocate();
            req.Reset();
            req.Url = url;
            req.Method = method;
            req.OnSucceed = OnSucceed;
            req.OnFailed = OnFailed;
            req.Cookies = cookieJar;
            if (null != rawData)
                req.SetRawData(rawData);
            waitingQueue.Add(req);
            Debug.LogFormat("[HTTPRequest] {0}", url);
        }

        public void AddRequest(string url,
            Action<byte[]> OnSucceed,
            Action OnFailed,
            string method,
            WWWForm form)
        {
            var req = requestPool.Allocate();
            req.Reset();
            req.Url = url;
            req.Method = method;
            req.OnSucceed = OnSucceed;
            req.OnFailed = OnFailed;
            req.Cookies = cookieJar;
            if (null != form)
            {
                var e = form.headers.GetEnumerator();
                while (e.MoveNext())
                {
                    req.AddHeader(e.Current.Key, e.Current.Value);
                }
                req.SetRawData(form.data);
            }
            waitingQueue.Add(req);
            Debug.LogFormat("[HTTPRequest] {0}", url);
        }

        public void RemoveRequest(string url)
        {
            for (var i = 0; i < waitingQueue.Count; i++)
            {
                if (waitingQueue[i].Url != url)
                {
                    waitingQueue.RemoveAt(i);
                    break;
                }

            }
            for (var i = 0; i < sendingQueue.Count; i++)
            {
                if (sendingQueue[i].Url != url)
                {
                    sendingQueue[i].Abort();
                    break;
                }
            }

        }

        public bool IsRequesting(string url)
        {
            for (var i = 0; i < sendingQueue.Count; i++)
            {
                if (sendingQueue[i].Url == url)
                    return true;
            }
            for (var i = 0; i < waitingQueue.Count; i++)
            {
                if (waitingQueue[i].Url == url)
                    return true;
            }
            return false;
        }

        protected void Update()
        {
            if (null == sendingQueue || sendingQueue.Count >= MaxReqSize || waitingQueue.Count <= 0)
                return;

            var r = waitingQueue[0];
            sendingQueue.Add(r);
            waitingQueue.Remove(r);

            StartCoroutine(Send(r));
        }

        protected IEnumerator Send(IHttpRequest req)
        {
            yield return req.Send();
            sendingQueue.Remove(req);
            req.CallBack();
            req.Dispose();
            requestPool.Release((WebRequestImp) req);
            
        }
    }
}