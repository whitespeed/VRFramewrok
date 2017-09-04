using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public enum AssetType
    {
        None,
        Texture2D,
        TextAsset,
        Prefab,
        AudioClip,
        Shader,
    }

    [System.Serializable]
    public struct AssetInfo
    {
        public AssetType AssetType;
        public string AssetPath;
        public string ResourcePath;
        public string AssetBundleName;

        public static AssetInfo Create(AssetType type, string resPath, string abName, string assetPath)
        {
            AssetInfo info = new AssetInfo();
            info.AssetType = type;
            info.ResourcePath = resPath;
            info.AssetBundleName = abName;
            info.AssetPath = assetPath;
            return info;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\", \"{1}\", \"{2}\", \"{3}\"", AssetType, AssetPath, ResourcePath, AssetBundleName);
        }
    }

    public class AssetManifest : ScriptableObject
    {
        public static string BundleName = "r";
        public static string AssetPath =string.Format("Assets/{0}/AutoGen/Resources/",VRApplication.AppName);
        public static string AssetName = "R.asset";
        [SerializeField]
        protected List<string> keys = new List<string>();
        [SerializeField]
        protected List<AssetInfo> values = new List<AssetInfo>();

        public void AddRange(List<AssetInfo> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (!keys.Contains(data[i].ResourcePath))
                {
                    keys.Add(data[i].ResourcePath);
                    values.Add(data[i]);
                }
            }
        }


        public bool Contain(string resPath)
        {
            return keys.Contains(resPath);
        }

        public bool TryGetAssetBundleName(string resPath,out string bundleName)
        {
            int idx = keys.IndexOf(resPath);
            if(idx >= 0)
            {
                bundleName = values[idx].AssetBundleName;
                return true;
            }
            else
            {
                bundleName = string.Empty;
                return false;
            }
           
        }
        public string GetAssetBundleName(string resPath)
        {
            int idx = keys.IndexOf(resPath);
            if (idx >= 0)
                return values[idx].AssetBundleName;
            return string.Empty;
        }

        public string GetAssetPath(string resPath)
        {
            int idx = keys.IndexOf(resPath);
            if (idx >= 0)
                return values[idx].AssetPath;
            return string.Empty;
        }
        public AssetType GetAssetType(string resPath)
        {
            int idx = keys.IndexOf(resPath);
            if (idx >= 0)
                return values[idx].AssetType;
            return AssetType.None;
        }


        protected AssetManifest()
        {

        }

    }

}

