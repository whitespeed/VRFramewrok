using System;
using Framework.LitJson;
using UnityEngine;

namespace Framework
{
    public class VRApplication
    {
        public const string AppName = "SoccerVR";

        public static Action ChangeFirstScene = null;

        public static VRPlatform VRPlatform { get; set; }

        public static string Version { get; protected set; }
        public static string Revision { get; protected set; }
        public static string BuildTime { get; protected set; }
        public static bool IsDebug { get; set; }
        public static bool IsOffline { get { return false; } }

        public static void Start()
        {
            Initialize();
            CreateSingleton();
            GotoFirstScene();
        }

        public static void BackToLauncher()
        {
            Debug.LogWarning("[VRApplication] BackToLauncher");
            VRManager.Viewer.BackToLauncher();
        }

        public static void ForceQuit()
        {
            Debug.LogWarning("[VRApplication] ForceQuit");
            VRManager.Viewer.ForceQuit();
        }

        public static void CreateSingleton()
        {
            VRDebug.Create();
            VRManager.Create();
            SceneManager.Create();
            PluginManager.Create();
            TextureManager.Create();
            TimerExecutor.Create();
            GlobalUIManager.Create();
            HttpRequester.Create();
            NetworkManager.Create();
            if(IsDebug)
            {
                VRLogUI.Create();
            }
        }

        public static void Destroysingleton()
        {
            VRDebug.Destroy();
            SceneManager.Destroy();
            PluginManager.Destroy();
            VRManager.Destroy();
            TimerExecutor.Destroy();
            GlobalUIManager.Destroy();
            TextureManager.Destroy();
            HttpRequester.Destroy();
            NetworkManager.Destroy();
            if(IsDebug)
            {
                VRLogUI.Destroy();
            }
        }

        public static void GotoFirstScene()
        {
            VRManager.Viewer.Recent();
            if(ChangeFirstScene != null)
            {
                ChangeFirstScene();
            }
            else
            {
                
            }
        }

        protected static void Initialize()
        {
            var textAsset = VRRes.Load<TextAsset>(AppName);
            if(textAsset)
            {
                var json = JsonMapper.ToObject(textAsset.text);
                Version = json["version"].ToString();
                Revision = json["revision"].ToString();
                BuildTime = json["build"].ToString();
            }
            else
            {
                Version = Application.version;
                Revision = "unknown";
                BuildTime = "unknown";
            }
            //初始化语言
            Localization.Language = Localization.PlayerPref;
        }
    }
}