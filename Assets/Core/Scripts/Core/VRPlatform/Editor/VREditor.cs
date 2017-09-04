using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;

namespace Framework
{
    public class VREditor
    {
        const string CardboardMenu = "VRPlatform/Cardboard";
        const string DaydreamMenu = "VRPlatform/Daydream";
        const string GearVRPMenu = "VRPlatform/GearVR";
        const string LuancherMenu = "VRPlatform/Launcher";

        static void CheckPlatformMenu()
        {
            VRPlatform curPlatform = GetPlatformPrefs();
            Menu.SetChecked(CardboardMenu, curPlatform == VRPlatform.Cardboard);
            Menu.SetChecked(DaydreamMenu, curPlatform == VRPlatform.Daydream);
            Menu.SetChecked(GearVRPMenu, curPlatform == VRPlatform.GearVR);
            Menu.SetChecked(LuancherMenu, curPlatform == VRPlatform.Launcher);
        }
        static VRPlatform GetPlatformPrefs()
        {
            return VRPlatform.Launcher;
        }
        static void SavePlatformPrefs(VRPlatform platform)
        {
            AssetDatabase.Refresh();
        }

        //[UnityEditor.InitializeOnLoadMethod]
        public static void InitEditor()
        {
            EditorApplication.playmodeStateChanged += CheckPlatformMenu;
            EditorApplication.delayCall += CheckPlatformMenu;
        }
        //[MenuItem(CardboardMenu)]
        public static void SwtichToCardboard()
        {
            SavePlatformPrefs(VRPlatform.Cardboard);
            CheckPlatformMenu();
        }
        //[MenuItem(DaydreamMenu)]
        public static void SwitchToDaydream()
        {
            SavePlatformPrefs(VRPlatform.Daydream);
            CheckPlatformMenu();
        }
        //[MenuItem(GearVRPMenu)]
        public static void SwitchToGearVR()
        {
            SavePlatformPrefs(VRPlatform.GearVR);
            CheckPlatformMenu();
        }
        //[MenuItem(LuancherMenu)]
        public static void SwitchToLuancher()
        {
            SavePlatformPrefs(VRPlatform.Launcher);
            CheckPlatformMenu();
        }
    }

    public class VRFinder
    {
        public static GameObject FindPrefab
            (string prefabName, string type)
        {
            string[] assetGUIDs = AssetDatabase.FindAssets(string.Format("{0} t:Prefab", prefabName));
            for (int i = 0; i < assetGUIDs.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                GameObject o = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (o != null && o.GetComponent(type) != null)
                {
                    return o;
                }
            }
            return null;
        }
    }

}
