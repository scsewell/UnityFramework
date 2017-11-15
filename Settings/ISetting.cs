using System;

namespace Framework.SettingManagement
{
    public interface ISetting
    {
        string Name { get; }
        DisplayOptions DisplayOptions { get; }
        void Apply();
        void UseDefaultValue();
        string Serialize();
        void Deserialize(string value);
        bool IsOfType(Type type);
    }
}