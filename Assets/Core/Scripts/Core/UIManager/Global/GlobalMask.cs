using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Framework
{
    public class GlobalMask : MonoBehaviour
    {
        public CanvasGroup Group;
        public void Show()
        {
            gameObject.SetActive(true);
            //Group.DOKill(false);
            //Group.DOFade(1f, 0.5f).OnComplete(() =>
            //{
               
            //});
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            //Group.DOKill(false);
            //Group.DOFade(0f, 0.5f).SetDelay(0.2f).OnComplete(() =>
            //{
                
            //});
        }
    }
}

