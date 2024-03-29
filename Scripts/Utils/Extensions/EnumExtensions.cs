﻿using System;
using System.Reflection;

using UnityEngine.Scripting;

namespace Framework
{
    /// <summary>
    /// A class containing <see cref="Enum"/> extension methods.
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

        static class EnumMethods
        {
            [Preserve] static bool Contains(sbyte a, sbyte b) => (a & b) == b;
            [Preserve] static bool Contains(byte a, byte b) => (a & b) == b;
            [Preserve] static bool Contains(short a, short b) => (a & b) == b;
            [Preserve] static bool Contains(ushort a, ushort b) => (a & b) == b;
            [Preserve] static bool Contains(int a, int b) => (a & b) == b;
            [Preserve] static bool Contains(uint a, uint b) => (a & b) == b;
            [Preserve] static bool Contains(long a, long b) => (a & b) == b;
            [Preserve] static bool Contains(ulong a, ulong b) => (a & b) == b;

            [Preserve] static bool Intersects(sbyte a, sbyte b) => (a & b) != 0;
            [Preserve] static bool Intersects(byte a, byte b) => (a & b) != 0;
            [Preserve] static bool Intersects(short a, short b) => (a & b) != 0;
            [Preserve] static bool Intersects(ushort a, ushort b) => (a & b) != 0;
            [Preserve] static bool Intersects(int a, int b) => (a & b) != 0;
            [Preserve] static bool Intersects(uint a, uint b) => (a & b) != 0;
            [Preserve] static bool Intersects(long a, long b) => (a & b) != 0;
            [Preserve] static bool Intersects(ulong a, ulong b) => (a & b) != 0;
        }

        static class EnumHelper<T> where T : Enum
        {
            public static readonly Func<T, T, bool> containsFunc;
            public static readonly Func<T, T, bool> intersectsFunc;

            static EnumHelper()
            {
                containsFunc = InitFunction("Contains");
                intersectsFunc = InitFunction("Intersects");
            }

            static Func<T, T, bool> InitFunction(string functionName)
            {
                var type = typeof(T);

                if (!type.IsDefined(typeof(FlagsAttribute), false))
                {
                    throw new ArgumentException($"Enum {type.FullName} does not have flags attribute!");
                }

                var valueType = Enum.GetUnderlyingType(type);

                var method = typeof(EnumMethods).GetMethod(
                    functionName,
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[] { valueType, valueType }, 
                    null
                );

                if (method == null)
                {
                    throw new MissingMethodException($"Unknown enum value type {valueType}!");
                }

                return (Func<T, T, bool>)Delegate.CreateDelegate(typeof(Func<T, T, bool>), method);
            }
        }
    }
}
