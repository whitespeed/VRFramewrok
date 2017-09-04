
using UnityEngine;
using System;

namespace Framework
{
    public partial class UIAnimation
    {
        public static UIAnimation common = new UIAnimation();
    }

    public partial class UIAnimation 
    {
        public virtual void BeginShow(GameObject window, Action onComplete)
        {
            onComplete();
        }

        public virtual void BeginHide(GameObject window, Action onComplete)
        {
            onComplete();
        }

        public virtual void Reset(bool show)
        {
        }
    }
}



