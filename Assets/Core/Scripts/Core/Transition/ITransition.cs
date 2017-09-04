using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

namespace Framework
{
    public interface ITransition
    {
        IEnumerator TransitIn(float duration);
        IEnumerator TransitOut(float duration);
        void DestroySelf();
        void DontDestroyOnLoad();
    }
}

