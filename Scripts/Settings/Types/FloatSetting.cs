using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "Float Setting", menuName = "Framework/Settings/Float", order = 3)]
    public class FloatSetting : RangeSetting<float>
    {
        /// <summary>
        /// The min value allowed.
        /// </summary>
        public override float Min => m_range.Min;

        /// <summary>
        /// The max value allowed.
        /// </summary>
        public override float Max => m_range.Max;

        public override float Sanitize(float newValue)
        {
            return SnapToRange(newValue);
        }

        public override bool Deserialize(string serialized, out float value)
        {
            return float.TryParse(serialized, out value);
        }

        public override string Serialize(float value)
        {
            return value.ToString();
        }
    }
}
