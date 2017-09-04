using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework
{
    public class EditorVRViewer : IVRViewer
    {
        private float mouseX;
        private float mouseY;
        private float mouseZ;


        public EditorVRViewer()
        {
            var viewer = UITools.AddChild(null, (GameObject) VRRes.Load<GameObject>(R.Prefab.EditorVRViewer));
            viewer.AddComponent<ViewerMonoBehaviour>().UpdateHandler = Update;
            viewer.name = "VRViewer";
            GameObject.DontDestroyOnLoad(viewer);
            Reticle = viewer.GetComponentInChildren<VRReticle>();
            Head.gameObject.AddComponent<PhysicsRaycaster>();
        }

        public void Destroy()
        {
        }

        public Transform Head
        {
            get { return Camera.main.transform; }
        }

        public int EyeTextureWidth
        {
            get { return Screen.width; }
        }

        public int EyeTextureHeight
        {
            get { return Screen.height; }
        }

        public Camera Right
        {
            get { return Camera.main; }
        }

        public Camera Left
        {
            get { return Camera.main; }
        }

        public VRReticle Reticle { get; protected set; }

        public void Recent()
        {
            mouseX = mouseZ = 0f;
            var el = Head.eulerAngles;
            el.y = 0f;
            Head.eulerAngles = el;
        }

        public void BackToLauncher()
        {
            ForceQuit();
        }

        public void ForceQuit()
        {
            PluginManager.Plugin.ApplicationQuit();
        }

        private void Update()
        {
            ControlMainCamera(Head);
        }

        private void ControlMainCamera(Transform head)
        {
            var rolled = false;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                mouseX += Input.GetAxis("Mouse X")*5f;
                if (mouseX <= -180)
                {
                    mouseX += 360;
                }
                else if (mouseX > 180)
                {
                    mouseX -= 360;
                }
                mouseY -= Input.GetAxis("Mouse Y")*2.4f;
                mouseY = Mathf.Clamp(mouseY, -85, 85);
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                rolled = true;
                mouseZ += Input.GetAxis("Mouse X")*5;
                mouseZ = Mathf.Clamp(mouseZ, -85, 85);
            }
            if (!rolled)
            {
                // People don't usually leave their heads tilted to one side for long.
                mouseZ = Mathf.Lerp(mouseZ, 0, Time.deltaTime/(Time.deltaTime + 0.1f));
            }
            var rot = Quaternion.Euler(mouseY, mouseX, mouseZ);
            head.localRotation = Quaternion.Lerp(head.localRotation, rot, Time.deltaTime*100);
        }


        private void OnShowRecenterTarget(GameObject recenterObj, Transform head)
        {
            if (recenterObj)
            {
                recenterObj.SetActive(Head.transform.eulerAngles.x > 300 && Head.transform.eulerAngles.x < 340);
                var Angle = recenterObj.transform.eulerAngles;
                Angle.y = head.eulerAngles.y;
                recenterObj.transform.eulerAngles = Angle;
            }
        }

        public class ViewerMonoBehaviour : MonoBehaviour
        {
            public Action UpdateHandler;

            private void Update()
            {
                if (null != UpdateHandler)
                    UpdateHandler();
            }
        }
    }

    public class EditorVRController : IVRController
    {
        public EditorVRController()
        {
            var handler = VRManager.Instance.gameObject.AddComponent<UpdateHandler>();
            handler.UpdateEvent += () =>
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    if (null != OnBackButton)
                        OnBackButton();
                }
            };
        }

        public event Action OnBackButton;

        public bool ClickBtnDown
        {
            get { return Input.GetMouseButtonDown(0); }
        }

        public bool ClickBtn
        {
            get { return Input.GetMouseButton(0); }
        }

        public bool ClickBtnUp
        {
            get { return Input.GetMouseButtonUp(0); }
        }

        public void OnClickBack()
        {
            if (OnBackButton != null)
                OnBackButton();
        }

        public bool ClickOnce()
        {
            return false;
        }

        public bool IsTouching
        {
            get { return Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftControl); }
        }

        public Vector2 TouchPos
        {
            get { return Input.mousePosition; }
        }

        public class UpdateHandler : MonoBehaviour
        {
            public event Action UpdateEvent;

            private void Update()
            {
                if (null != UpdateEvent)
                    UpdateEvent();
            }
        }
    }
}