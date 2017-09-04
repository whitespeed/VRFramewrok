using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Framework
{
    public interface IVRController
    {
        event Action OnBackButton;
        bool ClickBtnDown { get; }
        bool ClickBtn { get; }
        bool ClickBtnUp { get; }
        void OnClickBack();
        bool ClickOnce();

        bool IsTouching { get; }
        Vector2 TouchPos { get; }
    }


}
