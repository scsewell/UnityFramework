using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "Int Setting", menuName = "Framework/Settings/Int", order = 2)]
    public class IntSetting : RangeSetting<int>
    {
        /// <summary>
        /// The min value allowed.
        /// </summary>
        public override int Min => Mathf.RoundToInt(SnapToRange(m_range.Min));

        /// <summary>
        /// The max value allowed.
        /// </summary>
        public override int Max => Mathf.RoundToInt(SnapToRange(m_range.Max));

        public override int Sanitize(int newValue)
        {
            return Mathf.RoundToInt(SnapToRange(newValue));
        }

        public override bool Deserialize(string serialized, out int value)
        {
            return int.TryParse(serialized, out value);
        }

        public override string Serialize(int value)
        {
            return value.ToString();
        }
    }
}
