using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Framework
{
    public class UIHoverHandler : UIHandler<UIHoverHandler>, IPointerHoverHandler
    {
        public UnityEvent onHover = new UnityEvent();
        public void OnHover(PointerEventData eventData)
        {
            onHover.Invoke();
        }
    }
}

