using System.Collections.Generic;
using UnityEngine;

namespace Framework.Volumes
{
    public abstract class VolumeManager<TVolume, TManager> 
        where TVolume : Volume<TVolume, TManager>
        where TManager : VolumeManager<TVolume, TManager>, new()
    {
        private static readonly TManager m_instance = new TManager();
        public static TManager Instance => m_instance;
        
        private readonly Dictionary<VolumeLayer, List<TVolume>> m_volumes = new Dictionary<VolumeLayer, List<TVolume>>();
        private readonly Dictionary<VolumeLayer, bool> m_sortNeeded = new Dictionary<VolumeLayer, bool>();
        
        public void Register(TVolume volume, VolumeLayer layer)
        {
            if (layer != null)
            {
                List<TVolume> volumes;
                if (!m_volumes.TryGetValue(layer, out volumes))
                {
                    volumes = new List<TVolume>();
                    m_volumes.Add(layer, volumes);
                }
                volumes.Add(volume);
                SetLayerDirty(layer);
            }
        }

        public void Deregister(TVolume volume, VolumeLayer layer)
        {
            if (layer != null)
            {
                List<TVolume> volumes;
                if (m_volumes.TryGetValue(layer, out volumes))
                {
                    volumes.Remove(volume);
                }
            }
        }
        
        internal void SetLayerDirty(VolumeLayer layer)
        {
            m_sortNeeded[layer] = true;
        }

        public struct WeightedVolume
        {
            public TVolume volume;
            public float weight;

            public WeightedVolume(TVolume volume, float weight)
            {
                this.volume = volume;
                this.weight = weight;
            }
        }
        
        private readonly List<WeightedVolume> m_weightedProfiles = new List<WeightedVolume>();

        public List<WeightedVolume> GetProfiles(Transform trigger, VolumeLayer layer)
        {
            m_weightedProfiles.Clear();

            if (layer == null)
            {
                return m_weightedProfiles;
            }

            bool onlyGlobal = trigger == null;
            Vector3 triggerPos = onlyGlobal ? Vector3.zero : trigger.position;
            
            List<TVolume> volumes;
            if (!m_volumes.TryGetValue(layer, out volumes))
            {
                return m_weightedProfiles;
            }
            
            if (m_sortNeeded[layer])
            {
                volumes.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                m_sortNeeded[layer] = false;
            }

            foreach (var volume in volumes)
            {
                if (!volume.enabled || volume.weight <= 0)
                {
                    continue;
                }

                if (volume.isGlobal)
                {
                    AddWeightedVolume(new WeightedVolume(volume, Mathf.Clamp01(volume.weight)));
                    continue;
                }

                if (onlyGlobal || volume.Colliders.Count == 0)
                {
                    continue;
                }
                
                float closestDistanceSqr = float.PositiveInfinity;
                foreach (var collider in volume.Colliders)
                {
                    if (collider.enabled)
                    {
                        Vector3 closestPoint = collider.ClosestPoint(triggerPos);
                        float d = (closestPoint - triggerPos).sqrMagnitude;
                        if (d < closestDistanceSqr)
                        {
                            closestDistanceSqr = d;
                        }
                    }
                }
                
                float blendDistSqr = volume.blendDistance * volume.blendDistance;
                
                if (closestDistanceSqr > blendDistSqr)
                {
                    continue;
                }
                
                float interpFactor = 1f;
                if (blendDistSqr > 0f)
                {
                    interpFactor = 1f - (closestDistanceSqr / blendDistSqr);
                }

                AddWeightedVolume(new WeightedVolume(volume, Mathf.SmoothStep(0f, 1f, interpFactor) * Mathf.Clamp01(volume.weight)));
            }

            m_weightedProfiles.Sort((a, b) => b.weight.CompareTo(a.weight));
            return m_weightedProfiles;
        }

        private void AddWeightedVolume(WeightedVolume volume)
        {
            if (volume.weight >= 1f)
            {
                // all lower priority profiles are overridden
                m_weightedProfiles.Clear();
            }
            else
            {
                // reduce the weighting of lower priority profiles
                float oneMinusWeight = 1f - volume.weight;
                for (int i = 0; i < m_weightedProfiles.Count; i++)
                {
                    WeightedVolume p = m_weightedProfiles[i];
                    p.weight *= oneMinusWeight;
                    m_weightedProfiles[i] = p;
                }
            }
            m_weightedProfiles.Add(volume);
        }
    }
}