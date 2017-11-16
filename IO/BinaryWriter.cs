using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Framework.IO
{
    public class BinaryWriter
    {
        private delegate int Write(byte[] buf, int offset);
        
        private int m_totalSize;
        List<Write> m_plannedWrites;

        public BinaryWriter()
        {
            m_totalSize = 0;
            m_plannedWrites = new List<Write>();
        }

        public void WriteValue<T>(T val)
        {
            WriteArray(new T[] { val }, false);
        }

        public void WriteArray<T>(T[] vals, bool includeSizeHeader = true)
        {
            if (includeSizeHeader)
            {
                WriteValue(vals.Length);
            }

            int len = (vals.Length * Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(vals, GCHandleType.Pinned);

            m_plannedWrites.Add(new Write((buff, offset) =>
            {
                try
                {
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    Marshal.Copy(pointer, buff, offset, len);
                }
                finally
                {
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                }
                return offset + len;
            }));

            m_totalSize += len;
        }

        public byte[] GetBytes()
        {
            byte[] output = new byte[m_totalSize];
            int offset = 0;
            foreach (Write write in m_plannedWrites)
            {
                offset = write(output, offset);
            }
            return output;
        }
    }
}
