using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
    public class GraphicTool : Editor
    {
        protected static string PathCGInclude = EditorApplication.applicationContentsPath+@"/CGIncludes/UnityCG.cginc";
        protected static string PathHLSLInclude = EditorApplication.applicationContentsPath + @"/CGIncludes/HLSLSupport.cginc";
        protected static string PathGLSLInclude = EditorApplication.applicationContentsPath + @"/CGIncludes/UnityCG.glslinc";
        protected static string PathLighting = EditorApplication.applicationContentsPath + @"/CGIncludes/Lighting.cginc";
        protected static string PathShaderVariables = EditorApplication.applicationContentsPath + @"/CGIncludes/UnityShaderVariables.cginc";

        [MenuItem("Shader/Open ShaderVariables.cginc")]
        protected static void OpenShaderVariables()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(PathShaderVariables, 0);
        }

        [MenuItem("Shader/Open UnityCG.cginc")]
        protected static void OpenCGInclude()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(PathCGInclude, 0);
        }


        [MenuItem("Shader/Open HLSLSupport.cginc")]
        protected static void OpenHLSLSupport()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(PathHLSLInclude, 0);
        }

        [MenuItem("Shader/Open UnityCG.glslinc")]
        protected static void OpenGLSLInclude()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(PathGLSLInclude, 0);
        }

        [MenuItem("Shader/Open Lighting.cginc")]
        protected static void OpenLightingInclude()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(PathLighting, 0);
        }

        public static void AddAlwaysIncludedShader(Shader shader)
        {

        }

        public static void RemoveAlwaysIncludedShader(Shader shader)
        {
            
        }

        public static bool IsAlwaysIncludedShader(Shader shader)
        {
            return false;
        }
    }
}

