using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class AssetManager
    {
        protected static AssetManifest manifest = null;

        public static IEnumerator Initialize()
        {
            yield return AssetBundleManager.LoadAssetBundle(AssetManifest.BundleName);
            var ab = AssetBundleManager.GetLoadedAssetBundle(AssetManifest.BundleName);
            if(ab != null)
            {
                manifest = ab.LoadAsset<AssetManifest>(AssetManifest.AssetName);
            }
            yield return null;
        }

        public static T Load<T>(string path) where T : Object
        {
            if(typeof(T) == typeof(Shader))
            {
                return Shader.Find(path) as T;
            }
            if(AssetSetting.SimulateAssetBundleInEditor)
            {
                return Resources.Load<T>(path);
            }
            else
            {
                string bundleName;
                if (manifest && manifest.TryGetAssetBundleName(path, out bundleName))
                {
                    var ab = AssetBundleManager.GetLoadedAssetBundle(bundleName);
                    var assetPath = manifest.GetAssetPath(path);
                    return ab.LoadAsset<T>(assetPath);
                }
                return Resources.Load<T>(path);
            }

        }
        public static void Unload(Object obj)
        {
            if (AssetSetting.SimulateAssetBundleInEditor)
            {
                Resources.UnloadAsset(obj);
            }
            else
            {

            }
        }
    }
}

