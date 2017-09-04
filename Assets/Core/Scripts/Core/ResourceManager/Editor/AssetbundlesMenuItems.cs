using UnityEditor;

namespace Framework
{
    public class AssetBundlesMenuItems
    {
        public const string MenuItemSimulationMode ="AssetBundle/Simulation Mode";
        public const string MenuItemBuildAssetBundles = "AssetBundle/Build/AssetBundles";
        public const string MenuItemBuildPlayer = "AssetBundle/Build/Player(for use with engine code stripping)";

        [MenuItem(MenuItemSimulationMode)]
        public static void ToggleSimulationMode()
        {
            AssetSetting.SimulateAssetBundleInEditor = !AssetSetting.SimulateAssetBundleInEditor;
        }

        [MenuItem(MenuItemSimulationMode, true)]
        public static bool ToggleSimulationModeValidate()
        {
            Menu.SetChecked(MenuItemSimulationMode, AssetSetting.SimulateAssetBundleInEditor);
            return true;
        }

        [MenuItem(MenuItemBuildAssetBundles + "/Android")]
        public static void BuildAssetBundlesAndroid()
        {
            BuildScript.BuildAssetBundles(BuildTarget.Android);
        }

        [MenuItem(MenuItemBuildAssetBundles + "/Win64")]
        public static void BuildAssetBundlesWin64()
        {
            BuildScript.BuildAssetBundles(BuildTarget.StandaloneWindows64);
        }

        [MenuItem(MenuItemBuildAssetBundles + "/iOS")]
        public static void BuildAssetBundlesiOS()
        {
            BuildScript.BuildAssetBundles(BuildTarget.iOS);
        }


        [MenuItem(MenuItemBuildPlayer)]
        public static void BuildPlayer()
        {
            BuildScript.BuildPlayer();
        }
    }
}