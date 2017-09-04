using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;


namespace Framework
{
    public class VRSplash : MonoBehaviour
    {
        public static float DelaySeconds = 0f;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(DelaySeconds);
            VRApplication.Start();
        }
    }

}





