using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    public class ScrollItemPool<T> where T:MonoBehaviour
    {
        protected Queue<T> pool = new Queue<T>();
        protected GameObject parent;
        protected T prefab;
        public ScrollItemPool(GameObject par, T pre,int preCount = 1)
        {
            parent = par;
            prefab = pre;
            for (int i = 0; i < preCount; i++)
            {
                Release(Get());
            }
        }

        public void Release(T item)
        {
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }

        public T Get()
        {
            T child = null;
            if (pool.Count > 0)
            {
                child = pool.Dequeue();
                child.transform.SetAsLastSibling();
            }
            else
            {
                child = parent.AddChild<T>(prefab.gameObject);
            }
            child.gameObject.SetActive(true);
            return child;
        }
    }
    public class VRLogUI : VRSingleton<VRLogUI>
    {
        protected Queue<string> cacheToShow = new Queue<string>();
        protected LogType filterLogType = LogType.Error;
        protected RawImage colorCodeView;
        protected bool dirty;
        protected Text frameDiagnosis;
        protected Canvas root;
        protected Button clear;
        protected ScrollRect scroll;
        protected Text text;

        protected ScrollItemPool<Text> logTexts;
        protected List<Text> shownTexts = new List<Text>(); 
        protected string lstLog = string.Empty;
        private void LogMessageReceived(string strLog, string strStackTrace, LogType type)
        {
            if(!IsLogTypeAllowed(type))
                return;

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                if (!string.IsNullOrEmpty(strStackTrace))
                {
                    lstLog = string.Format("<color=red>{0}\n{1}</color>\n\n ", strLog, strStackTrace);
                }
                else
                {
                    lstLog = string.Format("<color=red>{0</color>\n\n", strLog);
                }
            }
            else if (type == LogType.Warning)
                lstLog = string.Format("<color=yellow>{0}</color>\n\n", strLog);
            else
                lstLog = string.Format("{0}\n\n",strLog);
            cacheToShow.Enqueue(lstLog);
            dirty = true;
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            return ((logType <= this.filterLogType) || (logType == LogType.Exception));
        }


        private void Update()
        {
            if (frameDiagnosis)
            {
                frameDiagnosis.text = string.Format("{0:00.00}", 1/Time.smoothDeltaTime);
            }

            if (!dirty) return;
            dirty = false;
            while (cacheToShow.Count>0)
            {
                var log = cacheToShow.Dequeue();
                var child = logTexts.Get();
                child.text = log;
                shownTexts.Add(child);
            }
            DOTween.To(() => scroll.verticalNormalizedPosition, value => { scroll.verticalNormalizedPosition = value; },
                0f, 1f);
        }

        public override void OnUninitialize()
        {
            Application.logMessageReceived -= LogMessageReceived;
            if(clear)
                clear.onClick.RemoveListener(Clear);
        }

        public override void OnInitialize()
        {
            root = gameObject.AddChild<Canvas>(VRRes.Load<GameObject>(R.Prefab.VRLog));
            root.sortingLayerName = VRSortingLayer.Log;
            root.worldCamera = Camera.main;
            text = root.GetComponentInChildren<Text>(true);
            clear = root.GetComponentInChildren<Button>(true);
            if(clear)
                clear.onClick.AddListener(Clear);
            var frameRateTran = root.transform.FindChildRecursively("FrameRate");
            if (frameRateTran)
            {
                frameDiagnosis = frameRateTran.GetComponent<Text>();
            }
            scroll = root.GetComponentInChildren<ScrollRect>(true);
            logTexts = new ScrollItemPool<Text>(scroll.content.gameObject,text,1);
            PrintAppInfo();
            Application.logMessageReceived += LogMessageReceived;
            
        }

        private void PrintAppInfo()
        {
            var log = string.Format("{0}\nVersion:{1}\nPlatform:{2}\n", VRApplication.AppName, VRApplication.Version,
                SystemInfo.operatingSystem);
            text.text = log;
        }

        public void Clear()
        {
            for (int i = 0; i < shownTexts.Count; i++)
            {
                logTexts.Release(shownTexts[i]);
            }
            shownTexts.Clear();
        }













        public void SetColorCodeForLog(Texture colorCache)
        {
            if (GetColorComponent())
            {
                colorCodeView.gameObject.SetActive(true);
                colorCodeView.texture = colorCache;
            }
        }

        public void DisableColorCode()
        {
            if (GetColorComponent())
            {
                colorCodeView.gameObject.SetActive(false);
            }
        }

        private bool GetColorComponent()
        {
            if (!colorCodeView)
            {
                if (root)
                {
                    var ColorView = root.transform.FindChildRecursively("ColorCodeView");
                    if (ColorView)
                    {
                        ColorView.gameObject.SetActive(true);
                        colorCodeView = ColorView.GetComponent<RawImage>();
                    }
                }
            }
            return colorCodeView.IsActive();
        }
    }
}