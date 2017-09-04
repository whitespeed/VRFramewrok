using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnitySceneMgr = UnityEngine.SceneManagement.SceneManager;

namespace Framework
{
    public class SceneManager : Singleton<SceneManager>
    {
        public const string Event_OnLoaded = "on_loaded";
        public const string Event_OnUnload = "on_unload";
        protected static IScene curScene;
        protected static bool isLoading;
        protected static float TransitionLength = 0.5f;
        protected ITransition mask = null;
        public static readonly MsgDispather<string> Events = new MsgDispather<string>("SceneManager");

        public static IScene CurScene
        {
            get
            {
                if (curScene == null)
                {
                    curScene = FindObjectOfType<IScene>();
                }
                return curScene;
            }
        }

        public override void OnInitialize()
        {
        }

        public override void OnUninitialize()
        {
            if (mask != null)
            {
                mask.DestroySelf();
                mask = null;
            }
            Events.ClearAll();
            if (null == curScene)
                return;
            curScene.Unload();
            curScene = null;
        }

        public void LoadScene<SceneType, BundleType>(string scenePath, BundleType bundle) where SceneType : BaseScene<SceneType, BundleType>
        {
            if (IsLoading)
            {
                Debug.LogWarning("Scenes is loading, do not load again");
            }
            else
            {
                StartCoroutine(RunLoad<SceneType>(scenePath, bundle));
            }
        }

        public void LoadScene<SceneType>(string scenePath) where SceneType : BaseScene<SceneType>
        {
            if (IsLoading)
            {
                Debug.LogWarning("Scenes is loading, do not load again");
            }
            else
            {
                StartCoroutine(RunLoad<SceneType>(scenePath, null));
            }
        }

        public static bool IsLoading
        {
            get { return isLoading; }
        }

        private IEnumerator RunLoad<T>(string scenePath, object bundle,string transition = TransitionFactory.FadeTransition) where T : IScene
        {
            if (IsLoading)
            {
                Debug.LogWarning("Scenes is loading, do not load again");
                yield break;
            }
           
            isLoading = true;
            var lst = curScene;
            if (lst != null)
            {
                //VRProfiler.StartWatch(string.Format("[LoadingScene] Scene: {0} UnLoad ", lst.Name));
                Events.DispathMsg(Event_OnUnload, lst.Name);
                lst.Unload();
                //VRProfiler.StopWatch();
            }

            //VRProfiler.StartWatch("[LoadingScene] Transition In ");
            curScene = null;
            if(null == mask)
                mask = TransitionFactory.CreateTransition(transition);
            mask.DontDestroyOnLoad();
            SetRaycast(false);
            yield return mask.TransitIn(TransitionLength);
            //VRProfiler.StopWatch();


            //VRProfiler.StartWatch("[LoadingScene] Switch To Scene: Loading ");
            //销毁场景物件
            if (lst != null)
            {
                Destroy(lst.UnityObject);
            }
            //切换至空场景
            UnitySceneMgr.LoadScene(VRScene.Loading);
            //VRProfiler.StopWatch();

            //VRProfiler.StartWatch("[LoadingScene] Unload Unused Assets ");
            //资源回收
            yield return 1;
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();
            //VRProfiler.StopWatch();

            //VRProfiler.StartWatch(string.Format("[LoadingScene] Switch to Scene:{0} ",scenePath));
            //切换下一场景
            yield return UnitySceneMgr.LoadSceneAsync(scenePath);
            SetRaycast(true);
           

            //场景handler
            curScene = GetMissingSceneObj<T>();
            curScene.Name = scenePath;
            curScene.gameObject.name = curScene.GetType().Name;
            //配置场景相关内容
            curScene.name = "[scene] " + curScene.Name;
           // VRProfiler.StopWatch();
            //场景加载完成
            isLoading = false;
            //VRProfiler.StartWatch(string.Format("[LoadingScene] Scene: {0} Preload ", curScene.Name));
            curScene.Preload(bundle);
            //VRProfiler.StopWatch();

            

            //等待场景
            //VRProfiler.StartWatch(string.Format("[LoadingScene] Scene: {0} Wait ", curScene.Name));
            yield return curScene.Wait();
           // VRProfiler.StopWatch();

            //VRProfiler.StartWatch(string.Format("[LoadingScene] Scene: {0} Loaded ", curScene.Name));
            curScene.Loaded();
            Events.DispathMsg(Event_OnLoaded, curScene.Name);
            //VRProfiler.StopWatch();

            //VRProfiler.StartWatch("[LoadingScene] Transition Out ");
            //黑幕打开
            if (null != mask)
            {
                yield return mask.TransitOut(TransitionLength);
                mask.DestroySelf();
                mask = null;
            }
            //VRProfiler.StopWatch();

            yield return 1;

            //加载完成
            curScene.Opened();

        }

        private T GetMissingSceneObj<T>() where T : IScene
        {
            var scene = FindObjectOfType<T>();
            if (scene == null)
            {
                var sceneObj = new GameObject("scene", typeof (T));
                DontDestroyOnLoad(sceneObj);
                scene = sceneObj.GetComponent<T>();
            }
            return scene;
        }

        private void SetRaycast(bool active)
        {
            var input = FindObjectOfType<VRInputModule>();
            if (input != null)
            {
                input.enabled = active;
            }

            if (VRManager.Viewer != null && VRManager.Viewer.Reticle)
            {
                VRManager.Viewer.Reticle.gameObject.SetActive(active);
            }
        }
    }
}