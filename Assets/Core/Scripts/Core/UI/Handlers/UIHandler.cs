using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Framework
{
    public abstract class UIHandler<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Get(GameObject o)
        {
            var handler = o.GetComponent<T>();
            if (null == handler)
            {
                handler = o.AddComponent<T>();
            }
            return handler;
        }
    }

    [RequireComponent(typeof(RawImage))]
    public class UITextureHandler : UIHandler<UITextureHandler>
    {

        protected RawImage imag;
        protected string errorURL = string.Empty;
        public RawImage Image
        {
            get
            {
                if (null == imag)
                {
                    imag = GetComponent<RawImage>();
                }
                return imag;
            }
        }

        public void Load(string url, string errorUrl = "")
        {
            if (!string.IsNullOrEmpty(url))
            {
                var texture = TextureManager.Instance.FindInMemory(url);
                if (texture == null)
                {
                    StartCoroutine(LoadAsync(url, errorUrl));
                }
                else
                {                    
                    Image.texture = texture;
                    //imag.CrossFadeColor(Color.white, 1.5f, false, true);
                }
            }
            else
            {
                Debug.LogError("[TextureHandler] Texture url is null or empty.", gameObject);
            }
        }

        IEnumerator LoadAsync(string url, string backup = "")
        {
            var r = TextureManager.LoadAsync(url, -1, -1);
            yield return r;
            if (string.IsNullOrEmpty(r.error))
            {
                Image.texture = r.result;
                //Image.CrossFadeColor(Color.white, 1.5f, false, true);
            }
            else
            {
                if (!string.IsNullOrEmpty(backup))
                {
                    StartCoroutine(LoadAsync(backup));
                }
            }
        }
    }
}

