using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework
{
    /// This script provides an implemention of Unity's `BaseInputModule` class, so
    /// that Canvas-based (_uGUI_) UI elements can be selected by looking at them and
    /// pulling the viewer's trigger or touching the screen.
    /// This uses the player's gaze and the trigger as a raycast generator.
    ///
    /// To use, attach to the scene's **EventSystem** object.  Be sure to move it above the
    /// other modules, such as _TouchInputModule_ and _StandaloneInputModule_, in order
    /// for the user's gaze to take priority in the event system.
    ///
    /// Next, set the **Canvas** object's _Render Mode_ to **World Space**, and set its _Event Camera_
    /// to a (mono) camera that is controlled by a GvrHead.  If you'd like gaze to work
    /// with 3D scene objects, add a _PhysicsRaycaster_ to the gazing camera, and add a
    /// component that implements one of the _Event_ interfaces (_EventTrigger_ will work nicely).
    /// The objects must have colliders too.
    ///
    /// GazeInputModule emits the following events: _Enter_, _Exit_, _Down_, _Up_, _Click_, _Select_,
    /// _Deselect_, and _UpdateSelected_.  Scroll, move, and submit/cancel events are not emitted.
    public class VRInputModule : BaseInputModule
    {
        /// Time in seconds between the pointer down and up events sent by a trigger.
        /// Allows time for the UI elements to make their state transitions.
        private const float clickTime = 0.1f; // Based on default time for a button to animate to Pressed.

        public const float GazeIdleTime = 0.8f;
        public const float GazeClickTime = 2f;
        public static bool AutoGazeClick = true;

        /// The IGvrGazePointer which will be responding to gaze events.
        public static IGazeHandler GazeHandler;

        public static IPointerHoverHandler HoverHandler;

        public static IGlobalHandler InputHandler;
        private float gazeTime;

        // Active state
        private bool isActive;
        private Vector2 lastHeadPose;

        private float noGazeTime;

        private PointerEventData pointerData;

        /// @cond
        public override bool ShouldActivateModule()
        {
            var activeState = base.ShouldActivateModule();

            if (activeState != isActive)
            {
                isActive = activeState;

                // Activate gaze pointer
                if (GazeHandler != null)
                {
                    if (isActive)
                    {
                        GazeHandler.OnGazeEnabled();
                    }
                }
            }

            return activeState;
        }

        /// @endcond
        public override void DeactivateModule()
        {
            base.DeactivateModule();
            eventSystem.SetSelectedGameObject(null, GetBaseEventData());
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            return pointerData != null && pointerData.pointerEnter != null;
        }

        public override void Process()
        {
            // Save the previous Game Object
            var gazeObjectPrevious = GetCurrentGameObject();

            CastRayFromGaze();
            UpdateCurrentObject();
            UpdateReticle(gazeObjectPrevious);
            HandleSpecialClick(gazeObjectPrevious);
            //HandleHoveEvent();
            var isTriggered = VRManager.Controller.ClickBtnDown;
            var handlePendingClickRequired = !VRManager.Controller.ClickBtn || VRManager.Controller.ClickBtnUp;

            if (Time.unscaledTime - pointerData.clickTime < clickTime)
            {
                // Delay new events until clickTime has passed.
            }
            else if (!pointerData.eligibleForClick && isTriggered)
            {
                // New trigger action.
                HandleTrigger();
            }
            else if (handlePendingClickRequired)
            {
                // Check if there is a pending click to handle.
                HandlePendingClick();
            }
        }

        private void HandleHoveEvent()
        {
            var gazeObject = GetCurrentGameObject();
            if (null != gazeObject)
            {
                var hover = ExecuteEvents.GetEventHandler<IPointerHoverHandler>(gazeObject);
                ExecuteEvents.Execute(hover, pointerData, VREvent.HoverHandler);
               
            }
        }

        /// @endcond
        private void HandleSpecialClick(GameObject previousGazedObject)
        {
            var gazeObject = GetCurrentGameObject(); // Get the gaze target

            var go = pointerData.pointerCurrentRaycast.gameObject;
            pointerData.pressPosition = pointerData.position;
            pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
            pointerData.rawPointerPress = go;
            pointerData.pointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gazeObject);
            var isInteractive = pointerData.pointerPress != null;
            if (VRManager.Controller.ClickOnce() && isInteractive)
            {
                ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
                if (InputHandler != null)
                {
                    InputHandler.OnGlobalClick(pointerData);
                }
            }
            if (AutoGazeClick && gazeObject != null && previousGazedObject != null &&
                (gazeObject == previousGazedObject))
            {
                gazeTime += Time.deltaTime;
                if (gazeTime >= GazeClickTime + GazeIdleTime && isInteractive)
                {
                    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
                    if (InputHandler != null)
                    {
                        InputHandler.OnGlobalClick(pointerData);
                    }
                    gazeTime = 0f;
                }
            }
            else
            {
                gazeTime = 0f;
            }
        }

        private void CastRayFromGaze()
        {
            var headOrientation = VRManager.Viewer.Head.rotation;

            var headPose = NormalizedCartesianToSpherical(headOrientation*Vector3.forward);

            if (pointerData == null)
            {
                pointerData = new PointerEventData(eventSystem);
                lastHeadPose = headPose;
            }

            // Cast a ray into the scene
            pointerData.Reset();
            pointerData.position = GetGazePointerPosition();
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            pointerData.delta = headPose - lastHeadPose;
            lastHeadPose = headPose;
        }

        private void UpdateCurrentObject()
        {
            // Send enter events and update the highlight.
            var go = pointerData.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerData, go);
            // Update the current selection, or clear if it is no longer the current object.
            //var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
            if (go)
            {
                if (go == eventSystem.currentSelectedGameObject)
                {
                    ExecuteEvents.Execute(go, pointerData, VREvent.HoverHandler);
                }
                eventSystem.SetSelectedGameObject(go, pointerData);
            }
            else
            {
                eventSystem.SetSelectedGameObject(null, pointerData);
            }
        }

        private void UpdateReticle(GameObject previousGazedObject)
        {
            if (GazeHandler == null)
            {
                return;
            }

            var eventCam = pointerData.enterEventCamera; // Get the camera
            var gazeObject = GetCurrentGameObject(); // Get the gaze target
            var intersectionPosition = GetIntersectionPosition();

            //hover 事件
            if (null != HoverHandler)
            {
                HoverHandler.OnHover(pointerData);
            }

            //reticle auto disable
            if (gazeObject == null)
            {
                noGazeTime += Time.deltaTime;
                if (noGazeTime > 5f)
                {
                    GazeHandler.OnGazeDisabled();
                }
            }
            else
            {
                noGazeTime = 0;
                GazeHandler.OnGazeEnabled();
            }


            var isInteractive = pointerData.pointerPress != null ||
                                ExecuteEvents.GetEventHandler<IEventSystemHandler>(gazeObject) != null;


            if (gazeObject != null && previousGazedObject != null && (gazeObject == previousGazedObject))
            {
                GazeHandler.OnGazeStay(eventCam, gazeObject, intersectionPosition, isInteractive);
            }
            else
            {
                if (previousGazedObject != null)
                {
                    GazeHandler.OnGazeExit(eventCam, previousGazedObject);
                }

                if (gazeObject != null)
                {
                    GazeHandler.OnGazeStart(eventCam, gazeObject, intersectionPosition, isInteractive);
                }
            }
        }

        private void HandlePendingClick()
        {
            if (!pointerData.eligibleForClick && !pointerData.dragging)
            {
                return;
            }

            if (GazeHandler != null)
            {
                var c = pointerData.enterEventCamera;
                GazeHandler.OnGazeTriggerEnd(c);
            }

            var go = pointerData.pointerCurrentRaycast.gameObject;

            // Send pointer up and click events.
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
            if (pointerData.eligibleForClick)
            {
                ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
                if (InputHandler != null)
                {
                    InputHandler.OnGlobalClick(pointerData);
                }
            }



            // Clear the click state.
            pointerData.pointerPress = null;
            pointerData.rawPointerPress = null;
            pointerData.eligibleForClick = false;
            pointerData.clickCount = 0;
            pointerData.clickTime = 0;
            pointerData.pointerDrag = null;
            pointerData.dragging = false;
        }

        private void HandleTrigger()
        {
            var go = pointerData.pointerCurrentRaycast.gameObject;

            // Send pointer down event.
            pointerData.pressPosition = pointerData.position;
            pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
            pointerData.pointerPress =
                ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
                ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);


            // Save the pending click state.
            pointerData.rawPointerPress = go;
            pointerData.eligibleForClick = true;
            pointerData.delta = Vector2.zero;
            pointerData.dragging = false;
            pointerData.useDragThreshold = true;
            pointerData.clickCount = 1;
            pointerData.clickTime = Time.unscaledTime;

            if (GazeHandler != null)
            {
                GazeHandler.OnGazeTriggerStart(pointerData.enterEventCamera);
            }
        }

        private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords)
        {
            cartCoords.Normalize();
            if (Math.Abs(cartCoords.x) < Mathf.Epsilon)
                cartCoords.x = Mathf.Epsilon;
            var outPolar = Mathf.Atan(cartCoords.z/cartCoords.x);
            if (cartCoords.x < 0)
                outPolar += Mathf.PI;
            var outElevation = Mathf.Asin(cartCoords.y);
            return new Vector2(outPolar, outElevation);
        }

        private GameObject GetCurrentGameObject()
        {
            if (pointerData != null && pointerData.enterEventCamera != null)
            {
                return pointerData.pointerCurrentRaycast.gameObject;
            }

            return null;
        }

        private Vector3 GetIntersectionPosition()
        {
            // Check for camera
            var cam = pointerData.enterEventCamera;
            if (cam == null)
            {
                return Vector3.zero;
            }
            var intersectionDistance = pointerData.pointerCurrentRaycast.distance + cam.nearClipPlane;
            var intersectionPosition = cam.transform.position + cam.transform.forward*intersectionDistance;

            return intersectionPosition;
        }

        private void DisableGazePointer()
        {
            if (GazeHandler == null)
            {
                return;
            }

            var currentGameObject = GetCurrentGameObject();
            if (currentGameObject)
            {
                var camera = pointerData.enterEventCamera;
                GazeHandler.OnGazeExit(camera, currentGameObject);
            }

            GazeHandler.OnGazeDisabled();
        }

        private Vector2 GetGazePointerPosition()
        {
            var viewportWidth = VRManager.Viewer.EyeTextureWidth;
            var viewportHeight = VRManager.Viewer.EyeTextureHeight;

            return new Vector2(0.5f*viewportWidth, 0.5f*viewportHeight);
        }

        public override string ToString()
        {
            var sb = new StringBuilder("<b>Pointer Input Module of type: </b>" + GetType());
            sb.AppendLine();
            if (pointerData != null)
                sb.AppendLine(pointerData.ToString());
            else
                sb.AppendLine("pointerData = null .");
            return sb.ToString();
        }
    }

    public interface IGazeHandler
    {
        void OnGazeEnabled();

        void OnGazeDisabled();

        void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
            bool isInteractive);

        bool OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
            bool isInteractive);

        void OnGazeExit(Camera camera, GameObject targetObject);

        void OnGazeTriggerStart(Camera camera);

        void OnGazeTriggerEnd(Camera camera);

        void GetPointerRadius(out float innerRadius, out float outerRadius);

        void OnGazeToggle();
    }


    public abstract class IGlobalHandler: MonoBehaviour
    {
        public abstract void OnGlobalClick(PointerEventData eventData);
    }
}