using System.Collections.Generic;

using UnityEngine;

namespace Framework.Volumes
{
    /// <summary>
    /// A class that keeps track of volumes of a specific type.
    /// </summary>
    /// <typeparam name="TVolume">The concrete volume type managed by this class.</typeparam>
    /// <typeparam name="TManager">The concrete type inheriting from this class.</typeparam>
    public abstract class VolumeManager<TVolume, TManager>
        where TVolume : Volume<TVolume, TManager>
        where TManager : VolumeManager<TVolume, TManager>, new()
    {
        private static readonly TManager m_instance = new TManager();

        /// <summary>
        /// The volume manager instance.
        /// </summary>
        public static TManager Instance => m_instance;

        private readonly Dictionary<VolumeLayer, List<TVolume>> m_volumes = new Dictionary<VolumeLayer, List<TVolume>>();
        private readonly Dictionary<VolumeLayer, bool> m_sortNeeded = new Dictionary<VolumeLayer, bool>();
        private readonly List<WeightedVolume> m_weightedVolumes = new List<WeightedVolume>();

        /// <summary>
        /// Registers a volume with the manager.
        /// </summary>
        /// <param name="volume">The volume to register.</param>
        public void Register(TVolume volume)
        {
            if (volume == null)
            {
                return;
            }

            var layer = volume.Layer;

            if (layer == null)
            {
                return;
            }

            if (!m_volumes.TryGetValue(layer, out var volumes))
            {
                volumes = new List<TVolume>();
                m_volumes.Add(layer, volumes);
            }

            volumes.Add(volume);
            SetLayerDirty(layer);
        }

        /// <summary>
        /// Deregisters a volume from the manager.
        /// </summary>
        /// <param name="volume">The volume to deregister.</param>
        public void Deregister(TVolume volume)
        {
            if (volume == null)
            {
                return;
            }

            var layer = volume.Layer;

            if (layer == null)
            {
                return;
            }

            if (m_volumes.TryGetValue(layer, out var volumes) && volumes.Remove(volume))
            {
                SetLayerDirty(layer);
            }
        }

        /// <summary>
        /// Called when the volumes enabled on a layer have changed.
        /// </summary>
        /// <param name="layer">The layer to mark dirty.</param>
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

        /// <summary>
        /// Finds the volumes on a layer that have a non-zero influence given a scene point.
        /// </summary>
        /// <param name="layer">The volume layer to evaluate.</param>
        /// <param name="point">The scene point to test. When null, only global volumes are considred.</param>
        /// <returns>A list of relevant volumes sorted by decending weight.</returns>
        public IReadOnlyList<WeightedVolume> Evaluate(VolumeLayer layer, Transform point)
        {
            m_weightedVolumes.Clear();

            if (layer == null)
            {
                return m_weightedVolumes;
            }

            if (!m_volumes.TryGetValue(layer, out var volumes))
            {
                return m_weightedVolumes;
            }

            // update the volume layer if it is dirty
            if (m_sortNeeded[layer])
            {
                volumes.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                m_sortNeeded[layer] = false;
            }

            foreach (var volume in volumes)
            {
                if (!volume.enabled || volume.Weight <= 0)
                {
                    continue;
                }

                if (volume.IsGlobal)
                {
                    AddWeightedVolume(new WeightedVolume(volume, volume.Weight));
                    continue;
                }

                if (point == null || volume.Colliders.Count == 0)
                {
                    continue;
                }

                // find the squared distance from the volume to the point
                var pos = point.position;
                var closestDistanceSqr = float.PositiveInfinity;

                foreach (var collider in volume.Colliders)
                {
                    if (collider.enabled)
                    {
                        var d = (collider.ClosestPoint(pos) - pos).sqrMagnitude;
                        if (d < closestDistanceSqr)
                        {
                            closestDistanceSqr = d;
                        }
                    }
                }

                // check if the volume is close enough to have influence
                var blendDistSqr = volume.BlendDistance * volume.BlendDistance;

                if (closestDistanceSqr > blendDistSqr)
                {
                    continue;
                }

                // linearlize the distance and compute the influence
                var weight = volume.Weight;

                if (blendDistSqr > 0f)
                {
                    weight *= 1f - (Mathf.Sqrt(closestDistanceSqr) / Mathf.Sqrt(blendDistSqr));
                }

                AddWeightedVolume(new WeightedVolume(volume, weight));
            }

            // sort the volumes by decending influence
            m_weightedVolumes.Sort((a, b) => b.weight.CompareTo(a.weight));

            return m_weightedVolumes;
        }

        private void AddWeightedVolume(WeightedVolume volume)
        {
            if (volume.weight == 1f)
            {
                // all lower priority profiles are overridden
                m_weightedVolumes.Clear();
            }
            else
            {
                // reduce the weighting of lower priority profiles
                var oneMinusWeight = 1f - volume.weight;
                for (var i = 0; i < m_weightedVolumes.Count; i++)
                {
                    var p = m_weightedVolumes[i];
                    p.weight *= oneMinusWeight;
                    m_weightedVolumes[i] = p;
                }
            }

            m_weightedVolumes.Add(volume);
        }
    }
}