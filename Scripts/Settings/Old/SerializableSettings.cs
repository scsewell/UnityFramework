using System.Collections.Generic;

namespace Framework.SettingManagement
{
    public class SerializableSettings
    {
        public Dictionary<string, Dictionary<string, string>> categoryToSettings = new Dictionary<string, Dictionary<string, string>>();
        
        public SerializableSettings Serialize(SettingCollection settings)
        {
            foreach (string category in settings.CategoryToSettings.Keys)
            {
                Dictionary<string, string> nameToValue = new Dictionary<string, string>();
                categoryToSettings.Add(category, nameToValue);

                foreach (ISetting setting in settings.CategoryToSettings[category])
                {
                    nameToValue.Add(setting.Name, setting.Serialize());
                }
            }
            return this;
        }

        public bool Deserialize(SettingCollection settings)
        {
            try
            {
                foreach (string category in settings.CategoryToSettings.Keys)
                {
                    Dictionary<string, string> nameToValue = categoryToSettings[category];
                    foreach (ISetting setting in settings.CategoryToSettings[category])
                    {
                        if (nameToValue.ContainsKey(setting.Name))
                        {
                            setting.Deserialize(nameToValue[setting.Name]);
                        }
                    }
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
