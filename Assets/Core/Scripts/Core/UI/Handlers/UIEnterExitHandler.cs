using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Framework
{
    public class UIEnterExitHandler : UIHandler<UIEnterExitHandler>, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEvent OnExit = new UnityEvent();
        public UnityEvent OnEnter = new UnityEvent();
        public void OnPointerEnter(PointerEventData eventData)
        { 
            OnEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit.Invoke();
        }
    }

}

