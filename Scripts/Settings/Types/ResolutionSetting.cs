using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "Resolution Setting", menuName = "Framework/Settings/Resolution", order = 8)]
    public class ResolutionSetting : Setting<Resolution>
    {
        private Resolution[] m_resolutions = null;
        private string[] m_displayValues = null;

        /// <summary>
        /// The smallest resolution supported.
        /// </summary>
        public Resolution MinSupported => m_resolutions.First();

        /// <summary>
        /// The largest resolution supported.
        /// </summary>
        public Resolution MaxSupported => m_resolutions.Last();

        /// <inheritdoc/>
        public override bool IsRuntime => true;

        /// <inheritdoc/>
        public override string[] DisplayValues => m_displayValues;

        /// <inheritdoc/>
        internal override void Initialize()
        {
            // get all unique resolution sizes
            var resolutions = new List<Resolution>();
            var serialized = new HashSet<string>();

            foreach (var res in Screen.resolutions)
            {
                var s = Serialize(res);

                if (!serialized.Contains(s))
                {
                    serialized.Add(s);
                    resolutions.Add(res);
                }
            }

            m_resolutions = resolutions.ToArray();
            m_displayValues = resolutions.Select(r => Serialize(r)).ToArray();

            // set default resolution
            m_defaultValue = Serialize(MaxSupported);

            // apply the default value
            base.Initialize();
        }

        /// <inheritdoc/>
        internal override bool Deserialize(string serialized, out Resolution value)
        {
            value = new Resolution();

            var split = serialized.Split('x');

            if (split.Length == 2 &&
                int.TryParse(split[0].Trim(), out var width) &&
                int.TryParse(split[1].Trim(), out var height))
            {
                value.width = width;
                value.height = height;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        internal override string Serialize(Resolution value)
        {
            return $"{value.width} x {value.height}";
        }
    }
}
