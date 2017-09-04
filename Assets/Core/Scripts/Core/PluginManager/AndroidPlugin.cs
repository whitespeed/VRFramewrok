using UnityEngine;
using System.Collections;
using System;

namespace Framework
{
    public class AndroidPlugin : IPlugin
    {
        protected const int ToolCommon = 0;
        protected const int ToolPersistCon = 1;

        private static AndroidJavaObject androidActivity;
        private static AndroidJavaObject commonTool;
        private static AndroidJavaObject persistConTool;
        private static AndroidJavaObject networkTool;
        public void Init()
        {
            try
            {
                using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    androidActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
                }
                using (AndroidJavaObject context = androidActivity.Call<AndroidJavaObject>("getBaseContext"))
                {
                    commonTool = OnCreateObject("com.whaleyvr.tools.CommonTool", context);
                    persistConTool = OnCreateObject("com.whaleyvr.tools.PresistConnectionTool", context);
                    networkTool = OnCreateObject("com.whaleyvr.tools.NetworkTool", context);
                }

            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception while connecting to the Activity: " + e);
            }
        }

        protected AndroidJavaObject OnCreateObject(string className, AndroidJavaObject context)
        {
            var obj = new AndroidJavaObject(className);
            obj.Call("OnCreate", context);
            return obj;
        }

        public void BackToLauncher()
        {
            CallObjectMethod(commonTool, "BackToLauncher");
        }

        public void ApplicationQuit()
        {
            CallObjectMethod(commonTool, "ApplicationQuit");
        }

        public bool GetNetworkState(ref int state)
        {
            return CallObjectMethod<int>(ref state, networkTool, "GetNetworkState");
        }

        public void Destroy()
        {
            DisposeJavaObj(ref commonTool);
            DisposeJavaObj(ref persistConTool);
            DisposeJavaObj(ref networkTool);
        }

        protected static void DisposeJavaObj(ref AndroidJavaObject Obj)
        {
            if (Obj != null)
            {
                Obj.Call("OnDestory");
                Obj.Dispose();
                Obj = null;
            }
        }

        public void StartPersistenConnection()
        {
            CallStaticMethod(persistConTool, "DoLoginUser");
        }

        public void StopPersistenConnection()
        {
            CallStaticMethod(persistConTool, "DoLogoutUser");
        }

        public static AndroidJavaClass GetClass(string className)
        {
            try
            {
                return new AndroidJavaClass(className);
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception getting class " + className + ": " + e);
                return null;
            }
        }

        public static bool CallStaticMethod(AndroidJavaObject jo, string name, params object[] args)
        {
            if (jo == null)
            {
                Debug.LogError("Object is null when calling static method " + name);
                return false;
            }
            try
            {
                jo.CallStatic(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling static method " + name + ": " + e);
                return false;
            }
        }

        public static bool CallObjectMethod(AndroidJavaObject jo, string name, params object[] args)
        {
            if (jo == null)
            {
                Debug.LogError("Object is null when calling method " + name);
                return false;
            }
            try
            {
                jo.Call(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling method " + name + ": " + e);
                return false;
            }
        }

        public static bool CallStaticMethod<T>(ref T result, AndroidJavaObject jo, string name,
                                                  params object[] args)
        {
            if (jo == null)
            {
                Debug.LogError("Object is null when calling static method " + name);
                return false;
            }
            try
            {
                result = jo.CallStatic<T>(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling static method " + name + ": " + e);
                return false;
            }
        }

        public static bool CallObjectMethod<T>(ref T result, AndroidJavaObject jo, string name,
                                                  params object[] args)
        {
            if (jo == null)
            {
                Debug.LogError("Object is null when calling method " + name);
                return false;
            }
            try
            {
                result = jo.Call<T>(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling method " + name + ": " + e);
                return false;
            }
        }

        public void SetScreenBrightness(int Brightness)
        {
            CallObjectMethod(commonTool, "SetBrightness", Brightness);
        }

        public bool GetCurrentVolumePercent(ref int percent)
        {
            return CallObjectMethod<int>(ref percent, commonTool, "GetCurrentVolumePercent");
        }

        public void SetCurrentVolume(int percent)
        {
            CallObjectMethod(commonTool, "SetCurrentVolume", percent);
        }

        public bool GetBrightnessPercent(ref int percent)
        {
            return CallObjectMethod<int>(ref percent, commonTool, "GetBrightnessPercent");
        }

       
    }


}
