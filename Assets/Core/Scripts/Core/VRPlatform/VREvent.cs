using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Framework
{
    //public interface ISlideHandler : IEventSystemHandler
    //{
    //    void OnSlide(AxisEventData eventData);
    //}

    //public interface IInteractHandler : IEventSystemHandler
    //{
    //    void OnInteract(BaseEventData eventData);
    //}

    public interface IPointerHoverHandler : IEventSystemHandler
    {
        void OnHover(PointerEventData eventData);
    }

    public static class VREvent
    {
        //private static readonly ExecuteEvents.EventFunction<ISlideHandler> s_SlideHandler = Execute;
        //private static void Execute(ISlideHandler handler, BaseEventData eventData)
        //{
        //    handler.OnSlide(ExecuteEvents.ValidateEventData<AxisEventData>(eventData));
        //}
        //public static ExecuteEvents.EventFunction<ISlideHandler> SlideHandler
        //{
        //    get { return s_SlideHandler; }
        //}

        private static readonly ExecuteEvents.EventFunction<IPointerHoverHandler> s_HoverHandler = Execute;
        private static void Execute(IPointerHoverHandler handler, BaseEventData eventData)
        {
            handler.OnHover(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
        }
        public static ExecuteEvents.EventFunction<IPointerHoverHandler> HoverHandler
        {
            get { return s_HoverHandler; }
        }

        //private static readonly ExecuteEvents.EventFunction<IInteractHandler> s_InteractHandler = Execute;
        //private static void Execute(IInteractHandler handler, BaseEventData eventData)
        //{
        //    handler.OnInteract(ExecuteEvents.ValidateEventData<BaseEventData>(eventData));
        //}
        //public static ExecuteEvents.EventFunction<IInteractHandler> InteractHandler
        //{
        //    get { return s_InteractHandler; }
        //}
    }
}

