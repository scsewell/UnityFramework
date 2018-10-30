namespace Framework.Interpolation
{
    public class Interpolator<T>
    {
        private T[] m_latestValues = new T[2];
        private int m_newestValueIndex;

        private T olderValue
        {
            get { return m_latestValues[GetOlderValueIndex()]; }
        }

        private T newerValue
        {
            get { return m_latestValues[m_newestValueIndex]; }
        }

        private IInterpolated<T> m_interpolated;
        private bool m_firstFixedLoop = true;
        private bool m_firstUpdateLoop = true;
        private bool m_interpolate = true;
        private T m_lastVisual;

        public Interpolator(IInterpolated<T> interpolated)
        {
            m_interpolated = interpolated;
            ForgetPreviousValues();
        }

        public void ForgetPreviousValues()
        {
            T t = m_interpolated.ReadOriginal();
            m_latestValues[0] = t;
            m_latestValues[1] = t;
        }

        public void FixedFrame(bool useThresholds)
        {
            if (m_firstFixedLoop)
            {
                if (useThresholds)
                {
                    m_lastVisual = m_interpolated.ReadOriginal();
                }
                if (m_interpolate)
                {
                    m_interpolated.AffectOriginal(newerValue);
                }
                m_firstFixedLoop = false;
            }
            else
            {
                StoreCurrentValue();
            }

            m_firstUpdateLoop = true;
        }

        public void UpdateFrame(float factor, bool useThresholds)
        {
            if (m_firstUpdateLoop)
            {
                StoreCurrentValue();
                m_interpolate = !useThresholds || m_interpolated.AreDifferent(m_lastVisual, newerValue);
                m_firstUpdateLoop = false;
            }

            if (m_interpolate)
            {
                m_interpolated.AffectOriginal(m_interpolated.GetInterpolatedValue(olderValue, newerValue, factor));
            }

            m_firstFixedLoop = true;
        }

        public void StoreCurrentValue()
        {
            m_newestValueIndex = GetOlderValueIndex();
            m_latestValues[m_newestValueIndex] = m_interpolated.ReadOriginal();
        }

        private int GetOlderValueIndex()
        {
            return (m_newestValueIndex + 1) % 2;
        }
    }
}