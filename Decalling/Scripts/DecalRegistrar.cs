using System.Collections.Generic;
using UnityEngine;

namespace Framework.DeferredDecalling
{
    public class DecalRegistrar : Singleton<DecalRegistrar>
    {
        private HashSet<Decal> m_uniqueDecals = new HashSet<Decal>();

        // TODO: Materials that are no longer used should be removed from the dictionary

        private Dictionary<Material, List<Decal>> m_deferredDecals = new Dictionary<Material, List<Decal>>();
        public Dictionary<Material, List<Decal>> DeferredDecals
        {
            get { return m_deferredDecals; }
        }

        private Dictionary<Material, List<Decal>> m_unlitDecals = new Dictionary<Material, List<Decal>>();
        public Dictionary<Material, List<Decal>> UnlitDecals
        {
            get { return m_unlitDecals; }
        }
        
        private Dictionary<GameObject, LimitToInfo> m_limitTo = new Dictionary<GameObject, LimitToInfo>();
        public Dictionary<GameObject, LimitToInfo> LimitTo
        {
            get { return m_limitTo; }
        }

        public void RegisterDecal(Decal decal)
        {
            if (!m_uniqueDecals.Contains(decal))
            {
                Material mat = decal.Material;
                List<Decal> decals;
                
                if (decal.Type == Decal.DecalType.Deferred)
                {
                    if (!m_deferredDecals.TryGetValue(mat, out decals))
                    {
                        decals = new List<Decal>();
                        m_deferredDecals[mat] = decals;
                    }
                }
                else if (decal.Type == Decal.DecalType.Unlit)
                {
                    if (!m_unlitDecals.TryGetValue(mat, out decals))
                    {
                        decals = new List<Decal>();
                        m_unlitDecals[mat] = decals;
                    }
                }
                else
                {
                    return;
                }

                AddLimitTo(decal.LimitTo);

                decals.Add(decal);
                m_uniqueDecals.Add(decal);

                decal.Register = decals;
            }
        }

        public void UnregisterDecal(Decal decal)
        {
            if (m_uniqueDecals.Remove(decal))
            {
                RemoveLimitTo(decal.LimitTo);
                decal.Register.Remove(decal);
                decal.Register = null;
            }
        }

        public void AddLimitTo(GameObject limitTo)
        {
            if (limitTo != null)
            {
                LimitToInfo limitToInfo;
                if (!m_limitTo.TryGetValue(limitTo, out limitToInfo))
                {
                    limitToInfo = new LimitToInfo(limitTo);
                    m_limitTo.Add(limitTo, limitToInfo);
                }
                limitToInfo.AddUser();
            }
        }

        public void RemoveLimitTo(GameObject limitTo)
        {
            if (limitTo != null)
            {
                LimitToInfo limitToInfo;
                if (m_limitTo.TryGetValue(limitTo, out limitToInfo))
                {
                    limitToInfo.RemoveUser();

                    if (limitToInfo.UseCount == 0)
                    {
                        m_limitTo.Remove(limitTo);
                    }
                }
            }
        }

        public struct LimitToInfo
        {
            private static readonly List<Renderer> m_rendererCache = new List<Renderer>();
            private static readonly List<Decal> m_decalCache = new List<Decal>();

            private List<Renderer> m_renderers;
            public List<Renderer> Renderers
            {
                get { return m_renderers; }
            }

            private int m_useCount;
            public int UseCount
            {
                get { return m_useCount; }
            }
            
            public LimitToInfo(GameObject limitTo)
            {
                m_renderers = new List<Renderer>();
                
                m_rendererCache.Clear();
                limitTo.GetComponentsInChildren(m_rendererCache);

                foreach (Renderer renderer in m_rendererCache)
                {
                    if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                    {
                        m_decalCache.Clear();
                        renderer.GetComponents(m_decalCache);
                        if (m_decalCache.Count == 0)
                        {
                            m_renderers.Add(renderer);
                        }
                    }
                }

                m_useCount = 0;
            }

            public void AddUser()
            {
                m_useCount++;
            }

            public void RemoveUser()
            {
                m_useCount--;
            }
        }
    }
}
