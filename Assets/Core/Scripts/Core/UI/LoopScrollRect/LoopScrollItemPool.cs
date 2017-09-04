using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    public class LoopScrollItemPool
    {
        protected Queue<RectTransform> pool = new Queue<RectTransform>();

        public LoopScrollItemPool(RectTransform parent, RectTransform prefab, int count)
        {
            count = Mathf.Clamp(count, 0, 2);
            while (pool.Count < count)
            {
                CreateChild(parent, prefab);
            }
        }

        public void Release(RectTransform parent, RectTransform child)
        {
            child.gameObject.SetActive(false);
            child.SetParent(parent.parent);
            pool.Enqueue(child);
        }

        private void CreateChild(RectTransform parent, RectTransform prefab)
        {
            var go = GameObject.Instantiate(prefab.gameObject);
            go.layer = parent.gameObject.layer;
            go.SetActive(false);
            var t = (RectTransform) go.transform;
            t.SetParent(parent, false);
            t.SetAsLastSibling();
            t.localScale = Vector3.one;
            go.name = prefab.name;
            pool.Enqueue(t);
        }

        public Transform Get(RectTransform parent, RectTransform prefab)
        {
            if (pool.Count <= 0)
                CreateChild(parent, prefab);
            var child = pool.Dequeue();
            child.SetParent(parent, false);
            child.SetAsLastSibling();
            child.gameObject.SetActive(true);
            return child;
        }
    }
}
