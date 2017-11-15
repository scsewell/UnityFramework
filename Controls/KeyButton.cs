using UnityEngine;

namespace Framework.InputManagement
{
    public class KeyButton : ISource<bool>
    {
        private KeyCode m_button;

        private SourceInfo m_sourceInfo;
        public SourceInfo SourceInfo
        {
            get { return m_sourceInfo; }
        }

        public KeyButton(KeyCode button)
        {
            m_button = button;

            m_sourceInfo = new SourceInfo(ControlNames.GetName(m_button), SourceType.MouseKeyboard);
        }

        public bool GetValue()
        {
            return GetButtonValue(m_button);
        }
        
        public static bool GetButtonValue(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }
    }
}
