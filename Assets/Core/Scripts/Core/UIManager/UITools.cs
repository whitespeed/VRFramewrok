using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework
{
    public static class UITools
    {

        public static GameObject AddChild(GameObject parent, GameObject prefab, Vector3 localPos = default(Vector3))
        {
            GameObject go = GameObject.Instantiate(prefab) as GameObject;
            Assert.IsNotNull(go);
            Transform t = go.transform;
            if (parent != null)
            {
                t.SetParent(parent.transform);
                go.layer = parent.layer;
            }
            t.localPosition = localPos;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.name = prefab.name;
            return go;
        }
        public static T AddChild<T>(GameObject parent, GameObject prefab, Vector3 local = default(Vector3)) where T:Component
        {
            GameObject child = AddChild(parent, prefab, local);
            return child.GetComponent<T>();
        }
    }



}





