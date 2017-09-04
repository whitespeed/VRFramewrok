using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.Events;

namespace Framework
{
    public enum GlobalUIIcon
    {
        None,
        Error,
        NoNetwrok,
    }
    public class GlobalUIManager : Singleton<GlobalUIManager>
    {
        protected Canvas root;
        protected GlobalMask mask;
        protected GlobalPanel panel;
        protected List<Action> ClickOKEvents = new List<Action>(); 
        protected List<Action> ClickCancelEvents = new List<Action>();


        protected void OnClickOK()
        {
            for (int i = 0; i < ClickOKEvents.Count; i++)
            {
                ClickOKEvents[i]();
            }
            ClickOKEvents.Clear();
            Hide();
        }

        protected void OnClickCancel()
        {
            for (int i = 0; i < ClickCancelEvents.Count; i++)
            {
                ClickCancelEvents[i]();
            }
            ClickCancelEvents.Clear();
            Hide();
        }
        public void Show(string msg, Action ok=null, Action cancel=null, string okText = "", string cancelText = "", GlobalUIIcon icon = GlobalUIIcon.None) 
        {
            panel.Init(msg);
            if (ok != null && !ClickOKEvents.Contains(ok))
                ClickOKEvents.Add(ok);
            if(cancel!=null && !ClickCancelEvents.Contains(cancel))
                ClickCancelEvents.Add(cancel);
            panel.SetIcon(icon);
            panel.IsConfirmStyle(cancel != null);
            panel.okText.text = string.IsNullOrEmpty(okText) ? Localization.Get(R.Lang.OK) : okText;
            panel.cancelText.text = string.IsNullOrEmpty(cancelText) ? Localization.Get(R.Lang.Cancel) : cancelText;
            panel.Show();
            mask.Show();
        }

        public bool IsShown()
        {
            return panel.IsShown;
        }

        public void Hide()
        {
            if(mask)
                mask.Hide();
            if(panel)
                panel.Hide();
            ClickOKEvents.Clear();
            ClickCancelEvents.Clear();

        }

        public override void OnInitialize()
        {
            var prefab = VRRes.Load<GameObject>(R.Prefab.GlobalUIRoot);
            root = gameObject.AddChild<Canvas>(prefab);
            root.sortingLayerName = VRSortingLayer.GlobalUI;
            root.worldCamera = Camera.main;
            mask = root.GetComponentInChildren<GlobalMask>();
            mask.Hide();
            panel = root.GetComponentInChildren<GlobalPanel>();
            panel.ok = OnClickOK;
            panel.cancel = OnClickCancel;
            panel.Hide(true);
        }

        public override void OnUninitialize()
        {
            root = null;
            panel = null;
            mask = null;
        }
    }
}
