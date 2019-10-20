using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "New String Setting", menuName = "Framework/Settings/String", order = 5)]
    public class StringSetting : Setting<string>
    {
        public override bool Deserialize(string serialized, out string value)
        {
            if (serialized != null)
            {
                value = serialized;
                return true;
            }

            value = default;
            return false;
        }

        public override string Serialize(string value)
        {
            return value;
        }
    }
}
