using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework
{

    public abstract class UIWindow : MonoBehaviour
    {
        public bool FrontOnShow = true;
        public bool DestroyOnHide = false;
        public ShowMode ShowMode = ShowMode.DoNothing;
        public bool Persistent = false;
        public UIAnimation show_animation = UIAnimation.common;
        private Transform _transform = null;
        protected new Transform transform
        {
            get
            {
                if (_transform == null)
                {
                    _transform = base.transform;
                }
                return _transform;
            }
        }

        protected IScene scene
        {
            get { return SceneManager.CurScene; }
        }

        protected UIManager root
        {
            get { return SceneManager.CurScene.UIRoot; }
        }


        [ContextMenu("ToggleWindow")]
        public void Toggle()
        {
            if (IsShown)
                Hide();
            else
                Show();
        }
        [ContextMenu("ShowWindow")]
        public void Show()
        {
            if (IsShown)
            {
                return;
            }
            //hide other windows
            if (ShowMode == ShowMode.HideOthers)
            {
                SceneManager.CurScene.UIRoot.HideAllWindows();
            }
            //bring window to top
            if (FrontOnShow)
            {
                SceneManager.CurScene.UIRoot.BringToForeFront(this);
            }
            gameObject.SetActive(true);
            OnShow();
        }
        [ContextMenu("HideWindow")]
        public void Hide()
        {
            if (!IsShown)
            {
                return;
            }
            OnHide();
            if (DestroyOnHide)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
        public virtual bool IsShown
        {
            get { return gameObject.activeInHierarchy; }
        }

        public void SetAsFirstSibling()
        {
            transform.SetAsFirstSibling();
        }

        public void SetAsLastSibling()
        {
            transform.SetAsLastSibling();
        }

        public void SetParent(Transform transfrom)
        {
            transform.SetParent(transfrom);
        }

        public void SetLocalPosition(Vector3 position)
        {
            transform.localPosition = position;
        }

        public void SetLocalEulerAngels(Vector3 angle)
        {
            transform.localEulerAngles = angle;
        }

        public void SetLocalScale(Vector3 localScale)
        {
            transform.localScale = localScale;
        }

        public void AppliedFrom(UIConfig config)
        {
            transform.localPosition = config.LocalPos;
            transform.localEulerAngles = config.LocalEuler;
            transform.localScale = config.LocalScale;
        }

        public virtual void Create() { }
        public virtual void Create(object context) { }
        protected abstract void OnHide();
        protected abstract void OnShow();
        public virtual void ReceiveMessage(int msg, object param) { }



    }

    public enum WindowMsgDef
    {
        SETTINGBRIGHT = 0x10000,
        SETTINGVOLUME,
        PAGE_1,
        PAGE_2,
        TEAM_1, TEAM_2,
        NAVIGATION,
        EXIT
    }
    /// <summary>
    /// 窗口信息
    /// </summary>
    public class WindowMessageDef
    {
        public const int MSG_CLICKONEMPTY = 0x0000003;
        public const int MSG_POINTERHOVER = 0x0000004;       
    }
}

