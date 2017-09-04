using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Framework
{
    public class ThreadTool : Singleton<ThreadTool>
    {
        public const int maxThreads = 8;

        private static int numThreads;
        private static float _SpeedRate = 1f;
        private static bool KillMainThread;

        private int _count;

        private readonly List<QueueItem> _actions = new List<QueueItem>();
        private readonly List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

        private readonly List<QueueItem> _currentActions = new List<QueueItem>();
        private readonly List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

        /// <summary>
        /// Rate of the Main Threading Update Speed, default is 1.
        /// </summary>
        public static float SpeedRate { get { return _SpeedRate; } set { _SpeedRate = value == 0 ? 100f : 1/value; } }

        public static void QueueOnMainThread(Action<object> action, object param)
        {
            QueueOnMainThread(action, param, 0f);
        }

        public static void QueueOnMainThread(Action<object> action, object param, float time)
        {
            if(time != 0)
            {
                lock(Instance._delayed)
                {
                    Instance._delayed.Add(new DelayedQueueItem {time = Time.time + time, action = action, param = param});
                }
            }
            else
            {
                lock(Instance._actions)
                {
                    Instance._actions.Add(new QueueItem {action = action, param = param});
                }
            }
        }

        public static Thread RunAsync(Action<object> a, object param, float speed)
        {
            _SpeedRate = speed;
            return RunAsync(a, param);
        }

        public static Thread RunAsync(Action<object> a, object param)
        {
            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, new QueueItem {action = a, param = param});
            return null;
        }

        public static void KillTheMainThread()
        {
            KillMainThread = true;
        }

        public override void OnInitialize()
        {
        }

        public override void OnUninitialize()
        {
        }

        private void Awake()
        {
            StartCoroutine(RunningOnMainThread());
        }

        private static void RunAction(object obj)
        {
            try
            {
                var item = (QueueItem) obj;
                item.action(item.param);
            }
            catch
            {
            }
            finally
            {
                Interlocked.Decrement(ref numThreads);
            }
        }

        private void OnApplicationQuit()
        {
            KillMainThread = true;
        }

        private IEnumerator RunningOnMainThread()
        {
            while(true)
            {
                lock(_actions)
                {
                    _currentActions.Clear();
                    _currentActions.AddRange(_actions);
                    _actions.Clear();
                }
                foreach(var item in _currentActions)
                {
                    item.action(item.param);
                }
                lock(_delayed)
                {
                    _currentDelayed.Clear();
                    _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
                    foreach(var item in _currentDelayed)
                    {
                        _delayed.Remove(item);
                    }
                }
                foreach(var delayed in _currentDelayed)
                {
                    delayed.action(delayed.param);
                }
                yield return 1;
                if(KillMainThread)
                {
                    break;
                }
            }
            //yield return new WaitForEndOfFrame();
        }

        private class QueueItem
        {
            public Action<object> action;
            public object param;
        }

        private class DelayedQueueItem : QueueItem
        {
            public float time;
        }
    }
}