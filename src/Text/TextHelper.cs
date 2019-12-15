using System;
using System.IO;
using System.Text;

namespace NStuff.Text
{
    /// <summary>
    /// Provides utility methods to manipulate text.
    /// </summary>
    public static class TextHelper
    {
        /// <summary>
        /// Gets the code point of the unicode character at position <c>index</c> in the specified string.
        /// </summary>
        /// <param name="text">A string of characters.</param>
        /// <param name="index">A valid index in the string. It is updated to the index of the next character.</param>
        /// <param name="codePoint">The code point decoded from the string.</param>
        /// <returns><c>true</c> if the character was successfully decoded, <c>false</c> if the end of the string was reached.</returns>
        /// <exception cref="ArgumentException">If the string contains invalid unicode sequences.</exception>
        public static bool TryGetCodePoint(string text, ref int index, out int codePoint)
        {
            if (index >= text.Length)
            {
                codePoint = 0;
                return false;
            }
            var c = text[index++];
            if (char.IsHighSurrogate(c))
            {
                if (index >= text.Length)
                {
                    throw new ArgumentException(nameof(text));
                }
                var c2 = text[index++];
                if (!char.IsLowSurrogate(c2))
                {
                    throw new ArgumentException(nameof(text));
                }
                codePoint = char.ConvertToUtf32(c, c2);
                return true;
            }
            else if (char.IsLowSurrogate(c))
            {
                throw new ArgumentException(nameof(text));
            }
            else
            {
                codePoint = c;
                return true;
            }
        }

        /// <summary>
        /// Gets the code point of the unicode character at the current position in the specified reader.
        /// </summary>
        /// <param name="textReader">A text reader.</param>
        /// <param name="codePoint">The code point decoded from the reader.</param>
        /// <returns><c>true</c> if the character was successfully decoded, <c>false</c> if the end of the stream was reached.</returns>
        /// <exception cref="ArgumentException">If the reader contains invalid unicode sequences.</exception>
        public static bool TryGetCodePoint(TextReader textReader, out int codePoint)
        {
            var c = textReader.Read();
            if (c == -1)
            {
                codePoint = 0;
                return false;
            }
            if (char.IsHighSurrogate((char)c))
            {
                var c2 = textReader.Read();
                if (c2 == -1 || !char.IsLowSurrogate((char)c2))
                {
                    throw new ArgumentException(nameof(textReader));
                }
                codePoint = char.ConvertToUtf32((char)c, (char)c2);
                return true;
            }
            else if (char.IsLowSurrogate((char)c))
            {
                throw new ArgumentException(nameof(textReader));
            }
            else
            {
                codePoint = c;
                return true;
            }
        }

        /// <summary>
        /// Appends the 16-bit characters corresponding to the specified unicode code point to the given string builder. 
        /// </summary>
        /// <param name="builder">A string builder used to store the encoded characters.</param>
        /// <param name="codePoint">The code point to encode.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the code point is invalid.</exception>
        public static void AppendCodePoint(StringBuilder builder, int codePoint)
        {
            switch (ConvertToUtf16(codePoint, out var c1, out var c2))
            {
                case 0:
                    throw new ArgumentOutOfRangeException(nameof(codePoint));
                case 1:
                    builder.Append(c1);
                    break;
                case 2:
                    builder.Append(c1);
                    builder.Append(c2);
                    break;
            }
        }

        /// <summary>
        /// Encodes the specified code point to 16-bit characters.
        /// </summary>
        /// <param name="codePoint">The code point to encode.</param>
        /// <param name="c1">The first part of the UTF-16 encoded code point.</param>
        /// <param name="c2">The second part of the UTF-16 encoded code point, if any.</param>
        /// <returns><c>0</c> if the code point is out of range, <c>1</c> if the code point could be encoding with 16 bits,
        ///     <c>2</c> if the code point was encoded with 32 bits.</returns>
        public static int ConvertToUtf16(int codePoint, out char c1, out char c2)
        {
            if ((uint)codePoint > 0x10ffffu || (codePoint >= 0x00d800 && codePoint <= 0x00dfff))
            {
                c1 = '\0';
                c2 = '\0';
                return 0;
            }
            if (codePoint < 0x10000)
            {
                c1 = (char)codePoint;
                c2 = '\0';
                return 1;
            }
            else
            {
                c1 = (char)(codePoint / 0x400 + 0x00d800);
                c2 = (char)(codePoint % 0x400 + 0x00dc00);
                return 2;
            }
        }
    }
}
