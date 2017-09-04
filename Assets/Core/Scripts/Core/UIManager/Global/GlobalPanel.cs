using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    public class GlobalPanel : MonoBehaviour
    {
        public Sprite ErrorSprite;
        public Sprite NoNetwrokSprite;
        [Space(20)]
        public Text MainMessage;
        public Button OKBtn;
        public Button CancelBtn;
        public Image IconImage;
        public CanvasGroup Group;
        public Action ok;
        public Text okText;
        public Action cancel;
        public Text cancelText;
        public bool HideOnClick = false;
        protected bool isShown = true;
        public float smoothing = 3f;
        protected Transform selfTransform = null;
        void Awake()
        {
            OKBtn.onClick.AddListener(OnClickOK);
            CancelBtn.onClick.AddListener(OnClickCancel);
            selfTransform = transform;
        }

        void LateUpdate()
        {
            Vector3 rotDegree = VRManager.Viewer.Head.eulerAngles;
            rotDegree.x = rotDegree.z = 0f;
            selfTransform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotDegree), Time.deltaTime * smoothing);
        }
        public void SetIcon(GlobalUIIcon icon)
        {
            IconImage.enabled = (icon != GlobalUIIcon.None);
            switch (icon)
            {
                case GlobalUIIcon.None:
                    IconImage.overrideSprite = null;
                    break;
                case GlobalUIIcon.Error:
                    IconImage.overrideSprite = ErrorSprite;
                    break;
                case GlobalUIIcon.NoNetwrok:
                    IconImage.overrideSprite = NoNetwrokSprite;
                    break;
                default:
                    IconImage.overrideSprite = null;
                    break;
            }
        }
        public void IsConfirmStyle(bool show)
        {
            CancelBtn.gameObject.SetActive(show);
        }
        public virtual void Init(string msg)
        {
            MainMessage.text = msg;
        }
        protected void OnClickOK()
        {
            if (HideOnClick)
                Hide();
            if (ok != null)
            {
                ok();
            }
        }

        protected void OnClickCancel()
        {
            if (HideOnClick)
                Hide();
            if (cancel != null)
            {
                cancel();
            }
        }
        public virtual void Show()
        {
            if (!IsShown)
            {
                isShown = true;
                DOTween.KillAll(false);
                gameObject.SetActive(true);
                Group.DOFade(1f, 0.3f);
                //transform.localScale = new Vector3(0.5f, 0.5f, 1);
                //transform.DOScale(Vector3.one, 0.3f);
            }
        }
        public virtual void Hide(bool immedia = false)
        {
            if (IsShown)
            {
                isShown = false;
                DOTween.KillAll(false);
                if (!immedia)
                {
                    Group.DOFade(0f, 0.3f).OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                    });
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public virtual bool IsShown
        {
            get { return isShown; }
        }
    }
}
