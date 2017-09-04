using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Framework
{
    public class UIDownUpHandler : UIHandler<UIDownUpHandler>, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent OnDown = new UnityEvent();
        public UnityEvent OnUp = new UnityEvent();
        public void OnPointerDown(PointerEventData eventData)
        {
            OnDown.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnUp.Invoke();
        }
    }
}

