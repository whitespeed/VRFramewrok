using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{

    public static class UIResource
    {
        public static Dictionary<Type, string> Paths = new Dictionary<Type, string>
        {
        };

        public static GameObject GetPrefab<T>() where T : UIWindow
        {
            return VRRes.Load<GameObject>(UIResource.Paths[typeof(T)]);
        }
    }



}
