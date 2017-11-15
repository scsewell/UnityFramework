using System.Collections.Generic;

namespace Framework.InputManagement
{
    public interface IInputSource
    {
        string DisplayName { get; }
        bool CanRebind { get; }
        List<SourceInfo> SourceInfos { get; }
        void RemoveSource(int index);
    }
}
