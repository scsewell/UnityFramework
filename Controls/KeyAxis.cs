using UnityEngine;

namespace Framework.InputManagement
{
    /// <summary>
    /// Stores an axis type input for a pair of keys.
    /// </summary>
    public class KeyAxis : ISource<float>
    {
        private KeyCode m_negative;
        private KeyCode m_positive;

        private SourceInfo m_sourceInfo;
        public SourceInfo SourceInfo
        {
            get { return m_sourceInfo; }
        }

        public KeyAxis(KeyCode negative, KeyCode positive)
        {
            m_negative = negative;
            m_positive = positive;

            m_sourceInfo = new SourceInfo(ControlNames.GetName(m_positive) + "-" + ControlNames.GetName(m_negative), SourceType.MouseKeyboard);
        }

        // returns the value of the axis
        public float GetValue()
        {
            return GetKeyValue(m_positive) - GetKeyValue(m_negative);
        }
        
        private float GetKeyValue(KeyCode key)
        {
            return Input.GetKey(key) ? 1 : 0;
        }
    }
}