using UnityEngine;

namespace Framework.Interpolation
{
    public class FloatInterpolator : MonoBehaviour, IInterpolator
    {
        public bool useThresholds = false;

        [SerializeField]
        private float m_threshold = 0.001f;

        private Interpolator<float> m_interpolator;
        private InterpolatedFloat m_interpolated;
        private bool m_initialized = false;

        public FloatInterpolator Initialize(InterpolatedFloat interpolated)
        {
            m_interpolator = new Interpolator<float>(interpolated);
            m_interpolated = interpolated;
            m_initialized = true;
            return this;
        }

        private void OnEnable()
        {
            if (m_initialized)
            {
                ForgetPreviousValues();
            }
            InterpolationController.Instance.AddInterpolator(this);
        }

        private void OnDisable()
        {
            InterpolationController.Instance.RemoveInterpolator(this);
        }

        private void Start()
        {
            if (!m_initialized)
            {
                Debug.LogError("Interpolator was not initialized on " + transform.name);
            }
        }

        public void SetThreshold(float threshold)
        {
            m_threshold = threshold;
        }

        public void ForgetPreviousValues()
        {
            if (m_interpolator != null)
            {
                m_interpolator.ForgetPreviousValues();
            }
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
                m_interpolated.SetThreshold(m_threshold);

                m_interpolator.UpdateFrame(factor, useThresholds);
            }
        }
    }
}