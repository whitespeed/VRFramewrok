using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Framework
{
    public class AssetSetting 
    {
        public enum LogMode
        {
            All,
            JustErrors
        }

        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        private static LogMode m_LogMode = LogMode.All;

        public static bool SimulateAssetBundleInEditor
        {
#if UNITY_EDITOR
            get
            {
                return EditorPrefs.GetBool("SimulateAssetBundles", true);
            }
            set
            {
                EditorPrefs.SetBool("SimulateAssetBundles", value);
            }
#else
            get { return false;  }
            set { }
#endif
        }

        public static string AssetBundleDownloadUrl
        {
            get
            {
                if(Application.isEditor)
                {
                    var urlFile = Resources.Load("AssetBundleServerURL") as TextAsset;
                    var url = urlFile != null ? urlFile.text.Trim() : null;
                    return url;
                }
                
                else
                {
                    //TODO: used in real runtime.
                    return "http://172.0.0.1/";
                }
            }
        }
        public static AssetSetting.LogMode logMode { get { return AssetSetting.m_LogMode; } set { AssetSetting.m_LogMode = value; } }
    }
}

