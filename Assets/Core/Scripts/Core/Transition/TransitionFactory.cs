using UnityEngine;
using System.Collections;
using System;

namespace Framework
{

    public class TransitionFactory
    {
        public const string FadeTransition = "TransitionFade";
        public static ITransition CreateTransition(string path)
        {
            ITransition transition = null;
            switch (path)
            {
                case FadeTransition:
                    GameObject o = VRRes.Load<GameObject>(R.Prefab.Fade);
                    GameObject newOne = GameObject.Instantiate(o);
                    transition = newOne.GetComponent<TransitionFade>();
                    break;
                default:
                    break;
            }
            return transition;
        }

        public static ITransition CreateTransition<T>() where T: ITransition
        {
            ITransition transition = null;
            if (typeof (T) == typeof (TransitionFade))
            {
                var o = VRRes.Load<GameObject>(R.Prefab.Fade);
                var newOne = GameObject.Instantiate(o);
                transition = newOne.GetComponent<TransitionFade>();
            }
            return transition;
        }
    }
}

