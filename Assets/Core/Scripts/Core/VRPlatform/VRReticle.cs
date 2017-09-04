using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Framework
{
    [RequireComponent(typeof(Renderer))]
    public class VRReticle : MonoBehaviour, IGazeHandler
    {
        /// Number of segments making the reticle circle.
        public int reticleSegments = 20;
        /// Growth speed multiplier for the reticle/
        public float reticleGrowthSpeed = 8.0f;

        // Private members
        private Material materialComp;
        private Renderer MeshRender;

        // Current inner angle of the reticle (in degrees).
        private float reticleInnerAngle = 0.0f;
        // Current outer angle of the reticle (in degrees).
        private float reticleOuterAngle = 0.5f;
        // Current distance of the reticle (in meters).
        private float reticleDistanceInMeters = 10.0f;

        // Minimum inner angle of the reticle (in degrees).
        private const float kReticleMinInnerAngle = 0.0f;
        // Minimum outer angle of the reticle (in degrees).
        private const float kReticleMinOuterAngle = 0.5f;
        // Angle at which to expand the reticle when intersecting with an object
        // (in degrees).
        private const float kReticleGrowthAngle = 1.5f;

        // Minimum distance of the reticle (in meters).
        private const float kReticleDistanceMin = 0.05f;

        // Maximum distance of the reticle (in meters).
        private const float kReticleDistanceMax = 15.0f;

        // Current inner and outer diameters of the reticle,
        // before distance multiplication.
        private float reticleInnerDiameter = 0.0f;
        private float reticleOuterDiameter = 0.0f;

        private Mesh mesh = null;
        private MeshFilter meshFilter = null;
        private float gazeStart = 0;
        void Start()
        {
            CreateReticleVertices();

            MeshRender = gameObject.GetComponent<Renderer>();
            MeshRender.sortingLayerName = VRSortingLayer.GlobalUI;
            materialComp = MeshRender.material;
        }

        void OnEnable()
        {
            VRInputModule.GazeHandler = this;
        }

        void OnDisable()
        {
            VRInputModule.GazeHandler = null;
        }

        void Update()
        {
            UpdateDiameters();
        }

        public void OnGazeEnabled()
        {
            if (MeshRender)
                MeshRender.enabled = true;
        }

        public void OnGazeDisabled()
        {
            if (MeshRender)
                MeshRender.enabled = false;
        }

        public void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                                bool isInteractive)
        {
            SetGazeTarget(intersectionPosition, isInteractive);
            if (VRInputModule.AutoGazeClick)
            {
                gazeStart = Time.time;
                UpdateProgress(1);
            }
        }


        public bool OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                               bool isInteractive)
        {
            SetGazeTarget(intersectionPosition, isInteractive);
            if (VRInputModule.AutoGazeClick && isInteractive && ExecuteEvents.GetEventHandler<IPointerClickHandler>(targetObject))
            {
                var stayTime = Time.time - gazeStart;
                if (stayTime >= VRInputModule.GazeIdleTime)
                {
                    var progress = (stayTime - VRInputModule.GazeIdleTime) / VRInputModule.GazeClickTime;
                    UpdateProgress(progress);
                    if (progress >= 1)
                        gazeStart = Time.time;
                }
                else
                {
                    UpdateProgress(1);
                }
            }
            else
            {
                UpdateProgress(1);
            }
            return false;
        }

        /// Called when the user's look no longer intersects an object previously
        /// intersected with a ray projected from the camera.
        /// This is also called just before **OnGazeDisabled** and may have have any of
        /// the values set as **null**.
        ///
        /// The camera is the event camera and the target is the object the user
        /// previously looked at.
        public void OnGazeExit(Camera camera, GameObject targetObject)
        {
            if (VRInputModule.AutoGazeClick)
            {
                UpdateProgress(1);
            }
            reticleDistanceInMeters = kReticleDistanceMax;
            reticleInnerAngle = kReticleMinInnerAngle;
            reticleOuterAngle = kReticleMinOuterAngle;
        }

        /// Called when a trigger event is initiated. This is practically when
        /// the user begins pressing the trigger.
        public void OnGazeTriggerStart(Camera camera)
        {
            // Put your reticle trigger start logic here :)
            if (VRInputModule.AutoGazeClick)
            {
                UpdateProgress(1);
            }
            reticleDistanceInMeters = kReticleDistanceMax;
            reticleInnerAngle = kReticleMinInnerAngle;
            reticleOuterAngle = kReticleMinOuterAngle;

        }

        /// Called when a trigger event is finished. This is practically when
        /// the user releases the trigger.
        public void OnGazeTriggerEnd(Camera camera)
        {
            // Put your reticle trigger end logic here :)
            if (VRInputModule.AutoGazeClick)
            {
                UpdateProgress(1);
            }
            reticleDistanceInMeters = kReticleDistanceMax;
            reticleInnerAngle = kReticleMinInnerAngle;
            reticleOuterAngle = kReticleMinOuterAngle;

        }

        public void GetPointerRadius(out float innerRadius, out float outerRadius)
        {
            float min_inner_angle_radians = Mathf.Deg2Rad * kReticleMinInnerAngle;
            float max_inner_angle_radians = Mathf.Deg2Rad * (kReticleMinInnerAngle + kReticleGrowthAngle);

            innerRadius = 2.0f * Mathf.Tan(min_inner_angle_radians);
            outerRadius = 2.0f * Mathf.Tan(max_inner_angle_radians);
        }
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> indices = new List<int>();
        private readonly List<int> progressIndices = new List<int>();
        private int lastIndicesCount = 0;
        private void CreateReticleVertices()
        {
            mesh = new Mesh();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            int segments_count = reticleSegments;
            int vertex_count = (segments_count + 1) * 2;

            #region Vertices

            vertices.Clear();
            vertices.AddRange(new Vector3[vertex_count]);

            const float kTwoPi = Mathf.PI * 2.0f;
            int vi = 0;
            for (int si = 0; si <= segments_count; ++si)
            {
                // Add two vertices for every circle segment: one at the beginning of the
                // prism, and one at the end of the prism.
                float angle = (float)si / (float)(segments_count) * kTwoPi;

                float x = Mathf.Sin(angle);
                float y = Mathf.Cos(angle);

                vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
                vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
            }
            #endregion

            #region Triangles
            int indices_count = (segments_count + 1) * 3 * 2;
            indices.Clear();
            indices.AddRange(new int[indices_count]);


            int vert = 0;
            int idx = 0;
            for (int si = 0; si < segments_count; ++si)
            {
                indices[idx++] = vert + 1;
                indices[idx++] = vert;
                indices[idx++] = vert + 2;

                indices[idx++] = vert + 1;
                indices[idx++] = vert + 2;
                indices[idx++] = vert + 3;

                vert += 2;
            }
            #endregion
            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
        }

        private void UpdateProgress(float fProgress)
        {
            fProgress = Mathf.Clamp01(fProgress);

            int segments_count = reticleSegments;
            segments_count = (int)(segments_count * fProgress);
            int indices_count = (segments_count + 1) * 3 * 2;
            if (indices_count == lastIndicesCount)
                return;
            lastIndicesCount = indices_count;
            progressIndices.Clear();
            for (int i = 0; i < indices_count && i < indices.Count; i++)
            {
                progressIndices.Add(indices[i]);
            }
            mesh.SetTriangles(progressIndices, 0);
        }
        private void UpdateDiameters()
        {
            reticleDistanceInMeters =
              Mathf.Clamp(reticleDistanceInMeters, kReticleDistanceMin, kReticleDistanceMax);

            if (reticleInnerAngle < kReticleMinInnerAngle)
            {
                reticleInnerAngle = kReticleMinInnerAngle;
            }

            if (reticleOuterAngle < kReticleMinOuterAngle)
            {
                reticleOuterAngle = kReticleMinOuterAngle;
            }

            float inner_half_angle_radians = Mathf.Deg2Rad * reticleInnerAngle * 0.5f;
            float outer_half_angle_radians = Mathf.Deg2Rad * reticleOuterAngle * 0.5f;

            float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
            float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

            reticleInnerDiameter =
                Mathf.Lerp(reticleInnerDiameter, inner_diameter, Time.deltaTime * reticleGrowthSpeed);
            reticleOuterDiameter =
                Mathf.Lerp(reticleOuterDiameter, outer_diameter, Time.deltaTime * reticleGrowthSpeed);

            materialComp.SetFloat("_InnerDiameter", reticleInnerDiameter * reticleDistanceInMeters);
            materialComp.SetFloat("_OuterDiameter", reticleOuterDiameter * reticleDistanceInMeters);
            materialComp.SetFloat("_DistanceInMeters", reticleDistanceInMeters);
        }

        private void SetGazeTarget(Vector3 target, bool interactive)
        {
            Vector3 targetLocalPosition = transform.InverseTransformPoint(target);

            reticleDistanceInMeters =
                Mathf.Clamp(targetLocalPosition.z, kReticleDistanceMin, kReticleDistanceMax);
            if (interactive)
            {
                reticleInnerAngle = kReticleMinInnerAngle + kReticleGrowthAngle;
                reticleOuterAngle = kReticleMinOuterAngle + kReticleGrowthAngle;
            }
            else
            {
                reticleInnerAngle = kReticleMinInnerAngle;
                reticleOuterAngle = kReticleMinOuterAngle;
            }
        }

        public void OnGazeToggle()
        {
            MeshRender.enabled = !MeshRender.enabled;
        }
    }

}
