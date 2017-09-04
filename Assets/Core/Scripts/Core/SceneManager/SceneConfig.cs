using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.LitJson;
namespace Framework
{

    public class SceneConfig
    {
        protected JsonData data = null;

        public SceneConfig(string name)
        {
            var textAsset = VRRes.Load<TextAsset>(string.Format("Config/Scenes/{0}",name));
            if (null != textAsset && !string.IsNullOrEmpty(textAsset.text))
            {
                try
                {
                    data = JsonMapper.ToObject(textAsset.text);
                }
                catch (Exception)
                {
                    Debug.LogErrorFormat("[SceneConfig] read config of scene: {0} error",name);
                }
            }
        }

        protected T GetValue<T>(string key)
        {
            if (null != data && data.Keys.Contains(key))
            {
                return JsonMapper.ToObject<T>(data[key].ToJson());
            }
            else
            {
                return default(T);
            }
        }

        public Vector2 GetVector2(string key)
        {
            return GetValue<Vector2>(key);
        }

        public Vector3 GetVector3(string key)
        {
            return GetValue<Vector3>(key);
        }

        public bool GetBoolean(string key)
        {
            if (null != data && data.Keys.Contains(key))
            {
                return (bool)data[key];
            }
            else
            {
                return false;
            }
        }

        public int GetInt(string key)
        {
            if (null!=data && data.Keys.Contains(key))
            {
                return (int) data[key];
            }
            else
            {
                return 0;
            }
        }

        public string GetString(string key)
        {
            if (null!=data && data.Keys.Contains(key))
            {
                return (string)data[key];
            }
            else
            {
                return string.Empty;
            }
        }

        public float GetFloat(string key)
        {
            if (null!=data && data.Keys.Contains(key))
            {
                return (float)data[key];
            }
            else
            {
                return 0f;
            }
        }

        public UIConfig GetWindowConfig(string key)
        {
            return GetValue<UIConfig>(key);
        }
    }
}

