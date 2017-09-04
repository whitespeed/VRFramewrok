using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class ISceneWidget : MonoBehaviour
    {
        public virtual void OnAttach() { }
        public virtual void OnDetach() { }
    }
    public abstract class ISceneWidget<T> : ISceneWidget where T : IScene
    {
        private T belongScene = null;

        protected T CurScene
        {
            get
            {
                if (null == belongScene)
                {
                    belongScene = SceneManager.CurScene as T;
                }
                return belongScene;
            }
        }

    }
}

