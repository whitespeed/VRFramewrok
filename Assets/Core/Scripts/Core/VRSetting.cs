using System;
using UnityEngine;
using System.Collections;
using System.IO;
using  Framework.LitJson;
using System.Collections.Generic;
using System.ComponentModel;

namespace Framework
{
    
    public class VRSetting
    {
        public struct PrefKey<T>
        {
            public T defaultValue;
            public string path;

            public PrefKey(string k)
            {
                defaultValue = default(T);
                path = k;
            }

            public PrefKey(string k, T dValue)
            {
                defaultValue = dValue;
                path = k;
            } 
        }

        public static T GetSetting<T>(VRSetting.PrefKey<T> key)
        {

            if (PlayerPrefs.HasKey(key.path))
            {
                string value = PlayerPrefs.GetString(key.path);
                if (typeof (T).IsValueType || typeof(T) ==  typeof(string))
                {
                    var foo = TypeDescriptor.GetConverter(typeof(T));
                    return (T)(foo.ConvertFromInvariantString(value));
                }
                else
                {
                    T re = JsonMapper.ToObject<T>(value);
                    return re;
                }
            }
            else
            {
                return key.defaultValue;
            }
        }
        public static void SetSetting<T>(VRSetting.PrefKey<T> key, T value)
        {
            var content = typeof(T).IsValueType ? value.ToString() : JsonMapper.ToJson(value);
            PlayerPrefs.SetString(key.path, content);
        }
    }

}
