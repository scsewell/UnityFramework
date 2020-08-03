using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Framework.Volumes
{
    /// <summary>
    /// A class that defines a volume that can have properties that can be blended between.
    /// </summary>
    /// <typeparam name="TVolume">The concrete type inheriting from this class.</typeparam>
    /// <typeparam name="TManager">The concrete volume manager type for this type of volume.</typeparam>
    public abstract class Volume<TVolume, TManager> : MonoBehaviour
        where TVolume : Volume<TVolume, TManager>
        where TManager : VolumeManager<TVolume, TManager>, new()
    {
        private const string VOLUME_LAYER_NAME = "Volumes";

        private static TManager m_manager = typeof(TManager).BaseType
            .GetField("m_instance", BindingFlags.NonPublic | BindingFlags.Static)
            .GetValue(null) as TManager;

        [SerializeField]
        [Tooltip("The layer affected by this volume.")]
        private VolumeLayer m_layer = null;

        [SerializeField]
        [Tooltip("A global volume is applied to the whole scene and ignores colliders.")]
        private bool m_isGlobal = false;

        [SerializeField]
        [Tooltip("The priority of this volume in the stack. " +
            "Volumes with greater priorities override volume with lesser priorities. " +
            "Negative values are supported.")]
        [Range(-10000, 10000)]
        private int m_priority = 0;

        [Tooltip("The strength of this volume in the scene. " +
            "Volumes with a weight of 0 have no effect, while a weight of 1 means full effect.")]
        [Range(0f, 1f)]
        private float m_weight = 1f;

        [Tooltip("The distance from the volume to start blending in at.")]
        private float m_blendDistance = 0f;

        private readonly List<Collider> m_colliders = new List<Collider>();

        /// <summary>
        /// The layer affected by this volume.
        /// </summary>
        public VolumeLayer Layer => m_layer;

        /// <summary>
        /// A global volume is applied to the whole scene and ignores colliders.
        /// </summary>
        public bool IsGlobal
        {
            get => m_isGlobal;
            set => m_isGlobal = value;
        }

        /// <summary>
        /// The priority of this volume in the stack.
        /// </summary>
        /// <remarks>
        /// Volumes with greater priorities override volume with lesser priorities.
        /// Negative prorities are supported.
        /// </remarks>
        public int Priority
        {
            get => m_priority;
            set => m_priority = value;
        }

        /// <summary>
        /// The strength of this volume in the scene. Volumes with a weight of 0 have no
        /// effect, while a weight of 1 means full effect.
        /// </summary>
        public float Weight
        {
            get => m_weight;
            set => m_weight = Mathf.Clamp01(value);
        }

        /// <summary>
        /// The distance from the volume to start blending in at.
        /// </summary>
        public float BlendDistance
        {
            get => m_blendDistance;
            set => m_blendDistance = Mathf.Max(value, 0);
        }

        /// <summary>
        /// The colliders whose union defines the volume shape.
        /// </summary>
        public IReadOnlyList<Collider> Colliders => m_colliders;

        /// <summary>
        /// The color of the volume in the editor scene view.
        /// </summary>
        protected abstract Color GizmoColor { get; }

#if UNITY_EDITOR
        private VolumeLayer m_previousLayer;
        private float m_previousPriority;
#endif

        protected virtual void Reset()
        {
            var layer = LayerMask.NameToLayer(VOLUME_LAYER_NAME);

            if (layer != -1)
            {
                foreach (var t in GetComponentsInChildren<Transform>(true))
                {
                    t.gameObject.layer = layer;
                }
            }
            else
            {
                Debug.LogWarning($"Layer \"{VOLUME_LAYER_NAME}\" is not defined. Create it and disable all physics interactions for that layer.");
            }
        }

        protected virtual void Awake()
        {
            if (m_layer == null)
            {
                Debug.LogWarning($"Volume \"{name}\" does not have a layer assigned. This volume will be ignored.");
            }

            GetComponents(m_colliders);

#if UNITY_EDITOR
            m_previousLayer = m_layer;
            m_previousPriority = m_priority;
#endif
        }

        protected virtual void OnEnable()
        {
            Register();
        }

        protected virtual void OnDisable()
        {
            Deregister();
        }

        private void Register()
        {
            m_manager.Register(this as TVolume);
        }

        private void Deregister()
        {
            m_manager.Deregister(this as TVolume);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (m_layer != m_previousLayer)
            {
                Deregister();
                m_previousLayer = m_layer;
                Register();
            }

            if (m_priority != m_previousPriority)
            {
                m_manager.SetLayerDirty(m_layer);
                m_previousPriority = m_priority;
            }
        }

        private void OnDrawGizmos()
        {
            if (m_isGlobal)
            {
                return;
            }

            var scale = transform.localScale;
            var invScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);

            var color = GizmoColor;
            color.a = 0.25f;
            Gizmos.color = color;

            m_colliders.Clear();
            GetComponents(m_colliders);

            // Draw a separate gizmo for each collider
            foreach (var collider in Colliders)
            {
                if (!collider.enabled)
                {
                    continue;
                }

                if (collider is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.DrawWireCube(box.center, box.size + (2f * invScale * m_blendDistance));
                }
                else if (collider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius + (invScale.x * m_blendDistance));
                }
                else if (collider is MeshCollider mesh)
                {
                    // Only convex mesh colliders work for volumes
                    if (!mesh.convex)
                    {
                        continue;
                    }

                    // Mesh pivot should be centered or this won't work
                    Gizmos.DrawMesh(mesh.sharedMesh);
                    Gizmos.DrawWireMesh(mesh.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one + (2f * invScale * m_blendDistance));
                }
            }
        }
#endif
    }
}