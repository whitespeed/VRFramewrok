using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public class MsgDispather
    {
        protected Dictionary<string, List<Action>> listeners = new Dictionary<string, List<Action>>();
        protected string prefix = "[MsgDispather]";

        public MsgDispather(string tag = "")
        {
            if(!string.IsNullOrEmpty(tag))
            {
                prefix = string.Format("[{0}]", tag);
            }
        }

        public void AddListener(string msg, Action listener)
        {
            Debug.Assert(listener != null, "listener != null");
            if(!listeners.ContainsKey(msg))
            {
                listeners[msg] = new List<Action>();
            }
            var l = listeners[msg];
            if(!l.Contains(listener))
            {
                l.Add(listener);
            }
        }

        public bool ContainsKey(string msg)
        {
            return listeners.ContainsKey(msg);
        }

        public void RemoveListner(string msg, Action listener)
        {
            Debug.Assert(listener != null, "listener != null");
            if(listener != null && listeners.ContainsKey(msg))
            {
                var l = listeners[msg];
                if(l.Contains(listener))
                {
                    l.Remove(listener);
                }
            }
        }

        public void ClearListeners(string msg)
        {
            if(listeners.ContainsKey(msg))
            {
                listeners.Remove(msg);
            }
        }

        public virtual void DispathMsg(string msg)
        {
            if(listeners.ContainsKey(msg))
            {
                var l = listeners[msg];
                for(var i = 0; i < l.Count; i++)
                {
                    if(null != l[i])
                    {
                        l[i]();
                    }
                }
            }
        }

        public void ClearAll()
        {
            listeners.Clear();
        }
    }

    public class MsgDispather<T>
    {
        protected Dictionary<string, Action<T>> listeners = new Dictionary<string, Action<T>>();
        protected string prefix = "[MsgDispather]";

        public MsgDispather(string tag = "")
        {
            if(!string.IsNullOrEmpty(tag))
            {
                prefix = string.Format("[{0}]", tag);
            }
        }

        public void AddListener(string msg, Action<T> listener)
        {
            if(!listeners.ContainsKey(msg))
            {
                listeners[msg] = listener;
            }
            else
            {
                listeners[msg] += listener;
            }
        }

        public bool ContainsKey(string msg)
        {
            return listeners.ContainsKey(msg);
        }

        public void RemoveListner(string msg, Action<T> listener)
        {
            if(listener != null && listeners.ContainsKey(msg))
            {
                listeners[msg] -= listener;
                if(null == listeners[msg])
                {
                    listeners.Remove(msg);
                }
            }
        }

        public void ClearListeners(string msg)
        {
            if(listeners.ContainsKey(msg))
            {
                listeners.Remove(msg);
            }
        }

        public virtual void DispathMsg(string msg, T context)
        {
            if(listeners.ContainsKey(msg) && null != listeners[msg])
            {
                listeners[msg](context);
            }
        }

        public void ClearAll()
        {
            listeners.Clear();
        }
    }

    public class MsgDispather<MSG, ARG>
    {
        protected Dictionary<MSG, Action<ARG>> listeners = new Dictionary<MSG, Action<ARG>>();

        public List<MSG> Keys { get { return listeners.Keys.ToList(); } }

        public void AddListener(MSG msg, Action<ARG> listener)
        {
            if(!listeners.ContainsKey(msg))
            {
                listeners[msg] = listener;
            }
            else
            {
                listeners[msg] += listener;
            }
        }

        public bool ContainsKey(MSG msg)
        {
            return listeners.ContainsKey(msg);
        }

        public void RemoveListner(MSG msg, Action<ARG> listener)
        {
            if(listener != null && listeners.ContainsKey(msg))
            {
                listeners[msg] -= listener;
                if(null == listeners[msg])
                {
                    listeners.Remove(msg);
                }
            }
        }

        public void ClearListeners(MSG msg)
        {
            if(listeners.ContainsKey(msg))
            {
                listeners.Remove(msg);
            }
        }

        public virtual void DispathMsg(MSG msg, ARG context)
        {
            if(listeners.ContainsKey(msg) && listeners[msg] != null)
            {
                listeners[msg](context);
            }
        }

        public void ClearAll()
        {
            listeners.Clear();
        }
    }

    public class IncludeMsgDispather<T> : MsgDispather<T>
    {
        public IncludeMsgDispather(string tag = "") : base(tag)
        {
        }

        public override void DispathMsg(string msg, T context)
        {
            if(listeners.Count > 0)
            {
                foreach(var key in listeners.Keys)
                {
                    if(key.Contains(msg) && null != listeners[key])
                    {
                        listeners[key](context);
                    }
                }
            }
        }
    }
}