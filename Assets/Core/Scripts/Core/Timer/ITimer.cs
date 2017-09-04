using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Framework
{

    public abstract class ITimer
    {
        public abstract void Tick(long timeStamp);

        protected void StartTick()
        {
            TimerExecutor.Instance.AddTimer(this);
        }

        protected void StopTick()
        {
            TimerExecutor.Instance.RemoveTimer(this);
        }
    }
}
