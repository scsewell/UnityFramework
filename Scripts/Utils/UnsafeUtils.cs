namespace Framework
{
    /// <summary>
    /// Utilities for doing memory unsafe operations.
    /// </summary>
    public static class UnsafeUtils
    {
        /// <summary>
        /// Gets the size of a blittable type in bytes.
        /// </summary>
        /// <typeparam name="T">The type to get the size of.</typeparam>
        public static unsafe int SizeOf<T>() where T : unmanaged
        {
            return sizeof(T);
        }

        /// <summary>
        /// Gets the size of a blittable array in bytes.
        /// </summary>
        /// <typeparam name="T">The type to get the size of.</typeparam>
        /// <param name="array">The array to get the size of.</param>
        public static unsafe int LengthInBytes<T>(T[] array) where T : unmanaged
        {
            return array != null ? sizeof(T) * array.Length : 0;
        }
    }
}
