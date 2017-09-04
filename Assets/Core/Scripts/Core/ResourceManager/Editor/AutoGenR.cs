using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
    public class AutoGenR
    {

        protected static readonly List<string> SearchPaths = new List<string>
        {
            Path.Combine(Application.dataPath, string.Format("{0}/Resources",VRApplication.AppName)),
            Path.Combine(Application.dataPath,string.Format("{0}/Shaders", VRApplication.AppName)),
        };
        protected static readonly string ClassFilePath = Path.Combine(Application.dataPath, "AutoGen");
        protected static readonly string ClassFileName = @"R.cs";
        protected static readonly string ClassContentFormat =
            "/*This is generated automatically. DONT EDIT BY HAND*/ \npublic static partial class R {{\n{0}\n}};";

        protected static readonly string InfoContentFormat =
            "/*This is generated automatically. DONT EDIT BY HAND*/ \n /*Type, Extension, ResPath, ABName*/ \n{0}\n";


        protected List<IAutoGenerator> generators = new List<IAutoGenerator>();



        [MenuItem("Tools/Auto Generate R")]
        public static void SearchAllAndGen()
        {
            var u = new AutoGenR(new ConfigAutoGen(),new TextureAutoGen(),new AudioAutoGen(),new PrefabAutoGen(),new LanguageAutoGen(),new ShaderAutoGen());
            u.SearchAll();
            u.GenRClass();
            u.GenRInfo();
        }

        public AutoGenR(params IAutoGenerator[] gens)
        {
            for (var i = 0; i < gens.Length; ++i)
            {
                generators.Add(gens[i]);
            }
        }

        public void GenRClass()
        {
            if (!Directory.Exists(ClassFilePath))
            {
                Directory.CreateDirectory(ClassFilePath);
            }
            var path = Path.Combine(ClassFilePath, ClassFileName);
            var builder = new StringBuilder();
            for (var i = 0; i < generators.Count; ++i)
            {
                builder.AppendLine(generators[i].GenerateRClass());
            }
            File.WriteAllText(path, string.Format(ClassContentFormat, builder));
            AssetDatabase.Refresh();
        }


        public void GenRInfo()
        {
            var assetmanifestFullPath = AssetUtility.GetFullPathFromAssetsPath(AssetManifest.AssetPath);
            if (!Directory.Exists(assetmanifestFullPath))
            {
                Directory.CreateDirectory(assetmanifestFullPath);
            }
            var path = AssetUtility.RelativeAssetsPath(Path.Combine(assetmanifestFullPath, AssetManifest.AssetName));
            var manifest = ScriptableObject.CreateInstance<AssetManifest>();
            for (var i = 0; i < generators.Count; ++i)
            {
                manifest.AddRange(generators[i].GenerateRInfo());
            }
            AssetDatabase.CreateAsset(manifest, path);
            AssetImporter import = AssetImporter.GetAtPath(path);
            import.assetBundleName = AssetManifest.BundleName;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void SearchAll()
        {
            var resDirs = new List<DirectoryInfo>();
            foreach (var path in SearchPaths)
            {
                resDirs.Add(new DirectoryInfo(path));
            }
            foreach (var e in resDirs)
            {
                var files = e.GetFiles("*", SearchOption.AllDirectories)
                    .Where(file => !file.Name.EndsWith(".meta"));
                foreach (var f in files)
                {
                    for (var i = 0; i < generators.Count; ++i)
                    {
                        if (generators[i].Belong(f.FullName))
                        {
                            try
                            {
                                generators[i].AddRes(f.FullName);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("Auto generate class R failed in: " + f.FullName, AssetUtility.LoadAssetAtFullPath<UnityEngine.Object>(f.FullName));
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }


    }
}

