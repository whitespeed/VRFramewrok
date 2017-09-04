using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Framework;
using System.Collections.Generic;
namespace Framework
{


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
        public static AppSetting.PrefKey<string> Language = new AppSetting.PrefKey<string>("Language","Chinese"); 
        public static AppSetting.PrefKey<bool> FirstOpen = new AppSetting.PrefKey<bool>("FirstOpen", true);
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
}
