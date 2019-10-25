using System.Collections.Generic;

namespace Framework.InputManagement
{
    public class SerializableControls
    {
        public Dictionary<string, List<ISource<bool>>> namesToButtonSources = new Dictionary<string, List<ISource<bool>>>();
        public Dictionary<string, List<ISource<float>>> namesToAxisSources = new Dictionary<string, List<ISource<float>>>();
        
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

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
