using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    public abstract class BuiltInEditor : Editor
    {
        private static readonly object[] EMPTY_ARRAY = new object[0];


        private static readonly Dictionary<string, MethodInfo> decoratedMethods = new Dictionary<string, MethodInfo>();

        private static readonly Assembly editorAssembly = Assembly.GetAssembly(typeof (Editor));

        private readonly Type decoratedEditorType;


        private Type editedObjectType;

        private Editor editorInstance;

        protected BuiltInEditor(string editorTypeName)
        {
            decoratedEditorType = editorAssembly.GetTypes().FirstOrDefault(t => t.Name == editorTypeName);

            Init();

            // Check CustomEditor types.
            var originalEditedType = GetCustomEditorType(decoratedEditorType);

            if (originalEditedType != editedObjectType)
            {
                throw new ArgumentException(
                    string.Format("Type {0} does not match the editor {1} type {2}",
                        editedObjectType, editorTypeName, originalEditedType));
            }
        }

        protected Editor EditorInstance
        {
            get
            {
                if (editorInstance == null && targets != null && targets.Length > 0)
                {
                    editorInstance = CreateEditor(targets, decoratedEditorType);
                }

                if (editorInstance == null)
                {
                    Debug.LogError("Could not create editor !");
                }

                return editorInstance;
            }
        }

        private Type GetCustomEditorType(Type type)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var attributes = type.GetCustomAttributes(typeof (CustomEditor), true) as CustomEditor[];
            if (attributes != null)
            {
                var field = attributes.Select(editor => editor.GetType().GetField("m_InspectedType", flags)).First();

                return field.GetValue(attributes[0]) as Type;
            }
            else
            {
                return null;
            }
        }

        private void Init()
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var attributes = GetType().GetCustomAttributes(typeof (CustomEditor), true) as CustomEditor[];
            if (attributes != null)
            {
                var field = attributes.Select(editor => editor.GetType().GetField("m_InspectedType", flags)).First();

                editedObjectType = field.GetValue(attributes[0]) as Type;
            }
        }

        private void OnDisable()
        {
            if (editorInstance != null)
            {
                DestroyImmediate(editorInstance);
            }
        }
        protected void CallInspectorMethod(string methodName)
        {
            MethodInfo method = null;

            // Add MethodInfo to cache
            if (!decoratedMethods.ContainsKey(methodName))
            {
                var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

                method = decoratedEditorType.GetMethod(methodName, flags);

                if (method != null)
                {
                    decoratedMethods[methodName] = method;
                }
                else
                {
                    Debug.LogError(string.Format("Could not find method {0}", method));
                }
            }
            else
            {
                method = decoratedMethods[methodName];
            }

            if (method != null)
            {
                method.Invoke(EditorInstance, EMPTY_ARRAY);
            }
        }

        public void OnSceneGUI()
        {
            CallInspectorMethod("OnSceneGUI");
        }

        protected override void OnHeaderGUI()
        {
            CallInspectorMethod("OnHeaderGUI");
        }

        public override void OnInspectorGUI()
        {
            EditorInstance.OnInspectorGUI();
        }

        public override void DrawPreview(Rect previewArea)
        {
            EditorInstance.DrawPreview(previewArea);
        }

        public override string GetInfoString()
        {
            return EditorInstance.GetInfoString();
        }

        public override GUIContent GetPreviewTitle()
        {
            return EditorInstance.GetPreviewTitle();
        }

        public override bool HasPreviewGUI()
        {
            return EditorInstance.HasPreviewGUI();
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            EditorInstance.OnInteractivePreviewGUI(r, background);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            EditorInstance.OnPreviewGUI(r, background);
        }

        public override void OnPreviewSettings()
        {
            EditorInstance.OnPreviewSettings();
        }

        public override void ReloadPreviewInstances()
        {
            EditorInstance.ReloadPreviewInstances();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            return EditorInstance.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        public override bool RequiresConstantRepaint()
        {
            return EditorInstance.RequiresConstantRepaint();
        }

        public override bool UseDefaultMargins()
        {
            return EditorInstance.UseDefaultMargins();
        }
    }
}