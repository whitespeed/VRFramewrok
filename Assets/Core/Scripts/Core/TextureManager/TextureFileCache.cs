#define USING_STACK
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using  Framework.LitJson;
using Framework;

namespace Framework
{
    public static class TextureUtil
    {
        public static Texture2D CreateEmptyTexture(int w = 0, int h = 0)
        {
            Texture2D texture = null;
#if UNITY_ANDROID
            texture = new Texture2D(w, h, TextureFormat.ETC_RGB4, false);
#elif UNITY_IOS
        texture = new Texture2D(w, h,TextureFormat.PVRTC_RGB4, false);
#else
        texture = new Texture2D(w, h,TextureFormat.RGB24, false);
#endif
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }
    }
    //Texture 存储信息
    public class TextureInfo
    {
        public string url;
        public int hashCode;
        public int width;
        public int height;

        //[NonSerialized]
        [LitJson.JsonIgnore]
        public byte[] data;
    }

    //Texture 文件缓存
    public class TextureFileCache
    {
        const string SAVE_PATH = "TextureCache";
        const string CONFIG_FILE = "info.json";
        const string TEXTURE_EXTENSION
#if !UNITY_EDITOR && UNITY_ANDROID
        = ".pkm";
#else
        = ".jpg";
#endif

        bool _isThreadRunning = false;

#if !UNITY_EDITOR && UNITY_ANDROID
    //ziyuan.liu: cache for Saving Pixels Data
    byte[] cacheBytes = new byte[256 * 1024 * 4];
#endif

        private string cacheDirectory = string.Empty;
        private string infoPath = string.Empty;
        List<TextureInfo> infos = new List<TextureInfo>();

        //ziyuan.liu: Async Save File
#if USING_STACK
        System.Collections.Stack _queue = new System.Collections.Stack();
        System.Collections.Stack _queueThread = new System.Collections.Stack();
#else
    System.Collections.Queue _queue = new System.Collections.Queue();
    System.Collections.Queue _queueThread = new System.Collections.Queue();
#endif

        public TextureFileCache()
        {
            InitPathAndConfig();
        }

        //图片缓存是否存在
        public bool Exists(string url)
        {
            for (int i = 0; i < infos.Count; ++i)
            {
                if (infos[i].url == url && File.Exists(GetUniquePath(infos[i].url)))
                    return true;
            }
            return false;
        }


        //获得缓存图片路径
        public string GetPath(string url)
        {
            for (int i = 0; i < infos.Count; ++i)
            {
                if (infos[i].url == url && File.Exists(GetUniquePath(url)))
                {
                    return GetUniquePath(url);
                }
            }
            return string.Empty;
        }

        public Texture2D GetCacheFromFileBuffer(byte[] fileData)
        {
#if !UNITY_EDITOR && UNITY_ANDROID

        if(fileData == null || fileData.Length <= 16) 
            return TextureUtil.CreateEmptyTexture();

        int w = (fileData[12] << 8) + fileData[13];
        int h = (fileData[14] << 8) + fileData[15];
        
        Texture2D texture = TextureUtil.CreateEmptyTexture(w, h);
        
        //Array.Clear(cacheBytes, 0, cacheBytes.Length);
        Buffer.BlockCopy(fileData, 16, cacheBytes, 0, fileData.Length - 16);
        
        texture.LoadRawTextureData(cacheBytes);
        texture.Apply(false, true);

        return texture;
#else
            return TextureUtil.CreateEmptyTexture();
#endif
        }

        //获得缓存的图片
        public Texture Get(string url)
        {
            string path = GetPath(url);
            if (!string.IsNullOrEmpty(path))
            {

                Texture2D tex = null;

                try
                {
#if !UNITY_EDITOR && UNITY_ANDROID
                TextureInfo info = GetInfo(url);
                tex = TextureUtil.CreateEmptyTexture(info.width, info.height);
                tex.LoadRawTextureData(File.ReadAllBytes(path));
                tex.Apply(false, true);
#else
                    tex = TextureUtil.CreateEmptyTexture();
                    tex.LoadImage(File.ReadAllBytes(path));
#endif
                    tex.name = url;
                    return tex;
                }
                catch
                {
                    Resources.UnloadAsset(tex);
                    tex = null;
                }
            }
            return null;
        }

        //获得图片信息
        public TextureInfo GetInfo(string url)
        {
            for (int i = 0; i < infos.Count; ++i)
            {
                if (infos[i].url == url)
                    return infos[i];
            }
            return null;
        }

        //清除缓存
        public IEnumerator ClearCache()
        {
            string[] configAndTextures = Directory.GetFiles(cacheDirectory);
            for (int i = 0; i < configAndTextures.Length; ++i)
            {
                File.Delete(configAndTextures[i]);
                yield return 1;
            }
            infos.Clear();
        }

        //保存图片bytes
        public void SaveAsync(string url, byte[] data, int w = 0, int h = 0)
        {
            if (!string.IsNullOrEmpty(url) && data != null)
            {
#if USING_STACK
                _queue.Push(new TextureInfo()
#else
            _queue.Enqueue(new TextureInfo()
#endif
                {
                    url = url,
                    data = data,
                    width = w,
                    height = h
                });

                if (!_isThreadRunning)
                {
                    _StartTaskFromQueue();
                }
            }
        }

        protected void _StartTaskFromQueue()
        {
            if (!_isThreadRunning && _queue.Count > 0)
            {
                _isThreadRunning = true;

                //ziyuan.liu: swap mainQueue and threadQueue, no Pop-Push
                var tQueue = _queue;
                _queue = _queueThread;
                _queueThread = tQueue;

                //ThreadTool.RunAsync(_SaveCacheInThread, null, 0.5f);
            }
        }

        protected void _SaveCacheInThread(object obj)
        {
            do
            {
#if USING_STACK
                TextureInfo info = _queueThread.Pop() as TextureInfo;
#else
            TextureInfo info = _queueThread.Dequeue() as TextureInfo
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
            int offset = 16;
#else
                int offset = 0;
#endif
                SaveTextureBytes(info.url, info.data, offset);
                SaveInfo(info.url, info.width, info.height);
            }
            while (_queueThread.Count > 0);

            //ThreadTool.QueueOnMainThread(_SaveCacheInThreadFinishInMain, null);
        }

        protected void _SaveCacheInThreadFinishInMain(object obj)
        {
            _isThreadRunning = false;
            _StartTaskFromQueue();
        }

        //初始化缓存配置
        protected void InitPathAndConfig()
        {
            //cacheDirectory = Path.Combine(VRApplication.PersistDataPath, SAVE_PATH);
            //if (!Directory.Exists(cacheDirectory))
            //    Directory.CreateDirectory(cacheDirectory);
            //infoPath = Path.Combine(cacheDirectory, CONFIG_FILE);
            ////文件不存在
            //if (!File.Exists(infoPath))
            //    return;
            //LoomThreading.RunAsync(LoadConfigInThread, null);
        }

        protected void LoadConfigInThread(object o)
        {
            try
            {
                StreamReader streamReader = File.OpenText(infoPath);
                JsonReader r = new JsonReader(streamReader);
                try
                {
                    TextureInfo info = null;
                    do
                    {
                        info = JsonMapper.ToObject<TextureInfo>(r);
                        if (info != null)
                            infos.Add(info);
                    } while (info != null);
                }
                //Json反序列化失败,文件被篡改
                catch
                {
                    r.Close();
                    streamReader.Close();
                    streamReader.Dispose();
                    File.WriteAllText(infoPath, string.Empty);
                }
                r.Close();
                streamReader.Close();
                streamReader.Dispose();
            }
            //其他IO错误
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            //ThreadTool.QueueOnMainThread(_LoadConfigInThreadFinishInMain, null);
        }

        protected void _LoadConfigInThreadFinishInMain(object o)
        {

        }

        //保存图片bytes到本地
        protected void SaveTextureBytes(string url, byte[] data, int offset)
        {
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);
            string filePath = GetUniquePath(url);

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.Write(data, offset, data.Length - offset);
                fs.Close();
            }
        }

        //保存图片信息
        protected void SaveInfo(string url, int w = 0, int h = 0)
        {
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);
            TextureInfo info = GetInfo(url);
            if (null == info)
            {
                info = new TextureInfo()
                {
                    //cacheTime = System.DateTime.UtcNow.Ticks,
                    url = url,
                    hashCode = GetUniqueCode(url),
                    width = w,
                    height = h
                };
                infos.Add(info);
                File.AppendAllText(infoPath, JsonMapper.ToJson(info));
                //LogDebug.Log("TexInfo Saved:" + info.url);
            }
        }

        //获得唯一路径
        protected string GetUniquePath(string url)
        {
            string fileName = GetUniqueCode(url) + TEXTURE_EXTENSION;
            return Path.Combine(cacheDirectory, fileName);
        }

        //获得唯一特征码
        protected int GetUniqueCode(string url)
        {
            return url.GetHashCode();
        }
    }
}



    



