using UnityEngine;

namespace Framework.InputManagement
{
    /// <summary>
    /// Stores an axis type input for a joystick.
    /// </summary>
    public class JoystickAxis : ISource<float>
    {
        private GamepadAxis m_axis;

        private SourceInfo m_sourceInfo;
        public SourceInfo SourceInfo
        {
            get { return m_sourceInfo; }
        }

        public JoystickAxis(GamepadAxis axis)
        {
            m_axis = axis;

            m_sourceInfo = new SourceInfo(ControlNames.GetName(m_axis), SourceType.Joystick);
        }
        
        public float GetValue()
        {
            return GetAxisValue(m_axis);
        }
        
        public static float GetAxisValue(GamepadAxis axis)
        {
            switch (axis)
            {
                case GamepadAxis.DpadX:
                    return Input.GetAxis("DPad_XAxis");
                case GamepadAxis.DpadY:
                    return -Input.GetAxis("DPad_YAxis");
                case GamepadAxis.LStickX:
                    return Input.GetAxis("L_XAxis");
                case GamepadAxis.LStickY:
                    return -Input.GetAxis("L_YAxis");
                case GamepadAxis.RStickX:
                    return Input.GetAxis("R_XAxis");
                case GamepadAxis.RStickY:
                    return -Input.GetAxis("R_YAxis");
                case GamepadAxis.Triggers:
                    /*
                    return Input.GetAxis("Triggers");
                    /*/
                    float LTrigger = Input.GetAxis("TriggersL");
                    float RTrigger = Input.GetAxis("TriggersR");
                    return RTrigger - LTrigger;
                    //*/
            }
            return 0;
        }
    }
}