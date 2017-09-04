using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Framework
{
    internal class LaunchAssetBundleServer : ScriptableSingleton<LaunchAssetBundleServer>
    {
        private const string ProcessName = "AssetBundleServer";
        private const string kLocalAssetbundleServerMenu = "AssetBundle/Local AssetBundle Server";
        private const string kLocalAssetbundleServerPath = "Assets/SoccerVR/Scripts/Core/ResourceManager/Editor/AssetBundleServer.exe";

        [MenuItem(kLocalAssetbundleServerMenu)]
        public static void ToggleLocalAssetBundleServer()
        {
            var isRunning = IsRunning();
            if(!isRunning)
            {
                Run();
            }
            else
            {
                KillRunningAssetBundleServer();
            }
        }

        [MenuItem(kLocalAssetbundleServerMenu, true)]
        public static bool ToggleLocalAssetBundleServerValidate()
        {
            var isRunnning = IsRunning();
            Menu.SetChecked(kLocalAssetbundleServerMenu, isRunnning);
            return true;
        }

        private static bool IsRunning()
        {
            var process = Process.GetProcessesByName(ProcessName);
            if (process.Length == 0)
            {
                return false;
            }
            return process.Any((p) => !p.HasExited);
        }

        private static void KillRunningAssetBundleServer()
        {
            // Kill the last time we ran
            try
            {
                var lastProcesses =  Process.GetProcessesByName(ProcessName);
                for(int i = 0; i < lastProcesses.Length; i++)
                {
                    lastProcesses[i].Kill();
                    Debug.Log("AssetBundleServer process stopped.");
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void Run()
        {
            var files = Directory.GetFiles(Application.dataPath, "AssetBundleServer.exe", SearchOption.AllDirectories);
            if(files.Length <= 0)
            {
                Debug.LogError("Can not find AssetBundleServer.exe in project.");
                return;
            }
            var pathToAssetServer = Path.GetFullPath(files[0]);
            var assetBundlesDirectory = Path.Combine(Environment.CurrentDirectory, "AssetBundles");
            KillRunningAssetBundleServer();
            BuildScript.CreateAssetBundleDirectory();
            BuildScript.WriteServerURL();
            var args = assetBundlesDirectory;
            args = string.Format("\"{0}\" {1}", args, Process.GetCurrentProcess().Id);
            var startInfo = new ProcessStartInfo("cmd.exe", "/c " + pathToAssetServer + " " + args)
            {
                UseShellExecute = true
            };
            var launchProcess = Process.Start(startInfo);
            
            if(launchProcess == null || launchProcess.HasExited || launchProcess.Id == 0)
            {
                Debug.LogError("Unable Start AssetBundleServer process.");
            }
            else
            {
                Debug.Log("AssetBundleServer process started.");
            }
        }
    }
}