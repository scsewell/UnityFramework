using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "New Bool Setting", menuName = "Framework/Settings/Bool", order = 1)]
    public class BoolSetting : Setting<bool>
    {
        private static readonly string[] DISPLAY_VALUES = new string[]
        {
            "Off",
            "On",
        };

        public override string[] DisplayValues => DISPLAY_VALUES;

        public override bool Deserialize(string serialized, out bool value)
        {
            if (serialized == DISPLAY_VALUES[0])
            {
                value = false;
                return true;
            }
            if (serialized == DISPLAY_VALUES[1])
            {
                value = true;
                return true;
            }

            value = default;
            return false;
        }

        public override string Serialize(bool value)
        {
            return DISPLAY_VALUES[value ? 1 : 0];
        }
    }
}
