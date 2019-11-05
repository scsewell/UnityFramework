using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Framework
{
    /// <summary>
    /// A Guid that can be serialized by Unity.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct UnityGuid : IComparable, IComparable<Guid>, IEquatable<Guid>
    {
        [FieldOffset(0)]
        private Guid m_guid;

        [SerializeField]
        [FieldOffset(0)]
        private long m_part0;

        [SerializeField]
        [FieldOffset(8)]
        private long m_part1;

        public int CompareTo(object obj)
        {
            if (obj is UnityGuid unityGuid)
            {
                return m_guid.CompareTo(unityGuid.m_guid);
            }
            if (obj is Guid guid)
            {
                return m_guid.CompareTo(guid);
            }
            return -1;
        }

        public int CompareTo(Guid other)
        {
            return m_guid.CompareTo(other);
        }

        public bool Equals(Guid other)
        {
            return m_guid == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is UnityGuid unityGuid)
            {
                return unityGuid == m_guid;
            }
            if (obj is Guid guid)
            {
                return guid == m_guid;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return m_guid.GetHashCode();
        }

        public override string ToString()
        {
            return m_guid.ToString();
        }

        public static implicit operator Guid(UnityGuid uGuid) => uGuid.m_guid;
    }
}
