using System.Text;

using Xunit;

namespace NStuff.Text.Tests
{
    public class PlainTextTest
    {
        [Theory]
        [InlineData("a", 1)]
        [InlineData("a\nb", 2)]
        [InlineData("a\r\nb", 2)]
        [InlineData("a\n\nb", 3)]
        [InlineData("ab\n\ncd", 3)]
        [InlineData("\r\n", 2)]
        [InlineData("\n", 2)]
        [InlineData("\r\n\r\n", 3)]
        public void InsertTest(string text, int lines)
        {
            var plainText = ConvertStringToPlainText(text);
            Assert.Equal(lines, plainText.LineCount);
            Assert.Equal(text, ConvertPlainTextToString(plainText));
        }

        [Theory]
        [InlineData("abc", 0, 1, 0, 2, "ac")]
        [InlineData("abc\ndef", 0, 1, 1, 2, "af")]
        [InlineData("abc\ndef\nghi", 0, 1, 2, 2, "ai")]
        [InlineData("abc\ndef\nghi", 0, 0, 2, 3, "")]
        [InlineData("abc\ndef\nghi\n", 0, 0, 3, 0, "")]
        public void RemoveRangeTest(string text, int startLine, int startColumn, int endLine, int endColumn, string result)
        {
            var plainText = ConvertStringToPlainText(text);
            plainText.RemoveRange((startLine, startColumn), (endLine, endColumn));
            Assert.Equal(result, ConvertPlainTextToString(plainText));
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("abcde")]
        [InlineData("abc\nde")]
        [InlineData("abcde\nfg")]
        [InlineData("abcde\nfg\n")]
        public void ReadTest(string text)
        {
            var plainText = ConvertStringToPlainText(text);
            var buffer = new char[4];
            var sb = new StringBuilder();
            var location = (0, 0);
            int read;
            while ((read = plainText.Read(buffer, 0, 4, ref location)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    sb.Append(buffer[i]);
                }
            }
            Assert.Equal(text, sb.ToString());
        }

        private static string ConvertPlainTextToString(PlainText plainText)
        {
            var builder = new StringBuilder();
            plainText.CopyTo(builder);
            return builder.ToString();
        }

        private static PlainText ConvertStringToPlainText(string text)
        {
            var plainText = new PlainText();
            plainText.Insert((0, 0), text);
            return plainText;
        }
    }
}
