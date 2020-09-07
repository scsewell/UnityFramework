using System;

namespace Framework
{
    /// <summary>
    /// A class containing <see cref="Enum"/> extention methods.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Checks if this enum completely intersects with the given value.
        /// </summary>
        /// <remarks>
        /// This is a strongly typed implementation <see cref="Enum.HasFlag(Enum)"/> that avoids
        /// boxing and has better overall performance.
        /// </remarks>
        /// <typeparam name="T">An enum decorated with <see cref="FlagsAttribute"/>.</typeparam>
        /// <param name="a">An enum value.</param>
        /// <param name="b">The enum value to compare against.</param>
        /// <returns>True if <paramref name="b"/> completely intersects with this value.</returns>
        public static bool Contains<T>(this T a, T b) where T : Enum
        {
            return EnumHelper<T>.containsFunc(a, b);
        }

        /// <summary>
        /// Checks if this enum has any intersection with the given value.
        /// </summary>
        /// <typeparam name="T">An enum decorated with <see cref="FlagsAttribute"/>.</typeparam>
        /// <param name="a">An enum value.</param>
        /// <param name="b">The enum value to compare against.</param>
        /// <returns>True if <paramref name="b"/> has any intersection with this value.</returns>
        public static bool Intersects<T>(this T a, T b) where T : Enum
        {
            return EnumHelper<T>.intersectsFunc(a, b);
        }
    }

    internal static class EnumHelper<T>
    {
        public static bool Contains(sbyte a, sbyte b) => (a & b) == b;
        public static bool Contains(byte a, byte b) => (a & b) == b;
        public static bool Contains(short a, short b) => (a & b) == b;
        public static bool Contains(ushort a, ushort b) => (a & b) == b;
        public static bool Contains(int a, int b) => (a & b) == b;
        public static bool Contains(uint a, uint b) => (a & b) == b;
        public static bool Contains(long a, long b) => (a & b) == b;
        public static bool Contains(ulong a, ulong b) => (a & b) == b;

        public static bool Intersects(sbyte a, sbyte b) => (a & b) != 0;
        public static bool Intersects(byte a, byte b) => (a & b) != 0;
        public static bool Intersects(short a, short b) => (a & b) != 0;
        public static bool Intersects(ushort a, ushort b) => (a & b) != 0;
        public static bool Intersects(int a, int b) => (a & b) != 0;
        public static bool Intersects(uint a, uint b) => (a & b) != 0;
        public static bool Intersects(long a, long b) => (a & b) != 0;
        public static bool Intersects(ulong a, ulong b) => (a & b) != 0;

        public static readonly Func<T, T, bool> containsFunc;
        public static readonly Func<T, T, bool> intersectsFunc;

        static EnumHelper()
        {
            containsFunc = InitFunction(nameof(Contains));
            intersectsFunc = InitFunction(nameof(Intersects));
        }

        static Func<T, T, bool> InitFunction(string functionName)
        {
            var type = typeof(T);

            if (!type.IsEnum)
            {
                throw new ArgumentException($"Type {type.FullName} is not an enum!");
            }
            if (!type.IsDefined(typeof(FlagsAttribute), false))
            {
                throw new ArgumentException($"Enum {type.FullName} does not have flags attribute!");
            }

            var valueType = Enum.GetUnderlyingType(type);
            var parameterTypes = new[] { valueType, valueType };

            var method = typeof(EnumHelper<T>).GetMethod(functionName, parameterTypes);

            if (method == null)
            {
                throw new MissingMethodException($"Unknown enum value type {valueType}!");
            }

            return (Func<T, T, bool>)Delegate.CreateDelegate(typeof(Func<T, T, bool>), method);
        }
    }
}
