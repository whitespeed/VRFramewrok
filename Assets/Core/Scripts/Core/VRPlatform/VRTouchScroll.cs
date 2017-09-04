using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Framework
{
    [RequireComponent(typeof(ScrollRect))]
    public class VRTouchScroll : MonoBehaviour
    {
        protected ScrollRect target = null;
        protected Vector2 anchoredPos = Vector2.zero;
        protected bool beginDragging = false;
        protected Vector2 lastInputPos = Vector2.zero;
        private void Awake()
        {
            target = GetComponent<ScrollRect>();
            target.onValueChanged.AddListener(OnValueChanged);
        }
        protected void OnValueChanged(Vector2 delta)
        {
            if(IsDragging)
            {
                Debug.Log("Change");
                target.content.anchoredPosition = anchoredPos;
            }
        }
        protected static bool IsDragging
        {
            get {
                return Input.GetMouseButton(0);
            }
        }

        protected static Vector2 InputPos
        {
            get
            {
                return Input.mousePosition;
            }
        }

        private void LateUpdate()
        {
            if(IsDragging)
            {
                if(!beginDragging)
                {
                    beginDragging = true;
                    lastInputPos = InputPos;
                }
                var delta = InputPos - lastInputPos;
                if (!target.horizontal)
                    delta.x = 0;
                if (!target.vertical)
                    delta.y = 0;
                target.content.anchoredPosition += delta;
                anchoredPos = target.content.anchoredPosition;
                lastInputPos = InputPos;
            }
            else
            {
                beginDragging = false;
            }
        }
    }
}

