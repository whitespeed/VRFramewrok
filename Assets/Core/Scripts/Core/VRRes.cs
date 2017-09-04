using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public static class VRRes
    {
        public static T Load<T>(string path) where T : Object
        {
            if(typeof(T) == typeof(Shader))
            {
                return Shader.Find(path) as T;
            }
            else
            {
                return Resources.Load<T>(path);
            }
        }
        public static void Unload(Object obj)
        {
            Resources.UnloadAsset(obj);
        }
    }

    public static class VRResAsync
    {
        
    }
}