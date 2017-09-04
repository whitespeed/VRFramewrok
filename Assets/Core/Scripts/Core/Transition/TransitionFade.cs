using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using System;

namespace Framework
{
    public class TransitionFade : MonoBehaviour, ITransition
    {

        public Renderer render;

        public IEnumerator TransitIn(float duration)
        {
            render.material.color = Color.clear;
            gameObject.SetActive(true);
            yield return render.material.DOColor(Color.black, duration).SetEase(Ease.Linear).WaitForCompletion();
        }

        public IEnumerator TransitOut(float duration)
        {
            yield return render.material.DOColor(Color.clear, duration).SetEase(Ease.Linear).WaitForCompletion();
            gameObject.SetActive(false);
        }

        public void DestroySelf()
        {
            Destroy(this.gameObject);
        }

        public void DontDestroyOnLoad()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }
}




