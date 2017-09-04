using System;
using UnityEngine;
using System.Collections;
using System.IO;
using  Framework.LitJson;
using System.Collections.Generic;
namespace Framework
{
    public class AppFile
    {
        public static void LoadFile<T>(string path,ref T value) where T: new()
        {
            string filePath = Path.Combine(AppPath.PersistDataPath, path);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                value = JsonMapper.ToObject<T>(json);
            }
        }
        public static void SaveFile<T>(string path, T value) where T : new()
        {
            string filePath = AppPath.PersistDataPath+path;
            string text = JsonMapper.ToJson(value);
            File.WriteAllText(filePath, text);
        }
    }

}
