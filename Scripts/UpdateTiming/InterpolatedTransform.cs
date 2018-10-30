using UnityEngine;

namespace Framework.Interpolation
{
    public class InterpolatedTransform : IInterpolated<TransformData>
    {
        private Transform m_transform;

        private float m_positionThreshold = 0;
        private float m_rotationThreshold = 0;
        private float m_scaleThreshold = 0;

        public InterpolatedTransform(Transform transform)
        {
            m_transform = transform;
        }

        public void SetThresholds(float positionThreshold, float rotationThreshold, float scaleThreshold)
        {
            m_positionThreshold = positionThreshold;
            m_rotationThreshold = rotationThreshold;
            m_scaleThreshold = scaleThreshold;
        }

        public TransformData GetInterpolatedValue(TransformData older, TransformData newer, float interpolationFactor)
        {
            return TransformData.Interpolate(older, newer, interpolationFactor);
        }

        public TransformData ReadOriginal()
        {
            return new TransformData(m_transform, Space.Self);
        }

        public void AffectOriginal(TransformData transformData)
        {
            transformData.ApplyTo(m_transform);
        }

        public bool AreDifferent(TransformData first, TransformData second)
        {
            return  Vector3.Distance(first.position, second.position) > m_positionThreshold ||
                    Quaternion.Angle(first.rotation, second.rotation) > m_rotationThreshold ||
                    Vector3.Distance(first.scale, second.scale) > m_scaleThreshold;
        }
    }
}