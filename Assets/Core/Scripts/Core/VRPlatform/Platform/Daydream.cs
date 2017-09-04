using UnityEngine;
using System.Collections;
using System;
using UnityEngine.VR;
#if Daydream
namespace Framework
{
    public class DayDreamViewer : IVRViewer
    {
        public const string ViewerDaydream = @"Viewer\GearVR";
        public Transform Head
        {
            get
            {
                return Camera.main.transform;
            }
        }

        public DayDreamViewer()
        {
            GameObject gvrViewer = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(ViewerDaydream));
            gvrViewer.name = "VRViewer";
            GameObject.DontDestroyOnLoad(gvrViewer);
        }


        public bool Enable
        {
            get
            {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
                return VRSettings.enabled;
#else
           return GvrViewer.Instance.VRModeEnabled;
#endif
            }
            set
            {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
                VRSettings.enabled = value;
#else
           GvrViewer.Instance.VRModeEnabled = value;
#endif
            }
        }

        public int EyeTextureWidth
        {
            get
            {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
                return VRSettings.eyeTextureWidth;
#else
            return Screen.width;
#endif
            }
        }
public void Destroy(){}
        public int EyeTextureHeight
        {
            get
            {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
                return VRSettings.eyeTextureHeight;
#else
            return Screen.height;
#endif
            }
        }

        public Camera Right
        {
            get
            {
                return null;
            }
        }

        public Camera Left
        {
            get
            {
                return null;
            }
        }

        public VRReticle Reticle
        {
            get
            {
                return null;
            }
        }

        public void Recent()
        {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
            InputTracking.Recenter();
#else
        GvrViewer.Instance.Recenter();
#endif
        }
    }

    public class DayDreamController : IVRController
    {
        public event Action OnBackButton
        {
            add
            {
            }
            remove
            {

            }
        }


        public bool ClickBtnDown
        {
            get
            {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
                return GvrController.ClickButtonDown;
#else
            return Input.GetMouseButtonDown(0);
#endif
            }
        }

        public bool ClickBtn
        {
            get
            {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
                return GvrController.ClickButton;
#else
            return Input.GetMouseButton(0);
#endif
            }
        }

        public bool ClickBtnUp
        {
            get
            {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
                return GvrController.ClickButtonUp;
#else
            return Input.GetMouseButtonUp(0);
#endif
            }
        }
    }

}
#endif