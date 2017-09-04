using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace Framework
{
    //Texture 管理
    public class TextureManager : VRSingleton<TextureManager>
    {
        protected TextureFileCache fileCache;
        protected TextureMemoryCache memoryCache;
        protected Texture2D DefaultTexture = null;
        protected MsgDispather<Texture> dispather;



        public override void OnInitialize()
        {
            fileCache = new TextureFileCache();
            memoryCache = new TextureMemoryCache();
            dispather = new MsgDispather<Texture>("TextureManager");
        }

        public override void OnUninitialize()
        {
        }

        protected void PostTextureMessage(string msg, Texture texture)
        {
            dispather.DispathMsg(msg, texture);
            dispather.ClearListeners(msg);
        }

        protected static string GetRequestUrl(string url, int maxWidth, int maxHeigh)
        {
            if (maxWidth <= 0 || maxHeigh <= 0)
            {
                return url;
            }
            if (false == url.StartsWith("http://") && false == url.StartsWith("https://"))
            {
                return url;
            }
            else
            {
                return url;
            }
        }

        public static TexureResult LoadAsync(string url, int width = 256, int heigh = 256)
        {
            //string requestUrl = GetRequestUrl(url, width, heigh);
            TexureResult result = new TexureResult();
            if (string.IsNullOrEmpty(url))
            {
                result.isDone = true;
                result.error = "Empty Url";
                result.result = Instance.DefaultTexture;
            }
            else
            {
                Instance.dispather.AddListener(url, (texture) =>
                {
                    if (texture == Instance.DefaultTexture)
                    {
                        result.error = "failed";
                    }
                    result.isDone = true;
                    result.result = texture;
                });
                Instance.LoadTexture(url, width, heigh);
            }

            return result;
        }

        public Texture FindInMemory(string url, int maxWidth = 256, int maxHeigh = 256)
        {
            bool fromWeb = url.StartsWith(@"http://") || url.StartsWith(@"https://");
            bool fromFile = url.StartsWith(@"file://");
            string requestUrl = fromWeb ? GetRequestUrl(url, maxWidth, maxHeigh) : url;
            if (memoryCache.Exists(requestUrl))
            {
                return memoryCache.Get(requestUrl);
            }
            else
            {
                return null;
            }
        }
        protected void LoadTexture(string url, int maxWidth = 256, int maxHeigh = 256)
        {
            bool fromWeb = url.StartsWith(@"http://") || url.StartsWith(@"https://");
            bool fromFile = url.StartsWith(@"file://");
            string requestUrl = fromWeb ? GetRequestUrl(url, maxWidth, maxHeigh) : url;

            //缓存中存在
            if (memoryCache.Exists(requestUrl))
            {
                PostTextureMessage(url, memoryCache.Get(requestUrl));
                return;
            }
            //本地存储存在
            if (fileCache.Exists(requestUrl))
            {
                LoadFromContainer(url, requestUrl);
            }
            //WEB请求
            else if (fromWeb)
            {
                LoadFromHttp(url, requestUrl);
            }
            //文件请求
            else if (fromFile)
            {
                StartCoroutine(LoadFromFile(url, requestUrl));
            }
            //Resource请求
            else
            {
                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(requestUrl))
                {
                    StartCoroutine(LoadFromRes(url, requestUrl));
                }
            }
        }

        protected void LoadFromContainer(string url, string requestUrl)
        {
            Texture t = fileCache.Get(requestUrl);
            if (t != null)
            {
                memoryCache.Add(requestUrl, t);
                PostTextureMessage(url, t);
            }
            else
            {
                PostTextureMessage(url, DefaultTexture);
            }
        }

        protected void LoadFromHttp(string url, string requestUrl)
        {
            if (HttpRequester.Instance.IsRequesting(requestUrl))
                return;
            HttpRequester.Instance.AddRequest(requestUrl,
                (bytes) =>
                {
                    Texture2D texture = null;
                    if (Application.platform == RuntimePlatform.Android && url != requestUrl)
                    {
                        texture = fileCache.GetCacheFromFileBuffer(bytes);
                    }
                    else
                    {
                        texture = TextureUtil.CreateEmptyTexture();
                        texture.LoadImage(bytes);
                    }
                    if (texture)
                    {
                        texture.name = url;
                        //container.SaveCacheAsync(requestUrl, bytes, texture.width, texture.height);
                        memoryCache.Add(requestUrl, texture);
                        PostTextureMessage(url, texture);
                    }
                    else
                    {
                        PostTextureMessage(url, DefaultTexture);
                    }
                },
                delegate
                {
                    Debug.LogError("[Texture] " + url);
                    PostTextureMessage(url, DefaultTexture);
                });
        }

        protected IEnumerator LoadFromRes(string url, string requestUrl)
        {
            var request = Resources.LoadAsync<Texture>(url);
            yield return request;
            if (request.asset != null)
            {
                Texture2D texture = (Texture2D)request.asset;
                texture.name = url;
                PostTextureMessage(url, texture);
            }
            else
            {
                Debug.LogError("[Texture] " + url);
                PostTextureMessage(url, DefaultTexture);
            }

        }
        protected IEnumerator LoadFromFile(string url, string requestUrl)
        {
            WWW www = new WWW(url);
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                Texture t = www.texture;
                t.name = url;
                memoryCache.Add(url, t);
                PostTextureMessage(url, t);
            }
            else
            {
                Debug.LogError("[Texture] " + url);
                PostTextureMessage(url, DefaultTexture);
            }
            www.Dispose();
        }

    }
}
