using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Framework.IO
{
    public class BinaryReader
    {
        private byte[] m_data;
        private int m_index;

        public BinaryReader(byte[] data)
        {
            m_index = 0;
            m_data = data;
        }

        public int GetReadPointer()
        {
            return m_index;
        }

        public void SetReadPointer(int index)
        {
            m_index = index;
        }

        public void SetReadPointerRelative(int offset)
        {
            m_index = Mathf.Max(m_index + offset, 0);
        }

        public T ReadValue<T>(T[] buffer = null)
        {
            return Read(m_data, ref m_index, buffer ?? new T[1])[0];
        }

        public T[] ReadArray<T>()
        {
            T[] vals = new T[ReadInt()];
            return Read(m_data, ref m_index, vals);
        }

        public static T ReadValue<T>(byte[] data, int index)
        {
            return Read(data, ref index, new T[1])[0];
        }

        public static T[] ReadArray<T>(byte[] data, int index)
        {
            T[] vals = new T[ReadValue<int>(data, index)];
            return Read(data, ref index, vals);
        }

        private static T[] Read<T>(byte[] data, ref int index, T[] vals)
        {
            int len = vals.Length * Marshal.SizeOf(typeof(T));

            GCHandle handle = GCHandle.Alloc(vals, GCHandleType.Pinned);
            try
            {
                IntPtr pointer = handle.AddrOfPinnedObject();
                Marshal.Copy(data, index, pointer, len);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            index += len;
            return vals;
        }

        /*
         * Wrapper methods to handle common types.
         */

        private byte[] m_byteBuffer = new byte[1];
        public byte ReadByte()
        {
            return ReadValue(m_byteBuffer);
        }

        private int[] m_intBuffer = new int[1];
        public int ReadInt()
        {
            return ReadValue(m_intBuffer);
        }

        private long[] m_longBuffer = new long[1];
        public long ReadLong()
        {
            return ReadValue(m_longBuffer);
        }

        private float[] m_floatBuffer = new float[1];
        public float ReadFloat()
        {
            return ReadValue(m_floatBuffer);
        }

        private double[] m_doubleBuffer = new double[1];
        public double ReadDouble()
        {
            return ReadValue(m_doubleBuffer);
        }

        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public string ReadString()
        {
            return Encoding.ASCII.GetString(ReadArray<byte>());
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public bool[] ReadBoolArray()
        {
            bool[] vals = new bool[ReadInt()];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = ReadBool();
            }
            return vals;
        }

        public string[] ReadStringArray()
        {
            string[] vals = new string[ReadInt()];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = ReadString();
            }
            return vals;
        }
    }
}
