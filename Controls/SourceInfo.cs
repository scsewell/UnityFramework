using System;

namespace Framework.InputManagement
{
    public struct SourceInfo : IEquatable<SourceInfo>
    {
        private string m_name;
        public string Name
        {
            get { return m_name; }
        }

        private SourceType m_sourceType;
        public SourceType SourceType
        {
            get { return m_sourceType; }
        }

        public SourceInfo(string name, SourceType sourceType)
        {
            m_name = name;
            m_sourceType = sourceType;
        }

        public override bool Equals(Object obj)
        {
            return obj is SourceInfo && this == (SourceInfo)obj;
        }

        public override int GetHashCode()
        {
            return m_name.GetHashCode() ^ m_sourceType.GetHashCode();
        }

        public bool Equals(SourceInfo sourceInfo)
        {
            return m_name == sourceInfo.Name && m_sourceType == sourceInfo.SourceType;
        }

        public static bool operator == (SourceInfo c1, SourceInfo c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator != (SourceInfo c1, SourceInfo c2)
        {
            return !c1.Equals(c2);
        }
    }
}
