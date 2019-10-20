using System.Collections.Generic;

using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "New Bool Setting", menuName = "Framework/Settings/Bool", order = 1)]
    public class BoolSetting : Setting<bool>
    {
        private static readonly Dictionary<bool, string> DISPLAY_NAMES = new Dictionary<bool, string>()
        {
            { true, "On" },
            { false, "Off" },
        };

        public override bool Deserialize(string serialized, out bool value)
        {
            foreach (bool key in DISPLAY_NAMES.Keys)
            {
                if (serialized == DISPLAY_NAMES[key])
                {
                    value = key;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public override string Serialize(bool value)
        {
            return DISPLAY_NAMES[value];
        }
    }
}
