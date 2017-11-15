namespace Framework.SettingManagement
{
    public class DisplayOptions
    {
        private DisplayType m_displayType;
        public DisplayType DisplayType
        {
            get { return m_displayType; }
        }

        private float m_minValue;
        public float MinValue
        {
            get { return m_minValue; }
        }

        private float m_maxValue;
        public float MaxValue
        {
            get { return m_maxValue; }
        }

        private bool m_integerOnly;
        public bool IntegerOnly
        {
            get { return m_integerOnly; }
        }

        private string[] m_values;
        public string[] Values
        {
            get { return m_values; }
        }

        public DisplayOptions()
        {
            m_displayType = DisplayType.Toggle;
        }

        public DisplayOptions(float minValue, float maxValue, bool integerOnly)
        {
            m_displayType = DisplayType.Slider;
            m_minValue = minValue;
            m_maxValue = maxValue;
            m_integerOnly = integerOnly;
        }
        public DisplayOptions(string[] values)
        {
            m_displayType = DisplayType.Dropdown;
            m_values = values;
        }
    }
}
