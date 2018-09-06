using System;
using UnityEngine;

namespace Framework
{
    [Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        [SerializeField]
        private Space m_space;

        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale, Space space)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            m_space = space;
        }

        public TransformData(Transform transform, Space space = Space.Self)
        {
            if (space == Space.Self || transform.parent == null)
            {
                position = transform.localPosition;
                rotation = transform.localRotation;
                scale = transform.localScale;
            }
            else
            {
                position = transform.position;
                rotation = transform.rotation;
                scale = transform.lossyScale;
            }
            m_space = space;
        }

        public void ApplyTo(Transform target)
        {
            if (m_space == Space.Self || target.parent == null)
            {
                target.localPosition = position;
                target.localRotation = rotation;
                target.localScale = scale;
            }
            else
            {
                target.position = position;
                target.rotation = rotation;
                target.localScale = new Vector3(
                    scale.x / target.parent.lossyScale.x,
                    scale.y / target.parent.lossyScale.y,
                    scale.z / target.parent.lossyScale.z
                );
            }
        }

        public static TransformData Interpolate(Transform current, Transform target, float fac, Space space = Space.World)
        {
            return Interpolate(new TransformData(current, space), new TransformData(target, space), fac);
        }

        public static TransformData Interpolate(Transform current, TransformData target, float fac)
        {
            return Interpolate(new TransformData(current, target.m_space), target, fac);
        }

        public static TransformData Interpolate(TransformData current, Transform target, float fac)
        {
            return Interpolate(current, new TransformData(target, current.m_space), fac);
        }

        public static TransformData Interpolate(TransformData current, TransformData target, float fac)
        {
            return new TransformData(
                Vector3.Lerp(current.position, target.position, fac),
                Quaternion.Slerp(current.rotation, target.rotation, fac),
                Vector3.Lerp(current.scale, target.scale, fac),
                current.m_space
            );
        }

        public override int GetHashCode()
        {
            return position.GetHashCode() ^ (rotation.GetHashCode() << 2) ^ (scale.GetHashCode() >> 2) ^ (m_space.GetHashCode() << 4);
        }

        public override bool Equals(object obj)
        {
            if (obj is TransformData)
            {
                return Equals((TransformData)obj);
            }
            return false;
        }

        public bool Equals(TransformData other)
        {
            return position == other.position && rotation == other.rotation && scale == other.scale && m_space == other.m_space;
        }

        public override string ToString()
        {
            return $"space:{m_space} pos:{position} rot:{rotation} scale:{scale}";
        }

        public static bool operator ==(TransformData a, TransformData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TransformData a, TransformData b)
        {
            return !a.Equals(b);
        }
    }
}