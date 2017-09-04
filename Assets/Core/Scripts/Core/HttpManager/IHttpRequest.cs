using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Framework
{
    public interface IHttpRequest
    {
        void SetRawData(byte[] data);
        Action<byte[]> OnSucceed { get; set; }
        Action OnFailed { get; set; }
        string Url { get; set; }
        string Method { get; set; }
        void AddHeader(string key, string value);
        void Reset();
        IEnumerator Send();
        void CallBack();
        void Dispose();
        void Abort();
        CookieContainer Cookies { get; set; }
    }
}
