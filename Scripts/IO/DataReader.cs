using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Framework.IO
{
    /// <summary>
    /// Reads blittable types from a buffer.
    /// </summary>
    public class DataReader : Disposable
    {
        private readonly Stream m_stream;
        private byte[] m_buffer;
        private GCHandle m_handle;
        private IntPtr m_bufStart;
        private IntPtr m_bufEnd;
        private IntPtr m_ptr;

        /// <summary>
        /// Is the reader's data source a stream.
        /// </summary>
        public bool IsStream { get; }

        /// <summary>
        /// The number of bytes that are left until the end of the buffer.
        /// </summary>
        private int BytesRemaining => (int)(m_bufEnd.ToInt64() - m_ptr.ToInt64());

        /// <summary>
        /// Create a data reader for a fixed buffer.
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

            IsStream = false;

            Prepare(buffer, GCHandle.Alloc(buffer, GCHandleType.Pinned), offset);
        }

        /// <summary>
        /// Create a data reader for a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public DataReader(Stream stream)
        {
            // validate input
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream must be readable.", nameof(stream));
            }

            IsStream = true;
            m_stream = stream;

            byte[] buffer = new byte[4096];
            Prepare(buffer, GCHandle.Alloc(buffer, GCHandleType.Pinned), 0);

            // load the initial data from the stream
            m_ptr = m_bufEnd;
            GetNextStreamedData(buffer.Length, false);
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                if (m_stream != null)
                {
                    m_stream.Close();
                }

                m_buffer = null;
                m_handle.Free();

                m_bufStart = IntPtr.Zero;
                m_bufEnd = IntPtr.Zero;
                m_ptr = IntPtr.Zero;
            }
        }

        private void Prepare(byte[] buffer, GCHandle handle, int offset)
        {
            m_buffer = buffer;
            m_handle = handle;

            m_bufStart = m_handle.AddrOfPinnedObject();
            m_bufEnd = m_bufStart + m_buffer.Length;
            m_ptr = m_bufStart + offset;
        }
        
        private unsafe void GetNextStreamedData(int dataSize, bool checkOverrun = true)
        {
            // only streams support reading additional data
            if (!IsStream)
            {
                throw new Exception("Attempted read is larger than the buffer's remaining contents!");
            }

            // Make sure the buffer is large enough to fit the entire data to read.
            // As well, we must copy over the buffered data we have not read yet back
            // to the start of the buffer, where we will begin reading from. Then,
            // we read the next data from the stream until the buffer is full again.
            int oldBytesRemaining = BytesRemaining;

            if (m_buffer.Length < dataSize)
            {
                byte[] newBuffer = new byte[dataSize];
                GCHandle newHandle = GCHandle.Alloc(newBuffer, GCHandleType.Pinned);

                Buffer.MemoryCopy(m_ptr.ToPointer(), newHandle.AddrOfPinnedObject().ToPointer(), oldBytesRemaining, oldBytesRemaining);

                m_handle.Free();

                Prepare(newBuffer, newHandle, 0);
            }
            else
            {
                Buffer.MemoryCopy(m_ptr.ToPointer(), m_bufStart.ToPointer(), oldBytesRemaining, oldBytesRemaining);

                Prepare(m_buffer, m_handle, 0);
            }

            // read in the stream's next contents
            int bytesRead = m_stream.Read(m_buffer, oldBytesRemaining, m_buffer.Length - oldBytesRemaining);

            // if there wasn't enough data left in the stream, report a buffer overrun
            if (checkOverrun && oldBytesRemaining + bytesRead < dataSize)
            {
                throw new Exception("Attempted read is larger than the stream's remaining contents!");
            }
        }

        /// <summary>
        /// Reads a single element from the buffer.
        /// </summary>
        /// <typeparam name="T">The type of element to read.</typeparam>
        public unsafe T Read<T>() where T : unmanaged
        {
            if (BytesRemaining < sizeof(T))
            {
                GetNextStreamedData(sizeof(T));
            }

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

                    // if the read overruns the buffer, try to read in more data
                    if (BytesRemaining < length)
                    {
                        GetNextStreamedData(length);
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
