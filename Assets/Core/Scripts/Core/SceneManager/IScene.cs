using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Framework
{
    public abstract class IScene : MonoBehaviour
    {
        public const string ScenePoolName = "[scene] ScenePool";
        private readonly MsgDispather<object> events = new MsgDispather<object>("IScene");
        private SceneConfig config;
        private Object lastGot;


        protected SpawnPool pool;
        protected string sceneName = "undefined";
        private UIManager uiRoot;

        public UIManager UIRoot
        {
            get
            {
                if (null == uiRoot)
                {
                    CreateUIRoot();
                }
                return uiRoot;
            }
        }

        public string Name
        {
            get { return sceneName; }
            internal set { sceneName = value; }
        }

        public GameObject UnityObject
        {
            get { return gameObject; }
        }

        public SceneConfig Config
        {
            get
            {
                if (null == config)
                {
                    config = new SceneConfig(GetType().Name);
                }
                return config;
            }
        }

        public MsgDispather<object> Events
        {
            get { return events; }
        }

        public virtual void Preload(object o)
        {
            VRManager.Controller.OnBackButton += OnClickEsc;
            //CreatePool();
            CreateUIRoot();
        }

        public virtual void Unload()
        {
            WidgetsControl(false);
            events.ClearAll();
            uiRoot = null;
            pool = null;
            VRManager.Controller.OnBackButton -= OnClickEsc;
        }

        public virtual void Loaded()
        {
            WidgetsControl(true);
            if (null != VRManager.Viewer)
            {
                var VRes = VRManager.Viewer.Reticle;
                VRes.OnGazeDisabled();
                //VRManager.Viewer.Recent();
            }
        }

        public virtual void Opened()
        {
            if (null != VRManager.Viewer)
            {
                var VRes = VRManager.Viewer.Reticle;
                VRes.OnGazeEnabled();
            }
        }

        public abstract IEnumerator Wait();

        protected void OnClickEsc()
        {
            GlobalUIManager.Instance.Hide();
            OnSceneReturn();
        }

        protected void CreatePool()
        {
            if (pool != null)
                return;
            var e = PoolsManager.Pools.GetEnumerator();
            while (e.MoveNext())
            {
                if (PoolsManager.GlobalPoolName != e.Current.Value.poolName)
                {
                    pool = e.Current.Value;
                    break;
                }
            }

            if (null == pool)
            {
                pool = PoolsManager.Pools.Create(ScenePoolName);
            }

            pool.gameObject.name = ScenePoolName;
        }

        protected void CreateUIRoot()
        {
            uiRoot = FindObjectOfType<UIManager>();
            if (null == UIRoot)
            {
                var obj = (GameObject)Instantiate(VRRes.Load<GameObject>(R.Prefab.UIManager));
                uiRoot = obj.GetComponent<UIManager>();
            }
            if (uiRoot.Canvas.worldCamera == null)
                uiRoot.Canvas.worldCamera = Camera.main;
            uiRoot.name = "UIRoot";
        }

        protected void WidgetsControl(bool attach)
        {
            var gadgets = GameObject.FindObjectsOfType<ISceneWidget>();
            for (int i = 0; i < gadgets.Length; i++)
            {
                if (attach)
                {
                    gadgets[i].OnAttach();
                }
                else
                {
                    gadgets[i].OnDetach();
                }
            }
        }


        public T Get<T>() where T : MonoBehaviour
        {
            if (null == lastGot || lastGot.GetType() != typeof(T))
            {
                lastGot = UnityObject.GetComponent<T>();
            }
            return (T)lastGot;
        }

        protected abstract void OnSceneReturn();
    }

    public abstract class BaseScene<T, BundleType> : IScene where T : BaseScene<T, BundleType>
    {

        public override void Preload(object bundle)
        {
            base.Preload(bundle);
            OnPreload((BundleType)bundle);
        }

        public override void Unload()
        {
            OnUnload();
            base.Unload();
        }

        public override void Loaded()
        {
            OnLoaded();
            base.Loaded();
        }




        protected abstract void OnPreload(BundleType bundle);
        protected abstract void OnUnload();
        protected abstract void OnLoaded();

        public static void Goto(string scene, BundleType bundle)
        {
            SceneManager.Instance.LoadScene<T, BundleType>(scene, bundle);
        }

        public T2 CreateSceneWidget<T2>() where T2 : ISceneWidget<T>
        {
            var re = UnityObject.AddComponent<T2>();
            return re;
        }
    }

    public abstract class BaseScene<T> : IScene where T : BaseScene<T>
    {

        public T2 CreateSceneWidget<T2>() where T2 : ISceneWidget<T>
        {
            var re = UnityObject.AddComponent<T2>();
            return re;
        }

        public override void Preload(object bundle)
        {
            base.Preload(bundle);
            OnPreload();
        }

        public override void Unload()
        {
            OnUnload();
            base.Unload();
        }

        public override void Loaded()
        {
            OnLoaded();
            base.Loaded();
        }
        protected abstract void OnPreload();
        protected abstract void OnUnload();
        protected abstract void OnLoaded();

        public static void Goto(string scene)
        {
            SceneManager.Instance.LoadScene<T>(scene);
        }
    }
}