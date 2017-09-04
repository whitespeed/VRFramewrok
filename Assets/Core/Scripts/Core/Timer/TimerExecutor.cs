using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class TimerExecutor : VRSingleton<TimerExecutor>
    {
        public static Func<long> TimeProducer = GetNowLocalUnixTime;

        public static readonly DateTime UnixLocalTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        public const float TickDeltaSeconds = 0.5f;
        protected List<ITimer> timers = new List<ITimer>();

        private IEnumerator TickTimer()
        {
            var tickDelta = new WaitForSeconds(TickDeltaSeconds);
            while (true)
            {
                TickAllTimer();
                yield return tickDelta;
            }
        }

        public void TickAllTimer()
        {
            if (null == TimeProducer)
                return;
            long timeStamp = TimeProducer();
            for (int i = 0; i < timers.Count; i++)
            {
                timers[i].Tick(timeStamp);
            }
        }

        public void AddTimer(ITimer timer)
        {
            if (!timers.Contains(timer))
            {
                timers.Add(timer);
            }
        }

        public void RemoveTimer(ITimer timer)
        {
            if (timers.Contains(timer))
            {
                timers.Remove(timer);
            }
        }
       
        public static long GetNowLocalUnixTime()
        {
            return (long)(DateTime.Now - UnixLocalTime).TotalMilliseconds;
        }

        public static DateTime UnixToLocalTime(long unixTicks)
        {
            return UnixLocalTime.AddMilliseconds(unixTicks);
        }

        public override void OnInitialize()
        {
            StartCoroutine(TickTimer());
        }

        public override void OnUninitialize()
        {
            TimeProducer = null;
        }
    }
}
