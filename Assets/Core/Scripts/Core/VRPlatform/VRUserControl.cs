using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    [RequireComponent(typeof (Canvas))]
    public class VRUserControl : MonoBehaviour
    {
        public Action resetAction = null;

        [SerializeField] private Button resetButton;

        public Action returnAction = null;

        [SerializeField] private Button returnButton;

        [SerializeField] private GameObject root;

        private void Start()
        {
            if (resetButton)
                resetButton.onClick.AddListener(() => { if (resetAction != null) resetAction(); });
            if (returnButton)
                returnButton.onClick.AddListener(() => { if (returnAction != null) returnAction(); });
        }

        public void SetReturnActive(bool isActive)
        {
            if (returnButton) returnButton.gameObject.SetActive(isActive);
        }

        public void SetResetActive(bool isActive)
        {
            if (resetButton) resetButton.gameObject.SetActive(isActive);
        }

        private void LateUpdate()
        {
            if (VRManager.Viewer == null || VRManager.Viewer.Head == null)
                return;
            var rotationNow = VRManager.Viewer.Head.rotation;
            var angles = rotationNow.eulerAngles;
            if (((angles.x > 323f && angles.x < 345f) || (angles.x > 22f && angles.x < 120f)))
            {
                showRecenter(angles);
                if (Vector3.Angle(VRManager.Viewer.Head.forward, Vector3.forward) > 50)
                {
                    resetButton.gameObject.SetActive(true);
                }
                else
                {
                    resetButton.gameObject.SetActive(false);
                }
            }
            else
            {
                if (root.activeInHierarchy)
                    root.SetActive(false);
            }
        }

        void showRecenter(Vector3 angles)
        {
            var rot = Quaternion.Euler(0, angles.y, 0);
            root.transform.position = VRManager.Viewer.Head.position;
            root.transform.rotation = rot;
            if (!root.activeInHierarchy)
                root.SetActive(true);
        }
        
    }
}