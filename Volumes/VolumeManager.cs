using System.Collections.Generic;
using UnityEngine;

namespace Framework.Volumes
{
    public abstract class VolumeManager<TProfile, TManager> 
        where TProfile : VolumeProfile 
        where TManager : VolumeManager<TProfile, TManager>, new()
    {
        private static readonly TManager m_instance = new TManager();
        public static TManager Instance => m_instance;
        
        private readonly Dictionary<VolumeLayer, List<Volume<TProfile, TManager>>> m_volumes = new Dictionary<VolumeLayer, List<Volume<TProfile, TManager>>>();
        private readonly Dictionary<VolumeLayer, bool> m_sortNeeded = new Dictionary<VolumeLayer, bool>();
        
        public void Register(Volume<TProfile, TManager> volume, VolumeLayer layer)
        {
            if (layer != null)
            {
                List<Volume<TProfile, TManager>> volumes;
                if (!m_volumes.TryGetValue(layer, out volumes))
                {
                    volumes = new List<Volume<TProfile, TManager>>();
                    m_volumes.Add(layer, volumes);
                }
                volumes.Add(volume);
                SetLayerDirty(layer);
            }
        }

        public void Deregister(Volume<TProfile, TManager> volume, VolumeLayer layer)
        {
            if (layer != null)
            {
                List<Volume<TProfile, TManager>> volumes;
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

        public struct WeightedProfile
        {
            public TProfile profile;
            public float weight;

            public WeightedProfile(TProfile profile, float weight)
            {
                this.profile = profile;
                this.weight = weight;
            }
        }

        private readonly TProfile m_defaultProfile = ScriptableObject.CreateInstance<TProfile>();
        private readonly List<WeightedProfile> m_weightedProfiles = new List<WeightedProfile>();

        public List<WeightedProfile> GetProfiles(Transform trigger, VolumeLayer layer)
        {
            m_weightedProfiles.Clear();
            m_weightedProfiles.Add(new WeightedProfile(m_defaultProfile, 1.0f));

            bool onlyGlobal = trigger == null;
            Vector3 triggerPos = onlyGlobal ? Vector3.zero : trigger.position;
            
            List<Volume<TProfile, TManager>> volumes;
            if (!m_volumes.TryGetValue(layer, out volumes))
            {
                return m_weightedProfiles;
            }
            
            if (m_sortNeeded[layer])
            {
                volumes.Sort((a, b) => a.priority.CompareTo(b.priority));
                m_sortNeeded[layer] = false;
            }
            
            foreach (var volume in volumes)
            {
                if (!volume.enabled || volume.Profile == null || volume.weight <= 0)
                {
                    continue;
                }

                if (volume.isGlobal)
                {
                    AddWeightedProfile(volume.Profile, Mathf.Clamp01(volume.weight));
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

                AddWeightedProfile(volume.Profile, interpFactor * Mathf.Clamp01(volume.weight));
            }

            m_weightedProfiles.Sort((a, b) => a.weight.CompareTo(b.weight));
            return m_weightedProfiles;
        }

        private void AddWeightedProfile(TProfile profile, float weight)
        {
            if (weight >= 1f)
            {
                // all lower priority profiles are overridden
                m_weightedProfiles.Clear();
            }
            else
            {
                // reduce the weighting of lower priority profiles
                float oneMinusWeight = 1f - weight;
                for (int i = 0; i < m_weightedProfiles.Count; i++)
                {
                    WeightedProfile p = m_weightedProfiles[i];
                    p.weight *= oneMinusWeight;
                    m_weightedProfiles[i] = p;
                }
            }
            m_weightedProfiles.Add(new WeightedProfile(profile, weight));
        }
    }
}