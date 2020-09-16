using UnityEngine;

namespace Framework.AssetBundles
{
    /// <summary>
    /// Stores fields needed to manage a reference to an asset that
    /// may be stored in an asset bundle.
    /// </summary>
    public abstract class AssetBundleReference
    {
        [SerializeField]
        protected string m_bundleName = null;

        [SerializeField]
        protected string m_assetGuid = null;

        /// <summary>
        /// Gets if the referenced asset resides in asset bundle.
        /// </summary>
        public bool IsBundled => !string.IsNullOrEmpty(m_bundleName);

#if UNITY_EDITOR
        /// <summary>
        /// Sets the reference from the currently assigned asset guid.
        /// </summary>
        /// <returns>False if no asset with the given GUID exists.</returns>
        internal abstract bool UpdateBundlePath();
#endif

        public override bool Equals(object obj)
        {
            return obj is AssetBundleReference other && m_assetGuid == other.m_assetGuid;
        }

        public override int GetHashCode()
        {
            return m_assetGuid.GetHashCode();
        }

        public override string ToString()
        {
            return $"{m_assetGuid}: {m_bundleName}";
        }
    }
}
