using System;
using System.CodeDom.Compiler;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Framework
{
    public class AssetUtility
    {
        public const string AssetBundlesOutputPath = "AssetBundles";

        private static readonly string ResourcesStr = "Resources";
        private static readonly string AssetsStr = "Assets";

        public static string GetPlatformName()
        {
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);

        }

        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch(target)
            {
                case BuildTarget.Android:
                    return "Android";
#if UNITY_TVOS
                case BuildTarget.tvOS:
                    return "tvOS";
#endif
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }


        public static string GetAssetBundleNameAtPath(string assetPath)
        {
            var import = AssetImporter.GetAtPath(assetPath);
            if (null != import)
            {
                return import.assetBundleName;
            }
            return String.Empty;
        }


        public static string GetAssetBundleVariantAtPath(string assetPath)
        {
            var import = AssetImporter.GetAtPath(assetPath);
            if (null != import)
            {
                return import.assetBundleVariant;
            }
            return String.Empty;
        }

        public static bool IsInResourcesFolder(string fullPath)
        {
            return fullPath.Contains(@"/Resources/");
        }

        public static bool IsInAssestFolder(string fullPath)
        {
            return fullPath.StartsWith(Application.dataPath);
        }

        public static string GetAssetNameFromPath(string path)
        {
            var key = Path.GetFileNameWithoutExtension(path.Trim()).Replace(" ", "_");
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Resource Name is INVALID: " + path);
            }
            var provider = CodeDomProvider.CreateProvider("C#");
            if (!provider.IsValidIdentifier(key))
            {
                throw new ArgumentException("Resource Name is INVALID: " + key);
            }
            return key;
        }


        public static string RelativeResourcesPath(string fullPath, bool keepExtension = false)
        {
            int resIdx = fullPath.Trim().LastIndexOf(AssetUtility.ResourcesStr, StringComparison.Ordinal) + 1;
            if (keepExtension)
            {
                return fullPath.Substring(resIdx + AssetUtility.ResourcesStr.Length);
            }
            else
            {
                int extId = fullPath.LastIndexOf(".", StringComparison.Ordinal);
                return fullPath.Substring(resIdx + AssetUtility.ResourcesStr.Length, extId - resIdx - AssetUtility.ResourcesStr.Length);
            }
        }

        public static string GetFullPathFromAssetsPath(string assetPath)
        {
            return Path.GetFullPath(assetPath);
        }

        public static string RelativeAssetsPath(string fullPath, bool keepExtension = true)
        {
            int idx = fullPath.Trim().IndexOf(AssetUtility.AssetsStr, StringComparison.Ordinal);
            if (keepExtension)
            {
                return fullPath.Substring(idx);
            }
            else
            {
                int extId = fullPath.LastIndexOf(".", StringComparison.Ordinal);
                return fullPath.Substring(idx + AssetUtility.ResourcesStr.Length, extId - idx - AssetUtility.ResourcesStr.Length);
            }
        }

        public static T LoadAssetAtFullPath<T>(string fullPath) where T:UnityEngine.Object
        {
            string assetPath = RelativeAssetsPath(fullPath);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
    }
}