using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Framework.IO
{
    /// <summary>
    /// Reads blittable types from a buffer.
    /// </summary>
    public class DataReader : Disposable
    {
        private readonly GCHandle m_handle;
        private readonly IntPtr m_bufStart;
        private readonly IntPtr m_bufEnd;
        private IntPtr m_ptr;

        /// <summary>
        /// The index of the current byte relative to the start offset in the buffer.
        /// </summary>
        public int CurrentByte => (int)(m_ptr.ToInt64() - m_bufStart.ToInt64());

        /// <summary>
        /// The number of bytes that are left until the end of the buffer.
        /// </summary>
        public int BytesRemaining => (int)(m_bufEnd.ToInt64() - m_ptr.ToInt64());

        /// <summary>
        /// Create a data reader.
        /// </summary>
        /// <param name="buffer">The buffer to read from. Will be pinned until the reader is disposed.</param>
        /// <param name="offset">The byte array index to start reading from.</param>
        public DataReader(byte[] buffer, int offset = 0)
        {
            // validate input
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot be negative.");
            }
            if (offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot exceed buffer length.");
            }

            // Pin the buffer so that it can't be moved in memory by the GC.
            // If it were moved, the pointers would be invalidated.
            m_handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            m_bufStart = m_handle.AddrOfPinnedObject();
            m_bufEnd = m_bufStart + buffer.Length;
            m_ptr = m_bufStart + offset;
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                m_handle.Free();
                m_ptr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Reads a single element from the buffer.
        /// </summary>
        /// <typeparam name="T">The type of element to read.</typeparam>
        public unsafe T Read<T>() where T : unmanaged
        {
            T value = *((T*)m_ptr);
            m_ptr += sizeof(T);
            return value;
        }

        /// <summary>
        /// Reads an array from the buffer. More efficient with large arrays.
        /// </summary>
        /// <typeparam name="T">The type of element to write.</typeparam>
        /// <param name="count">The number of values to read.</param>
        public unsafe T[] Read<T>(int count) where T : unmanaged
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Cannot be negative.");
            }

            T[] array = new T[count];

            if (count > 0)
            {
                // pin the array in memory so it is safe to copy to
                GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);

                try
                {
                    // get the size of the array in bytes
                    int length = sizeof(T) * count;

                    // check that there will be no buffer overrun on the source buffer
                    if (BytesRemaining < length)
                    {
                        throw new ArgumentException($"Cannot read {length} bytes from buffer with only {BytesRemaining} bytes remaining.", nameof(count));
                    }

                    // copy the memory using an optimal method
                    Buffer.MemoryCopy(m_ptr.ToPointer(), handle.AddrOfPinnedObject().ToPointer(), length, length);
                    m_ptr += length;
                }
                finally
                {
                    // always free the handle, even if an exception occurs
                    handle.Free();
                }
            }

            return array;
        }

        /// <summary>
        /// Reads a UTF-8 null terminated string from the buffer.
        /// </summary>
        public unsafe string ReadString()
        {
            int length = Read<int>();

            string value = Encoding.UTF8.GetString((byte*)m_ptr, length);
            m_ptr += length;

            return value;
        }
    }
}
