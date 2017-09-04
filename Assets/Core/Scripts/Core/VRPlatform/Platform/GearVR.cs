using UnityEngine;
using System.Collections;
using System;
using UnityEngine.VR;
#if GearVR
namespace Framework
{
    public class GearVRViewer : IVRViewer
    {
        public const string ViewerGearVR = @"Viewer/ViewerGearVR";
        public GearVRViewer()
        {
            GameObject gvrViewer = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(ViewerGearVR));
            gvrViewer.name = "VRViewer";
            GameObject.DontDestroyOnLoad(gvrViewer);
        }
        public Transform Head
        {
            get
            {
                return Camera.main.transform;
            }
        }
public void Destroy(){}
        public bool Enable
        {
            get
            {
                return VRSettings.enabled;
            }
            set
            {
                VRSettings.enabled = value;
            }
        }

        public int EyeTextureWidth
        {
            get
            {
                return VRSettings.eyeTextureWidth;
            }
        }

        public int EyeTextureHeight
        {
            get
            {
                return VRSettings.eyeTextureHeight;
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
            InputTracking.Recenter();
        }
    }

    public class GearVRController : IVRController
    {
        public event Action OnBackButton;

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
    }


}
#endif