using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
    public static class MonoBehaviourExtensions
    {
        /**
                Invokes the given action after the given amount of time.

                The action must be a method of the calling class.
            */
        public static void Invoke(this MonoBehaviour component, System.Action action, float time)
        {
            component.Invoke(action.Method.Name, time);
        }

        public static IEnumerator NextFrameAction(System.Action action)
        {
            yield return 1;
            action();
        }

        public static void InvokeNextFrame(this MonoBehaviour component, System.Action action)
        {
            component.StartCoroutine(NextFrameAction(action));
        }
        /**
                Invokes the given action after the given amount of time, and repeats the 
                action after every repeatTime seconds.

                The action must be a method of the calling class.
            */
        public static void InvokeRepeating(this MonoBehaviour component, System.Action action, float time, float repeatTime)
        {
            component.InvokeRepeating(action.Method.Name, time, repeatTime);
        }

        /**
                Invokes an action after a random time between the minimum and 
                maximum times given.
            */
        public static void InvokeRandom(this MonoBehaviour component, System.Action action, float minTime, float maxTime)
        {
            var time = UnityEngine.Random.value * (maxTime - minTime) + minTime;

            component.Invoke(action, time);
        }

        /**
                Cancels the action if it was scheduled.
            */
        public static void CancelInvoke(this MonoBehaviour component, System.Action action)
        {
            component.CancelInvoke(action.Method.Name);
        }

        /**
                Returns whether an invoke is pending on an action.
            */
        public static bool IsInvoking(this MonoBehaviour component, System.Action action)
        {
            return component.IsInvoking(action.Method.Name);
        }


        /**
                Gets an attached component that implements the interface of the type parameter.
            */
        public static I GetInterfaceComponent<I>(this MonoBehaviour thisComponent) where I : class
        {
            return thisComponent.GetComponent(typeof(I)) as I;
        }

    }

}

