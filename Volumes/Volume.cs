using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework.Volumes
{
    public abstract class Volume<TVolume, TManager> : MonoBehaviour
        where TVolume : Volume<TVolume, TManager>
        where TManager : VolumeManager<TVolume, TManager>, new()
    {
        private const string VOLUME_LAYER_NAME = "Volumes";
        
        private static TManager m_manager = typeof(TManager).BaseType.GetField("m_instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as TManager;
        
        [SerializeField]
        [Tooltip("The layer used by this volume.")]
        private VolumeLayer m_layer;

        [SerializeField]
        [Tooltip("A global volume is applied to the whole scene.")]
        public bool isGlobal = false;

        [SerializeField]
        [Tooltip("Volume priority in the stack. Higher number means higher priority. Negative values are supported.")]
        [Range(-10000, 10000)]
        private int m_priority = 0;

        [Tooltip("Outer distance to start blending from. A value of 0 means no blending and the volume overrides will be applied immediately upon entry.")]
        public float blendDistance = 0f;

        [Tooltip("Total weight of this volume in the scene. 0 means it won't do anything, 1 means full effect.")]
        [Range(0f, 1f)]
        public float weight = 1f;

        private readonly List<Collider> m_colliders = new List<Collider>();
        public List<Collider> Colliders => m_colliders;

        public int Priority => m_priority;
        public VolumeLayer Layer => m_layer;
        
        private void Reset()
        {
            int volumeLayer = LayerMask.NameToLayer(VOLUME_LAYER_NAME);
            if (volumeLayer != -1)
            {
                gameObject.layer = volumeLayer;
            }
            else
            {
                Debug.LogWarning($"Layer \"{VOLUME_LAYER_NAME}\" is not defined. Create it and set disable physics interactions on that layer.");
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

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            Deregister();
        }

        private void Register()
        {
            m_manager.Register(this as TVolume, m_layer);
        }

        private void Deregister()
        {
            m_manager.Deregister(this as TVolume, m_layer);
        }

#if UNITY_EDITOR
        private VolumeLayer m_previousLayer;
        private float m_previousPriority;

        protected abstract Color Color { get; }
        
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

        protected void DrawGizmos()
        {
            if (isGlobal)
            {
                return;
            }
            
            m_colliders.Clear();
            GetComponents(m_colliders);

            Vector3 scale = transform.localScale;
            Vector3 invScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);

            Color color = Color;
            color.a = 0.25f;
            Gizmos.color = color;

            // Draw a separate gizmo for each collider
            foreach (Collider collider in m_colliders)
            {
                if (!collider.enabled)
                {
                    continue;
                }
                
                if (collider is BoxCollider)
                {
                    BoxCollider c = collider as BoxCollider;
                    Gizmos.DrawCube(c.center, c.size);
                    Gizmos.DrawWireCube(c.center, c.size + (invScale * blendDistance * 2f));
                }
                else if (collider is SphereCollider)
                {
                    SphereCollider c = collider as SphereCollider;
                    Gizmos.DrawSphere(c.center, c.radius);
                    Gizmos.DrawWireSphere(c.center, c.radius + (invScale.x * blendDistance));
                }
                else if (collider is MeshCollider)
                {
                    MeshCollider c = collider as MeshCollider;

                    // Only convex mesh colliders are allowed
                    if (!c.convex)
                    {
                        c.convex = true;
                    }

                    // Mesh pivot should be centered or this won't work
                    Gizmos.DrawMesh(c.sharedMesh);
                    Gizmos.DrawWireMesh(c.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one + (invScale * blendDistance * 2f));
                }
            }
        }
#endif
    }
}