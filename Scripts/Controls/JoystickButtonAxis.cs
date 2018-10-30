namespace Framework.InputManagement
{
    /// <summary>
    /// Stores an axis type input for a pair of keys.
    /// </summary>
    public class JoystickButtonAxis : ISource<float>
    {
        private GamepadButton m_negative;
        private GamepadButton m_positive;

        private SourceInfo m_sourceInfo;
        public SourceInfo SourceInfo
        {
            get { return m_sourceInfo; }
        }

        public JoystickButtonAxis(GamepadButton negative, GamepadButton positive)
        {
            m_negative = negative;
            m_positive = positive;

            m_sourceInfo = new SourceInfo(ControlNames.GetName(m_positive) + "-" + ControlNames.GetName(m_negative), SourceType.Joystick);
        }

        // returns the value of the axis
        public float GetValue()
        {
            return GetButtonValue(m_positive) - GetButtonValue(m_negative);
        }

        private float GetButtonValue(GamepadButton button)
        {
            return JoystickButton.GetButtonValue(button) ? 1 : 0;
        }
    }
}