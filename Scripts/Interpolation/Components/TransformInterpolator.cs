using UnityEngine;

namespace Framework.Interpolation
{
    public class TransformInterpolator : MonoBehaviour, IInterpolator
    {
        public bool useThresholds = false;

        [SerializeField]
        private float m_postionThreshold = 0.001f;
        [SerializeField]
        private float m_rotationThreshold = 0.1f;
        [SerializeField]
        private float m_scaleThreshold = 0.001f;

        private Interpolator<TransformData> m_interpolator;
        private InterpolatedTransform m_interpolated;

        private void Awake()
        {
            m_interpolated = new InterpolatedTransform(transform);
            m_interpolator = new Interpolator<TransformData>(m_interpolated);
        }

        private void OnEnable()
        {
            ForgetPreviousValues();
            InterpolationController.Instance.AddInterpolator(this);
        }

        private void OnDisable()
        {
            InterpolationController.Instance.RemoveInterpolator(this);
        }

        public void SetThresholds(float positionThreshold, float rotationThreshold, float scaleThreshold)
        {
            m_postionThreshold = positionThreshold;
            m_rotationThreshold = rotationThreshold;
            m_scaleThreshold = scaleThreshold;
        }

        public void ForgetPreviousValues()
        {
            m_interpolator.ForgetPreviousValues();
        }

        public void FixedFrame()
        {
            if (isActiveAndEnabled)
            {
                m_interpolator.FixedFrame(useThresholds);
            }
        }

        public void UpdateFrame(float factor)
        {
            if (isActiveAndEnabled)
            {
                m_interpolated.SetThresholds(m_postionThreshold, m_rotationThreshold, m_scaleThreshold);

                m_interpolator.UpdateFrame(factor, useThresholds);
            }
        }
    }
}