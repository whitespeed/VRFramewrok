using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{

    public static class AppPath
    {
        static readonly string EditorPersistentPath = Application.dataPath + @"/../";
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
}

