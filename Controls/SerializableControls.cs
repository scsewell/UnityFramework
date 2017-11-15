using System.Collections.Generic;
using Framework.SettingManagement;

namespace Framework.InputManagement
{
    public class SerializableControls
    {
        public Dictionary<string, List<ISource<bool>>> namesToButtonSources = new Dictionary<string, List<ISource<bool>>>();
        public Dictionary<string, List<ISource<float>>> namesToAxisSources = new Dictionary<string, List<ISource<float>>>();
        public SerializableSettings controlSettings;
        
        public SerializableControls Serialize(Controls controls)
        {
            foreach (string name in controls.NameToButton.Keys)
            {
                namesToButtonSources.Add(name, controls.NameToButton[name].Sources);
            }
            foreach (string name in controls.NameToAxis.Keys)
            {
                namesToAxisSources.Add(name, controls.NameToAxis[name].Sources);
            }
            controlSettings = controls.Settings.Serialize();
            return this;
        }

        public bool Deserialize(Controls controls)
        {
            try
            {
                foreach (string name in controls.NameToButton.Keys)
                {
                    controls.NameToButton[name].Sources = namesToButtonSources[name];
                }

                foreach (string name in controls.NameToAxis.Keys)
                {
                    controls.NameToAxis[name].Sources = namesToAxisSources[name];
                }

                return controls.Settings.Deserialize(controlSettings);
            }
            catch
            {
                return false;
            }
        }
    }
}
