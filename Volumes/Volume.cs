using System.Collections.Generic;
using UnityEngine;

namespace Framework.Volumes
{
    public abstract class Volume<TProfile, TManager> : MonoBehaviour
        where TProfile : VolumeProfile
        where TManager : VolumeManager<TProfile, TManager>, new()
    {
        private const string VOLUME_LAYER_NAME = "Volumes";
        
        [SerializeField]
        [Tooltip("The profile for this volume.")]
        private TProfile sharedProfile;

        [Tooltip("The layer used by this volume.")]
        public VolumeLayer layer;

        [Tooltip("A global volume is applied to the whole scene.")]
        public bool isGlobal = false;

        [Tooltip("Volume priority in the stack. Higher number means higher priority. Negative values are supported.")]
        [Range(-10000, 10000)]
        public int priority = 0;

        [Tooltip("Outer distance to start blending from. A value of 0 means no blending and the volume overrides will be applied immediately upon entry.")]
        public float blendDistance = 0f;

        [Tooltip("Total weight of this volume in the scene. 0 means it won't do anything, 1 means full effect.")]
        [Range(0f, 1f)]
        public float weight = 1f;
        
        private VolumeLayer m_previousLayer;
        private float m_previousPriority;

        private readonly List<Collider> m_colliders = new List<Collider>();
        public List<Collider> Colliders => m_colliders;

        private TProfile m_profile;
        public TProfile Profile
        {
            get
            {
                if (m_profile == null)
                {
                    if (sharedProfile != null)
                    {
                        m_profile = sharedProfile;
                    }
                    else
                    {
                        m_profile = ScriptableObject.CreateInstance<TProfile>();
                    }
                }
                return m_profile;
            }
            set
            {
                m_profile = value;
            }
        }

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

        private void Awake()
        {
            if (layer == null)
            {
                Debug.LogWarning($"Volume \"{name}\" does not have a layer assigned. This volume will be ignored.");
            }

            GetComponents(m_colliders);
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
            VolumeManager<TProfile, TManager>.Instance.Register(this, layer);
        }

        private void Deregister()
        {
            VolumeManager<TProfile, TManager>.Instance.Deregister(this, layer);
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            if (layer != m_previousLayer)
            {
                Deregister();
                m_previousLayer = layer;
                Register();
            }

            if (priority != m_previousPriority)
            {
                VolumeManager<TProfile, TManager>.Instance.SetLayerDirty(layer);
                m_previousPriority = priority;
            }
        }

        private void OnDrawGizmos()
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
            
            Gizmos.color = new Color(0f, 1f, 0.1f, 0.25f);

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