using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Framework;
using System.Collections.Generic;
namespace Framework
{
    public static class VRPath
    {
        static readonly string EditorPersistentPath = Application.dataPath +@"/../";
        public static string PersistDataPath
        {
            get
            {
#if UNITY_EDITOR
                return EditorPersistentPath;
#else
                return Application.persistentDataPath; 
#endif
            }
        }

        public static string StreamingAssetPath
        {
            get
            {
                return Application.streamingAssetsPath; 
            }
        }
    }

    public static class VRScene
    {
        public const string Loading = "SoccerVR/Scenes/Loading";
        public const string Splash = "SoccerVR/Scenes/SoccerVRSplash";
        public const string Welcome = "SoccerVR/Scenes/Welcome";
        public const string Football = "SoccerVR/Scenes/Football";
        public static readonly List<string> Scenes = new List<string>{
            Splash,
            Loading,
            Welcome,
            Football,
        };
    }

    public static class VRPref
    {
        public static VRSetting.PrefKey<string> Language = new VRSetting.PrefKey<string>("Language","Chinese"); 
        public static VRSetting.PrefKey<bool> FirstOpen = new VRSetting.PrefKey<bool>("FirstOpen", true);
    }

    public static class VRSortingLayer
    {
        public const string Transition = "Transition";
        public const string GlobalUI = "GlobalUI";
        public const string Log = "Log";
    }

    public static class VRLayer
    {
        public const string ColorBlock = "ColorBlock";
    }

    public static class VRHttp
    {
        public static string BaseUrl {
            get
            {
                if (VRApplication.IsDebug)
                    return "https://vrtest-api.aginomoto.com/sport/";
                else
                    return "https://vr-api.aginomoto.com/sport/";
            }
        }
    }
}
