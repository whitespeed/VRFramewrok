using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Framework
{
    /// <summary>
    /// Loaded assetBundle contains the references count which can be used to
    /// unload dependent assetBundles automatically.
    /// </summary>
    public class AssetBundleInfo
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        public AssetBundleInfo(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }

        public AssetBundleInfo()
        {
            m_ReferencedCount = 1;
        }

        internal event Action unload;

        internal void OnUnload()
        {
            if(m_AssetBundle != null)
            {
                m_AssetBundle.Unload(true);
                m_AssetBundle = null;
            }
            if(unload != null)
            {
                unload();
            }
        }
    }

    /// <summary>
    /// Class takes care of loading assetBundle and its dependencies
    /// automatically, loading variants automatically.
    /// </summary>
    public class AssetBundleManager : MonoBehaviour
    {
        public delegate string OverrideBaseDownloadingURLDelegate(string bundleName);

        private static string m_BaseDownloadingURL = "";

        private static string[] m_ActiveVariants = {};

        private static AssetBundleManifest m_AssetBundleManifest;

        private static readonly Dictionary<string, AssetBundleInfo> m_AssetBundleInfos = new Dictionary<string, AssetBundleInfo>();

        private static readonly Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();

        private static readonly Dictionary<string, AssetBundleOperation> m_InProgressOperations = new Dictionary<string, AssetBundleOperation>();

        private static readonly Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

        private readonly List<string> finishedOperation = new List<string>();

        /// <summary>
        /// Variants which is used to define the active variants.
        /// </summary>
        public static string[] ActiveVariants { get { return m_ActiveVariants; } set { m_ActiveVariants = value; } }

        public static bool HasAssetBundleManifest { get { return m_AssetBundleManifest != null; } }

        /// <summary>
        /// Implements per-bundle base downloading URL override.
        /// The subscribers must return null values for unknown bundle names;
        /// </summary>
        public static event OverrideBaseDownloadingURLDelegate overrideBaseDownloadingURL;

        /// <summary>
        /// Sets base downloading URL to a directory relative to the streaming assets directory.
        /// Asset bundles are loaded from a local directory.
        /// </summary>
        public static void SetSourceAssetBundleDirectory(string relativePath)
        {
            m_BaseDownloadingURL = GetStreamingAssetsPath() + relativePath;
        }

        /// <summary>
        /// Sets base downloading URL to a web URL. The directory pointed to by this URL
        /// on the web-server should have the same structure as the AssetBundles directory
        /// in the demo project root.
        /// </summary>
        /// <example>For example, AssetBundles/iOS/xyz-scene must map to
        /// absolutePath/iOS/xyz-scene.
        /// <example>
        public static void SetSourceAssetBundleURL(string absolutePath)
        {
            if(!absolutePath.EndsWith("/"))
            {
                absolutePath += "/";
            }
            m_BaseDownloadingURL = absolutePath + GetPlatformName() + "/";
        }

        /// <summary>
        /// Retrieves an asset bundle that has previously been requested via LoadAssetBundle.
        /// Returns null if the asset bundle or one of its dependencies have not been downloaded yet.
        /// </summary>
        public static AssetBundle GetLoadedAssetBundle(string assetBundleName)
        {
            string error;
            if(m_DownloadingErrors.TryGetValue(assetBundleName, out error))
            {
                return null;
            }
            AssetBundleInfo info = null;
            m_AssetBundleInfos.TryGetValue(assetBundleName, out info);
            if(info == null || info.m_AssetBundle == null)
            {
                return null;
            }

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if(!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
            {
                return info.m_AssetBundle;
            }

            // Make sure all dependencies are loaded
            foreach(var dependency in dependencies)
            {
                if(m_DownloadingErrors.TryGetValue(dependency, out error))
                {
                    return null;
                }

                // Wait all the dependent assetBundles being loaded.
                AssetBundleInfo dependentBundle;
                m_AssetBundleInfos.TryGetValue(dependency, out dependentBundle);
                if(dependentBundle == null || dependentBundle.m_AssetBundle == null)
                {
                    return null;
                }
            }
            return info.m_AssetBundle;
        }

        public static Hash128 GetAssetBundleHash(string bundleName)
        {
            if(m_AssetBundleManifest == null)
            {
                Log(AssetSetting.LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return new Hash128();
            }
            return m_AssetBundleManifest.GetAssetBundleHash(bundleName);
        }

        public static IEnumerator Initialize()
        {
            yield return Initialize(GetPlatformName());
        }

        public static string GetPlatformName()
        {
            switch(Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXDashboardPlayer:
                case RuntimePlatform.OSXEditor:
                    return "OSX";
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                default:
                    return null;
            }
        }

        public static IEnumerator Initialize(string manifestAssetBundleName)
        {
            var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
            DontDestroyOnLoad(go);
            Log(AssetSetting.LogType.Info, "Simulation Mode: " + (AssetSetting.SimulateAssetBundleInEditor ? "Enabled" : "Disabled"));

            if(AssetSetting.SimulateAssetBundleInEditor)
            {
                yield break;
            }
            SetSourceAssetBundleURL(AssetSetting.AssetBundleDownloadUrl);
            var operation = new AssetBundleManifestOperation(manifestAssetBundleName);
            m_InProgressOperations.Add(manifestAssetBundleName, operation);
            Log(AssetSetting.LogType.Info, "Loading AssetBundleManifest");
            yield return operation;
        }

        public static string GetAssetBundleURL(string bundleName)
        {
            if(overrideBaseDownloadingURL != null)
            {
                foreach(var del in overrideBaseDownloadingURL.GetInvocationList())
                {
                    var method = (OverrideBaseDownloadingURLDelegate) del;
                    var res = method(bundleName);
                    if(res != null)
                    {
                        return res;
                    }
                }
            }
            if(!m_BaseDownloadingURL.EndsWith("/"))
            {
                m_BaseDownloadingURL += "/";
            }
            return m_BaseDownloadingURL + bundleName;
        }

        public static void UnloadAssetBundle(string assetBundleName)
        {
            if(AssetSetting.SimulateAssetBundleInEditor)
            {
                return;
            }

            assetBundleName = RemapVariantName(assetBundleName);
            UnloadAssetBundleInternal(assetBundleName);
            UnloadDependencies(assetBundleName);
        }

        // Temporarily work around a il2cpp bug
        public static IEnumerator LoadAssetBundle(string assetBundleName)
        {
          
            if(AssetSetting.SimulateAssetBundleInEditor)
            {
                yield break;
            }
            if(m_AssetBundleManifest == null)
            {
                Log(AssetSetting.LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                yield break;
            }
            // Check if the assetBundle has already been processed.
            var isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName);

            // Load dependencies.
            if(!isAlreadyProcessed)
            {
                LoadDependencies(assetBundleName);
            }
            while(null == GetLoadedAssetBundle(assetBundleName))
            {
                yield return null;
            }
        }

        public static void ProcessFinishedOperation(AssetBundleOperation operation)
        {
            if(operation == null)
            {
                return;
            }
            if(string.IsNullOrEmpty(operation.error))
            {
                if(m_AssetBundleInfos.ContainsKey(operation.assetBundleName))
                {
                    m_AssetBundleInfos[operation.assetBundleName].m_AssetBundle = operation.assetBundle;
                }
                else
                {
                    m_AssetBundleManifest = operation.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }
            }
            else
            {
                var msg = string.Format("Failed downloading bundle {0} from {1}: {2}", operation.assetBundleName, operation.url, operation.error);
                m_DownloadingErrors.Add(operation.assetBundleName, msg);
            }
        }

        protected static bool UsesExternalBundleVariantResolutionMechanism(string baseAssetBundleName)
        {
#if ENABLE_IOS_APP_SLICING
            var url = GetAssetBundleURL(baseAssetBundleName);
            if (url.ToLower().StartsWith("res://") ||
                url.ToLower().StartsWith("odr://"))
                return true;
#endif
            return false;
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        protected static string RemapVariantName(string assetBundleName)
        {
            var bundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();

            // Get base bundle name
            var baseName = assetBundleName.Split('.')[0];
            if(UsesExternalBundleVariantResolutionMechanism(baseName))
            {
                return baseName;
            }
            var bestFit = int.MaxValue;
            var bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for(var i = 0; i < bundlesWithVariant.Length; i++)
            {
                var curSplit = bundlesWithVariant[i].Split('.');
                var curBaseName = curSplit[0];
                var curVariant = curSplit[1];
                if(curBaseName != baseName)
                {
                    continue;
                }
                var found = Array.IndexOf(m_ActiveVariants, curVariant);

                // If there is no active variant found. We still want to use the first
                if(found == -1)
                {
                    found = int.MaxValue - 1;
                }
                if(found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }
            if(bestFit == int.MaxValue - 1)
            {
                Log(AssetSetting.LogType.Warning, "Ambigious asset bundle variant chosen because there was no matching active variant: " + bundlesWithVariant[bestFitIndex]);
            }
            if(bestFitIndex != -1)
            {
                return bundlesWithVariant[bestFitIndex];
            }
            return assetBundleName;
        }

        protected static bool LoadAssetBundleInternal(string assetBundleName)
        {
            Log(AssetSetting.LogType.Info, "Loading AssetBundle: " + assetBundleName);
            AssetBundleInfo bundle = null;
            m_AssetBundleInfos.TryGetValue(assetBundleName, out bundle);
            if(bundle != null)
            {
                bundle.m_ReferencedCount++;
                return true;
            }
            m_AssetBundleInfos.Add(assetBundleName, new AssetBundleInfo());
            m_InProgressOperations.Add(assetBundleName, new AssetBundleWWWOperation(assetBundleName));
            return false;
        }

        protected static void LoadDependencies(string assetBundleName)
        {
            // Get dependecies from the AssetBundleManifest object..
            var dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            if(dependencies.Length == 0)
            {
                return;
            }
            for(var i = 0; i < dependencies.Length; i++)
            {
                dependencies[i] = RemapVariantName(dependencies[i]);
            }

            // Record and load all dependencies.
            m_Dependencies.Add(assetBundleName, dependencies);
            for(var i = 0; i < dependencies.Length; i++)
            {
                LoadAssetBundleInternal(dependencies[i]);
            }
        }

        protected static void UnloadDependencies(string assetBundleName)
        {
            string[] dependencies = null;
            if(!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
            {
                return;
            }

            // Loop dependencies.
            foreach(var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency);
            }
            m_Dependencies.Remove(assetBundleName);
        }

        protected static void UnloadAssetBundleInternal(string assetBundleName)
        {
            string error;
            AssetBundleInfo bundleInfo = null;
            m_AssetBundleInfos.TryGetValue(assetBundleName, out bundleInfo);
            if(bundleInfo == null)
            {
                return;
            }
            if(--bundleInfo.m_ReferencedCount == 0)
            {
                bundleInfo.OnUnload();
                m_AssetBundleInfos.Remove(assetBundleName);
                Log(AssetSetting.LogType.Info, assetBundleName + " has been unloaded successfully");
            }
        }

        private static void Log(AssetSetting.LogType logType, string text)
        {
            if(logType == AssetSetting.LogType.Error)
            {
                Debug.LogError("[AssetBundleManager] " + text);
            }
            else if(AssetSetting.logMode == AssetSetting.LogMode.All && logType == AssetSetting.LogType.Warning)
            {
                Debug.LogWarning("[AssetBundleManager] " + text);
            }
            else if(AssetSetting.logMode == AssetSetting.LogMode.All)
            {
                Debug.Log("[AssetBundleManager] " + text);
            }
        }

        private static string GetStreamingAssetsPath()
        {
            if(Application.isEditor)
            {
                return "file://" + Environment.CurrentDirectory.Replace("\\", "/");
                // Use the build output folder directly.
            }
            if(Application.isWebPlayer)
            {
                return Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
            }
            if(Application.isMobilePlatform || Application.isConsolePlatform)
            {
                return Application.streamingAssetsPath;
            }
            return "file://" + Application.streamingAssetsPath;
        }

        private void Update()
        {
            foreach(var operation in m_InProgressOperations)
            {
                if(!operation.Value.Update())
                {
                    finishedOperation.Add(operation.Key);
                }
            }
            for(var i = 0; i < finishedOperation.Count; i++)
            {
                m_InProgressOperations.Remove(finishedOperation[i]);
            }
            finishedOperation.Clear();
        }
    } // End of AssetBundleManager.
}