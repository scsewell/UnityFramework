using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Framework.IO
{
    /// <summary>
    /// Writes blittable types into a buffer.
    /// </summary>
    public class DataWriter : Disposable
    {
        private byte[] m_buffer;
        private GCHandle m_handle;
        private IntPtr m_bufStart;
        private IntPtr m_bufEnd;
        private IntPtr m_ptr;

        /// <summary>
        /// Gets if this data writer is working on a buffer of fixed size.
        /// </summary>
        public bool IsFixedSize { get; }

        /// <summary>
        /// The index of the current byte relative to the start offset in the buffer.
        /// </summary>
        public int BytesWritten => (int)(m_ptr.ToInt64() - m_bufStart.ToInt64());

        /// <summary>
        /// The number of bytes that are left until the end of the buffer.
        /// </summary>
        private int BytesRemaining => (int)(m_bufEnd.ToInt64() - m_ptr.ToInt64());

        /// <summary>
        /// Create a data writer on a fixed size buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to. Will be pinned until the writer is disposed.</param>
        /// <param name="offset">The byte array index to start writing from.</param>
        public DataWriter(byte[] buffer, int offset = 0)
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

            IsFixedSize = true;

            Prepare(buffer, GCHandle.Alloc(buffer, GCHandleType.Pinned), offset);
        }

        /// <summary>
        /// Create a data writer which manages a flexibly sized buffer.
        /// </summary>
        public DataWriter()
        {
            IsFixedSize = false;

            var buffer = new byte[1024];
            Prepare(buffer, GCHandle.Alloc(buffer, GCHandleType.Pinned), 0);
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
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

        private unsafe void GrowBuffer(int requiredSize)
        {
            // Only grow buffers owned by this writer. Instead report the overrun.
            if (IsFixedSize)
            {
                throw new Exception("Attempted write would overrun buffer!");
            }

            // create a new buffer with enough space to fit the contents
            var newSize = Math.Max(BytesWritten + requiredSize, 2 * m_buffer.Length);

            var newBuffer = new byte[newSize];
            var newHandle = GCHandle.Alloc(newBuffer, GCHandleType.Pinned);

            // copy over the old buffer contents
            var bytesWritten = BytesWritten;
            Buffer.MemoryCopy(m_bufStart.ToPointer(), newHandle.AddrOfPinnedObject().ToPointer(), bytesWritten, bytesWritten);

            // release the old buffer
            m_handle.Free();

            // prepare to continue writing to the new buffer
            Prepare(newBuffer, newHandle, bytesWritten);
        }

        /// <summary>
        /// Copies all the buffer contents to a new byte array.
        /// </summary>
        public unsafe byte[] GetBytes()
        {
            var buffer = new byte[BytesWritten];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                Buffer.MemoryCopy(m_bufStart.ToPointer(), handle.AddrOfPinnedObject().ToPointer(), buffer.Length, buffer.Length);
            }
            finally
            {
                handle.Free();
            }
            return buffer;
        }

        /// <summary>
        /// Copies all the buffer contents to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void CopyToStream(Stream stream)
        {
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream must be writable.", nameof(stream));
            }

            stream.Write(m_buffer, 0, BytesWritten);
        }

        /// <summary>
        /// Writes a single element to the buffer.
        /// </summary>
        /// <typeparam name="T">The type of element to write.</typeparam>
        /// <param name="value">The value to write.</param>
        public unsafe void Write<T>(T value) where T : unmanaged
        {
            // ensure there is enough space for the new content
            if (BytesRemaining < sizeof(T))
            {
                GrowBuffer(sizeof(T));
            }

            *((T*)m_ptr) = value;
            m_ptr += sizeof(T);
        }

        /// <summary>
        /// Writes the contents of an array to the buffer. More efficient with
        /// large arrays.
        /// </summary>
        /// <typeparam name="T">The type of element to write.</typeparam>
        /// <param name="array">The values to write.</param>
        public unsafe void Write<T>(T[] array) where T : unmanaged
        {
            var length = UnsafeUtils.LengthInBytes(array);

            if (length > 0)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);

                try
                {
                    // ensure there is enough space for the new content
                    if (BytesRemaining < length)
                    {
                        GrowBuffer(length);
                    }

                    // start from where we last finished writting
                    var src = handle.AddrOfPinnedObject();

                    // copy the memory using an optimal method
                    Buffer.MemoryCopy(src.ToPointer(), m_ptr.ToPointer(), BytesRemaining, length);
                    m_ptr += length;
                }
                finally
                {
                    // always free the handle, even if an exception occurs
                    handle.Free();
                }
            }
        }

        /// <summary>
        /// Writes a UTF-8 null terminated string to the buffer.
        /// </summary>
        /// <param name="value">The string to encode.</param>
        public void Write(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes.Length);
            Write(bytes);
        }
    }
}
