using System;
using System.Runtime.InteropServices;
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
            int len = (vals.Length * Marshal.SizeOf(typeof(T)));

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
    }
}
