using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Framework
{
    public class EditorPlugin : IPlugin
    {

        public void Init()
        {
            Debug.LogWarning("Editor Plugin Init");
        }

        public void BackToLauncher()
        {
            Debug.LogWarning("Editor Plugin BackToLauncher");
            
        }

        public void Destroy()
        {
            Debug.LogWarning("Editor Plugin Destroy");
        }

        public void StartPersistenConnection()
        {
            Debug.LogWarning("Editor Plugin StartPersistenConnection");
        }

        public void StopPersistenConnection()
        {
            Debug.LogWarning("Editor Plugin StopPersistenConnection");
        }

        public bool GetNetworkState(ref int state)
        {
            Debug.LogWarning("Editor Plugin GetNetworkState");
            return false;
        }

        public void ApplicationQuit()
        {
            Debug.LogWarning("Editor Plugin ApplicationQuit");
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = a.GetType("UnityEditor.EditorApplication");
                if (t != null)
                {
                    t.GetProperty("isPlaying").SetValue(null, false, null);
                    break;
                }
            }
        }

        public void SetScreenBrightness(int luminance)
        {
           PlayerPrefs.SetInt("luminance",luminance);
        }

        public bool GetBrightnessPercent(ref int level)
        {
            level = PlayerPrefs.GetInt("luminance", 0);
            return true;
        }

        public void SetCurrentVolume(int level)
        {
            PlayerPrefs.SetInt("volume", level);
            AudioListener.volume = level/10f;
        }

        public bool GetCurrentVolumePercent(ref int level)
        {
            level = PlayerPrefs.GetInt("volume", 10);
            return true;
        }
    }

}

