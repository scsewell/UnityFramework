using System;

namespace Framework
{
    /// <summary>
    /// A four-character code.
    /// </summary>
    /// <remarks>
    /// A four-character code is a four byte code commonly used to identify data formats.
    /// </remarks>
    public struct FourCC : IEquatable<FourCC>
    {
        private int m_code;

        /// <summary>
        /// Create a FourCC from the given integer.
        /// </summary>
        /// <param name="code">FourCC code represented as an <c>int</c>. Character order is
        /// little endian. "ABCD" is stored with A in the highest order 8 bits and D in the
        /// lowest order 8 bits.</param>
        /// <remarks>
        /// This method does not actually verify whether the four characters in the code
        /// are printable.
        /// </remarks>
        public FourCC(int code)
        {
            m_code = code;
        }

        /// <summary>
        /// Create a FourCC from the given four characters.
        /// </summary>
        /// <param name="a">First character.</param>
        /// <param name="b">Second character.</param>
        /// <param name="c">Third character.</param>
        /// <param name="d">Fourth character.</param>
        public FourCC(char a, char b = ' ', char c = ' ', char d = ' ')
        {
            m_code = (a << 24) | (b << 16) | (c << 8) | d;
        }

        /// <summary>
        /// Create a FourCC from the given string.
        /// </summary>
        /// <param name="str">A string with four characters or less but with at least one character.</param>
        /// <exception cref="ArgumentException"><paramref name="str"/> is empty or has more than four characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="str"/> is <c>null</c>.</exception>
        public FourCC(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            var length = str.Length;
            if (length < 1 || length > 4)
            {
                throw new ArgumentException($"{nameof(FourCC)} string must be one to four characters long!", nameof(str));
            }

            var a = str[0];
            var b = length > 1 ? str[1] : ' ';
            var c = length > 2 ? str[2] : ' ';
            var d = length > 3 ? str[3] : ' ';

            m_code = (a << 24) | (b << 16) | (c << 8) | d;
        }

        /// <summary>
        /// Convert the FourCC into a string in the form of "ABCD".
        /// </summary>
        /// <returns>A string representation of the FourCC.</returns>
        public override string ToString()
        {
            var a = (char)(m_code >> 24);
            var b = (char)((m_code & 0xff0000) >> 16);
            var c = (char)((m_code & 0x00ff00) >> 8);
            var d = (char)(m_code & 0x0000ff);

            return new string(new[] { a, b, c, d });
        }

        public bool Equals(FourCC other)
        {
            return m_code == other.m_code;
        }

        public override bool Equals(object obj)
        {
            return obj is FourCC cc && Equals(cc);
        }

        public override int GetHashCode()
        {
            return m_code;
        }

        public static bool operator ==(FourCC left, FourCC right) => left.m_code == right.m_code;
        public static bool operator !=(FourCC left, FourCC right) => left.m_code != right.m_code;

        public static implicit operator int(FourCC fourCC) => fourCC.m_code;
        public static implicit operator FourCC(int i) => new FourCC(i);
    }
}
