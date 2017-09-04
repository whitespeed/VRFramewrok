using UnityEngine;
using UnityEngine.Assertions;

namespace Framework
{

    public static class GameObjectExtensions
    {
        public static GameObject AddChild(this GameObject parent, GameObject prefab, 
            Vector3 localPos = default(Vector3), Vector3 localEuler = default(Vector3))
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
            t.localEulerAngles = localEuler;
            t.localScale = Vector3.one;
            go.name = prefab.name;
            return go;
        }

        public static T AddChild<T>(this GameObject parent, GameObject prefab, Vector3 localPos = default(Vector3), Vector3 localEuler = default(Vector3)) where T : Component
        {
            GameObject child = AddChild(parent, prefab, localPos, localEuler);
            return child.GetComponent<T>();
        }
    }
}
