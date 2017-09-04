using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Framework
{
    public class UIClickHandler : UIHandler<UIClickHandler>, IPointerClickHandler
    {
        public UnityEvent OnClick = new UnityEvent();
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Invoke();
        }
    }
}

