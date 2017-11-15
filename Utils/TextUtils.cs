using System.Text;
using UnityEngine;

namespace Framework
{
    public static class TextUtils
    {
        private static readonly char[] DIGITS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        
        public static StringBuilder Concat(this StringBuilder sb, int val, uint padAmount = 0, char padChar = '0', uint numBase = 10)
        {
            if (val < 0)
            {
                sb.Append('-');
                uint uintVal = uint.MaxValue - ((uint)val) + 1;
                return sb.Concat(uintVal, padAmount, padChar, numBase);
            }
            else
            {
                return sb.Concat((uint)val, padAmount, padChar, numBase);
            }
        }

        public static StringBuilder Concat(this StringBuilder sb, uint val, uint padAmount = 0, char padChar = '0', uint numBase = 10)
        {
            // Calculate length of integer when written out
            uint length = 0;
            uint lengthCalc = val;

            do
            {
                lengthCalc /= numBase;
                length++;
            }
            while (lengthCalc > 0);

            // Pad out space for writing.
            sb.Append(padChar, (int)Mathf.Max(padAmount, length));

            int strPos = sb.Length;

            // We're writing backwards, one character at a time.
            while (length > 0)
            {
                strPos--;

                // Lookup from static char array, to cover hex values too
                sb[strPos] = DIGITS[val % numBase];

                val /= numBase;
                length--;
            }
            return sb;
        }

        public static StringBuilder Concat(this StringBuilder sb, float val, uint decimalPlaces = 5, uint padAmount = 0, char padChar = '0')
        {
            if (decimalPlaces == 0)
            {
                return sb.Concat(Mathf.RoundToInt(val), padAmount, padChar, 10);
            }
            else
            {
                int intPart = (int)val;
                sb.Concat(intPart, padAmount, padChar, 10);
                sb.Append('.');
                
                float remainder = Mathf.Abs(val - intPart);
                
                do
                {
                    remainder *= 10;
                    decimalPlaces--;
                }
                while (decimalPlaces > 0);
                
                remainder += 0.5f;
                
                sb.Concat((uint)remainder, 0, '0', 10);
                return sb;
            }
        }
    }
}
