using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    public class BuildScript
    {
        public static string overloadedDevelopmentServerURL = "";
        public static string assetBundleManagerResourcesDirectory = "Assets/AutoGen/Resources";
        public static string assetBundleUrlPath = Path.Combine(assetBundleManagerResourcesDirectory, "AssetBundleServerURL.bytes");
        public static string CreateAssetBundleDirectory()
        {
            // Choose the output path according to the build target.
            var outputPath = Path.Combine(AssetUtility.AssetBundlesOutputPath, AssetUtility.GetPlatformName());
            if(!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            return outputPath;
        }

        public static void BuildAssetBundles(BuildTarget target)
        {
            AutoGenR.SearchAllAndGen();
            // Choose the output path according to the build target.
            var outputPath = CreateAssetBundleDirectory();
            var options = BuildAssetBundleOptions.None;
            var shouldCheckODR = target == BuildTarget.iOS;
#if UNITY_TVOS
            shouldCheckODR |= target == BuildTarget.tvOS;
#endif
            if (shouldCheckODR)
            {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                if (PlayerSettings.iOS.useOnDemandResources)
                    options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
#if ENABLE_IOS_APP_SLICING
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
            }
            //@TODO: use append hash... (Make sure pipeline works correctly with it.)
            //options |= BuildAssetBundleOptions.DryRunBuild;
            options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            //options |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
            options |= BuildAssetBundleOptions.StrictMode;
            BuildPipeline.BuildAssetBundles(outputPath, options, target);
        }

        public static void WriteServerURL()
        {
            string downloadURL;
            if(string.IsNullOrEmpty(overloadedDevelopmentServerURL) == false)
            {
                downloadURL = overloadedDevelopmentServerURL;
            }
            else
            {
                IPHostEntry host;
                var localIP = "";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach(var ip in host.AddressList)
                {
                    if(ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
                downloadURL = "http://" + localIP + ":7888/";
            }

            Directory.CreateDirectory(assetBundleManagerResourcesDirectory);
            File.WriteAllText(assetBundleUrlPath, downloadURL);
            AssetDatabase.Refresh();
        }

        public static void BuildPlayer()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if(outputPath.Length == 0)
            {
                return;
            }
            var levels = GetLevelsFromBuildSettings();
            if(levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }
            var targetName = GetDefaultBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if(targetName == null)
            {
                return;
            }

            //TODO: Build and copy AssetBundles.
            //BuildAssetBundles();
            WriteServerURL();
#if UNITY_5_4 || UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
            BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
#else
            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = levels;
            buildPlayerOptions.locationPathName = outputPath + targetName;
            buildPlayerOptions.assetBundleManifestPath = GetAssetBundleManifestFilePath();
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = EditorUserBuildSettings.development
                ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(buildPlayerOptions);
#endif
        }

        public static void BuildStandalonePlayer()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if(outputPath.Length == 0)
            {
                return;
            }
            var levels = GetLevelsFromBuildSettings();
            if(levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }
            var targetName = GetDefaultBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if(targetName == null)
            {
                return;
            }

            //TODO: Build and copy AssetBundles.
            //BuildAssetBundles();
            CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, AssetUtility.AssetBundlesOutputPath));
            AssetDatabase.Refresh();
#if UNITY_5_4 || UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
            BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
#else
            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = levels;
            buildPlayerOptions.locationPathName = outputPath + targetName;
            buildPlayerOptions.assetBundleManifestPath = GetAssetBundleManifestFilePath();
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = EditorUserBuildSettings.development
                ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(buildPlayerOptions);
#endif
        }

        private static string GetDefaultBuildTargetName(BuildTarget target)
        {
            switch(target)
            {
                case BuildTarget.Android:
                    return "/test.apk";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "/test.exe";
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "/test.app";
                case BuildTarget.WebGL:
                case BuildTarget.iOS:
                    return "";
                // Add more build targets for your own.
                default:
                    Debug.Log("Target not implemented.");
                    return null;
            }
        }

        private static void CopyAssetBundlesTo(string outputPath)
        {
            // Clear streaming assets folder.
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            Directory.CreateDirectory(outputPath);
            var outputFolder = AssetUtility.GetPlatformName();

            // Setup the source folder for assetbundles.
            var source = Path.Combine(Path.Combine(Environment.CurrentDirectory, AssetUtility.AssetBundlesOutputPath),
                outputFolder);
            if(!Directory.Exists(source))
            {
                Debug.Log("No assetBundle output folder, try to build the assetBundles first.");
            }

            // Setup the destination folder for assetbundles.
            var destination = Path.Combine(outputPath, outputFolder);
            if(Directory.Exists(destination))
            {
                FileUtil.DeleteFileOrDirectory(destination);
            }
            FileUtil.CopyFileOrDirectory(source, destination);
        }

        private static string[] GetLevelsFromBuildSettings()
        {
            var levels = new List<string>();
            for(var i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                if(EditorBuildSettings.scenes[i].enabled)
                {
                    levels.Add(EditorBuildSettings.scenes[i].path);
                }
            }
            return levels.ToArray();
        }

        private static string GetAssetBundleManifestFilePath()
        {
            var relativeAssetBundlesOutputPathForPlatform = Path.Combine(AssetUtility.AssetBundlesOutputPath,
                AssetUtility.GetPlatformName());
            return Path.Combine(relativeAssetBundlesOutputPathForPlatform, AssetUtility.GetPlatformName()) + ".manifest";
        }
    }
}