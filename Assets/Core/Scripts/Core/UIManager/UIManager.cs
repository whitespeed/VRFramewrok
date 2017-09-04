using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework
{
    [RequireComponent(typeof (Canvas))]
    public class UIManager :  IGlobalHandler
    {
        protected Canvas canvas;

        protected List<UIWindow> windowsCache = new List<UIWindow>();

        public Canvas Canvas
        {
            get
            {
                if (null == canvas)
                    canvas = GetComponent<Canvas>();
                return canvas;
            }
        }


        #region Implement of IGlobalHandler
        public override void OnGlobalClick(PointerEventData eventData)
        {
            if (eventData == null || eventData.enterEventCamera == null)
            {
                BroadcastWindowMessage(WindowMessageDef.MSG_CLICKONEMPTY, null);
            }
        }
        #endregion

        private void Awake()
        {
            if (FindObjectsOfType<UIManager>().Length > 1)
            {
                Debug.LogError("[WindowManager] Something went really wrong " +
                               " - there should never be more than 1 singleton!" + " Reopening the scene might fix it.");
            }
        }

        private void OnEnable()
        {
            VRInputModule.InputHandler = this;
        }

        private void Disable()
        {
            VRInputModule.InputHandler = null;
        }


        public void BringToForeFront(UIWindow window)
        {
            if (window != null)
                window.transform.SetAsLastSibling();
        }

        public T CreateWindow<T>() where T : UIWindow
        {
            var window_obj = UITools.AddChild(gameObject, UIResource.GetPrefab<T>());
            var res = window_obj.GetComponentInChildren<T>(true);
            res.Create();
            res.gameObject.SetActive(false);
            return res;
        }

        public T CreateWindow<T>(object obj) where T : UIWindow
        {
            var window_obj = UITools.AddChild(gameObject, UIResource.GetPrefab<T>());
            var res = window_obj.GetComponentInChildren<T>(true);
            res.Create(obj);
            res.gameObject.SetActive(false);
            return res;
        }

        public T GetWindow<T>(bool include_hide = true) where T : UIWindow
        {
            return GetComponentInChildren<T>(include_hide);
        }

        public UIWindow GetFirstShownWidnow()
        {
            windowsCache.Clear();
            GetComponentsInChildren(true, windowsCache);
            for (var i = 0; i < windowsCache.Count; i++)
            {
                if (null != windowsCache[i] && windowsCache[i].IsShown)
                {
                    var window = windowsCache[i];
                    windowsCache.Clear();
                    return window;
                }
            }
            windowsCache.Clear();
            return null;
        }

        public void ShowWindow<T>() where T : UIWindow
        {
            var window = GetWindow<T>(true);
            if (null != window)
            {
                window.Show();
            }
        }

        public void ToggleWindow<T>() where T : UIWindow
        {
            var window = GetWindow<T>(true);
            if (null == window)
                return;
            if (window.IsShown)
            {
                window.Hide();
            }
            else
            {
                window.Show();
            }
        }

        public void HideWindow<T>() where T : UIWindow
        {
            var window = GetWindow<T>(true);
            if (null != window)
            {
                window.Hide();
            }
        }

        public void HideAllWindows()
        {
            windowsCache.Clear();
            GetComponentsInChildren(true, windowsCache);
            for (var i = 0; i < windowsCache.Count; i++)
            {
                var window = windowsCache[i];
                if (window != null && window.IsShown && !window.Persistent)
                    window.Hide();
            }
            windowsCache.Clear();
        }

        public void SendWindowMessage(UIWindow window, int msg, object param)
        {
            if (null != window)
            {
                window.ReceiveMessage(msg, param);
            }
        }

        public void BroadcastWindowMessage(int msg)
        {
            BroadcastWindowMessage(msg, null);
        }
        public void BroadcastWindowMessage(int msg, object param)
        {
            windowsCache.Clear();
            GetComponentsInChildren(true, windowsCache);
            for (var i = 0; i < windowsCache.Count; ++i)
            {
                if (null != windowsCache[i])
                    windowsCache[i].ReceiveMessage(msg, param);
            }
            windowsCache.Clear();
        }
    }
}