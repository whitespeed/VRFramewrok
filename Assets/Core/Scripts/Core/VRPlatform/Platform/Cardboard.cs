using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

#if Cardboard
namespace Framework
{
    public class CardboardViewer : IVRViewer
    {
        public class ViewerMonoBehaviour : MonoBehaviour
        {
            public Action UpdateHandler = null;
            void Update()
            {
                if (null != UpdateHandler)
                    UpdateHandler();
            }
        }

        protected VRReticle reticle;
        public Transform Head
        {
            get
            {
                return Camera.main.transform;
            }
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAppLoad()
        {
            VRSplash.DelaySeconds = 3.5f;
            var t = GameObject.FindObjectOfType<VRSplash>().transform;
            t.DOLocalMoveZ(180f, 1.5f).SetDelay(2f);
        }
        public CardboardViewer()
        {
            //VRApplication.IsDebug = true;
            var gvrViewer = GameObject.FindObjectOfType<GvrViewer>();
            GameObject.DontDestroyOnLoad(gvrViewer);
            gvrViewer.name = "VRViewer";
            gvrViewer.gameObject.AddComponent<ViewerMonoBehaviour>().UpdateHandler = Update;
            Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
            AddVRInputModule();
            AddVRReticle();

        }

        void AddVRInputModule()
        {
            var gaze = GameObject.FindObjectOfType<GazeInputModule>();
            if(null != gaze)
                GameObject.Destroy(gaze);
            var obj = GameObject.FindObjectOfType<EventSystem>();
            if (null == obj)
            {
                GameObject e = new GameObject("EventSystem");
                obj = e.AddComponent<EventSystem>();
            }
            GameObject.DontDestroyOnLoad(obj.gameObject);
            var inputModule = obj.GetComponentInChildren<VRInputModule>(true);
            if (inputModule == null)
            {
                inputModule= obj.gameObject.AddComponent<VRInputModule>();
            }
            VRInputModule.AutoGazeClick = false;
        }

        void AddVRReticle()
        {
            reticle = Camera.main.GetComponentInChildren<VRReticle>(true);
            if (null == reticle)
            {
                reticle = UITools.AddChild<VRReticle>(Camera.main.gameObject, VRRes.Load<GameObject>(R.Prefab.VRReticle));
            }
        }
        void Update()
        {
            OnShowRecenterTarget(RecenterObj);
        }


        void OnShowRecenterTarget(GameObject recenterObj)
        {
            if (recenterObj)
            {
                Vector3 posEuler = GvrViewer.Instance.HeadPose.Orientation.eulerAngles;
                recenterObj.SetActive(posEuler.x > 300 && posEuler.x < 340);
                Vector3 Angle = recenterObj.transform.eulerAngles;
                Angle.y = posEuler.y;
                recenterObj.transform.eulerAngles = Angle;
            }
        }

        public GameObject RecenterObj
        {
            get; protected set;
        }

        public bool Enable
        {
            get
            {
                return GvrViewer.Instance.VRModeEnabled;
            }
            set
            {
                GvrViewer.Instance.VRModeEnabled = value;
            }
        }

        public int EyeTextureWidth
        {
            get
            {
                return Screen.width;
            }
        }

        public int EyeTextureHeight
        {
            get
            {
                return Screen.height;
            }
        }

        public Camera Right
        {
            get
            {
                if (GvrViewer.Controller.Eyes[0].eye == GvrViewer.Eye.Right)
                {
                    return GvrViewer.Controller.Eyes[1].cam;
                }
                else
                {
                    return GvrViewer.Controller.Eyes[0].cam;
                }
            }
        }

        public Camera Left
        {
            get
            {
                if (GvrViewer.Controller.Eyes[0].eye == GvrViewer.Eye.Left)
                {
                    return GvrViewer.Controller.Eyes[1].cam;
                }
                else
                {
                    return GvrViewer.Controller.Eyes[0].cam;
                }
            }
        }

        public GameObject Reticle
        {
            get { return reticle.gameObject; }
        }


        public void Recent()
        {
            GvrViewer.Instance.Recenter();
        }

        public void Destroy()
        {

        }

        public void Quit()
        {
            //throw new NotImplementedException();
        }

        public void BackToLauncher()
        {
            //throw new NotImplementedException();
            //PluginManager.Plugin.BackToLauncher();
            PluginManager.Plugin.ApplicationQuit();
        }

        public void ForceQuit()
        {
            //throw new NotImplementedException();
            PluginManager.Plugin.ApplicationQuit();
        }
    }

    public class CardboardController : IVRController
    {

        public class UpdateHandler : MonoBehaviour
        {
            public event Action UpdateEvent = null;

            void Update()
            {
                if (null != UpdateEvent)
                    UpdateEvent();
            }
        }

        public CardboardController()
        {
            var o = GameObject.FindObjectOfType<VRManager>();
            if (o)
            {
                var handler = o.gameObject.AddComponent<UpdateHandler>();
                handler.UpdateEvent += () =>
                {
                    if (Input.GetKeyUp(KeyCode.Escape))
                    {
                        if (null != OnBackButton)
                            OnBackButton();
                    }

                };
            }
            else
            {
                Debug.LogError("VRManager 未找到");
            }
        }

        public event Action OnBackButton = null;


        public bool ClickBtnDown
        {
            get
            {
                return Input.GetMouseButtonDown(0);
            }
        }

        public bool ClickBtn
        {
            get
            {
                return Input.GetMouseButton(0);
            }
        }

        public bool ClickBtnUp
        {
            get
            {
                return Input.GetMouseButtonUp(0);
            }
        }


        public Vector2 TouchPos { get { return Input.mousePosition; } }

        bool IVRController.IsTouching
        {
            get
            {
                return !Input.GetMouseButtonDown(0) && Input.GetMouseButton(0);
            }
        }

        public void OnClickBack()
        {
            //throw new NotImplementedException();
            if (OnBackButton != null)
                OnBackButton();
        }

        public bool ClickOnce()
        {
            return false;
        }
    }

}

#endif


