using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace Framework
{
    public class Setup
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAppLoad()
        {
            VRApplication.IsDebug = IsDebugModeInEditor;
        }

        private static bool IsDebugModeInEditor
        {
            get { return EditorPrefs.GetBool("IsDebug", true); }
        }

        [MenuItem("Debug/Enable", true)]
        private static bool DebugModeMenuEnable()
        {
            return !IsDebugModeInEditor;
        }

        [MenuItem("Debug/Disable", true)]
        private static bool DebugModeMenuDisable()
        {
            return IsDebugModeInEditor;
        }

        [MenuItem("Debug/Enable", false)]
        private static void EnableDebugMode()
        {
            EditorPrefs.SetBool("IsDebug", true);
        }

        [MenuItem("Debug/Disable", false)]
        private static void DisableDebugMode()
        {
            EditorPrefs.SetBool("IsDebug", false);
        }
    }
}