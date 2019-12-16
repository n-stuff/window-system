using Xunit;

namespace NStuff.Text.Tests
{
    public class MonospaceTextTest
    {
        [Theory]
        [InlineData("", 1)]
        [InlineData("a", 1)]
        [InlineData("abc", 1)]
        [InlineData("abcd", 2)]
        [InlineData("abc\nd", 2)]
        [InlineData("abcd\nefghi\n", 5)]
        [InlineData("abcd\nefghi\nj", 5)]
        public void GetFormattedLineCountTest(string text, int expectedLines)
        {
            var decoratedText = new DecoratedText<byte>();
            decoratedText.Insert(default, text);
            var monospaceText = new MonospaceText<byte>(decoratedText)
            {
                MaxLineLength = 3
            };
            Assert.Equal(expectedLines, monospaceText.GetFormattedLineCount());
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("a", 1)]
        [InlineData("abc", 3)]
        [InlineData("abcd", 3, 1)]
        [InlineData("abc\nd", 3, 1)]
        [InlineData("abcd\nefghi\n", 3, 1, 3, 2, 0)]
        [InlineData("abcd\nefghi\nj", 3, 1, 3, 2, 1)]
        public void GetFormattedColumnCount(string text, params int[] lineLengths)
        {
            var decoratedText = new DecoratedText<byte>();
            decoratedText.Insert(default, text);
            var monospaceText = new MonospaceText<byte>(decoratedText)
            {
                MaxLineLength = 3
            };
            for (int i = 0; i < lineLengths.Length; i++)
            {
                Assert.Equal(lineLengths[i], monospaceText.GetFormattedColumnCount(i));
            }
            Assert.Equal(lineLengths.Length, monospaceText.GetFormattedLineCount());
        }

        [Theory]
        [InlineData("", 0, 0, 0, 0)]
        [InlineData("a", 0, 0, 0, 0)]
        [InlineData("a", 0, 1, 0, 1)]
        [InlineData("abc", 0, 3, 0, 3)]
        [InlineData("abcd", 0, 3, 1, 0)]
        [InlineData("abc\nd", 0, 3, 0, 3)]
        [InlineData("abc\nd", 1, 0, 1, 0)]
        [InlineData("abcd\ne", 1, 0, 2, 0)]
        [InlineData("abcd\nefghi\nj", 2, 0, 4, 0)]
        [InlineData("abcd\r\nefghi\r\nj", 2, 0, 4, 0)]
        public void LocationFromTextTest(string text, int textLine, int textColumn, int expectedLine, int expectedColumn)
        {
            var decoratedText = new DecoratedText<byte>();
            decoratedText.Insert(default, text);
            var monospaceText = new MonospaceText<byte>(decoratedText)
            {
                MaxLineLength = 3
            };
            var (line, column) = monospaceText.LocationFromText((textLine, textColumn));
            Assert.Equal(expectedLine, line);
            Assert.Equal(expectedColumn, column);
        }

        [Theory]
        [InlineData("", 0, 0, 0, 0)]
        [InlineData("a", 0, 0, 0, 0)]
        [InlineData("a", 0, 1, 0, 1)]
        [InlineData("abc", 0, 3, 0, 3)]
        [InlineData("abcd", 1, 0, 0, 3)]
        [InlineData("abc\nd", 0, 3, 0, 3)]
        [InlineData("abc\nd", 1, 0, 1, 0)]
        [InlineData("abcd\ne", 2, 0, 1, 0)]
        [InlineData("abcd\nefghi\nj", 4, 0, 2, 0)]
        [InlineData("abcd\r\nefghi\r\nj", 4, 0, 2, 0)]
        public void LocationToTextTest(string text, int line, int column, int expectedLine, int expectedColumn)
        {
            var decoratedText = new DecoratedText<byte>();
            decoratedText.Insert(default, text);
            var monospaceText = new MonospaceText<byte>(decoratedText)
            {
                MaxLineLength = 3
            };
            var location = monospaceText.LocationToText((line, column));
            Assert.Equal(expectedLine, location.line);
            Assert.Equal(expectedColumn, location.column);
        }

        [Theory]
        [InlineData("", 0, 0, 'a', "a")]
        [InlineData("b", 0, 0, 'a', "ab")]
        [InlineData("acd", 0, 1, 'b', "abc", "d")]
        [InlineData("abcd", 0, 3, '\n', "abc", "d")]
        [InlineData("abcefg", 0, 3, 'd', "abc", "def", "g")]
        [InlineData("abcdef\n", 1, 0, 'g', "abc", "def", "g")]
        public void InsertTest(string text, int line, int column, int codePoint, params string[] formattedLines)
        {
            var decoratedText = new DecoratedText<byte>();
            decoratedText.Insert(default, text);
            var monospaceText = new MonospaceText<byte>(decoratedText)
            {
                MaxLineLength = 3
            };
            monospaceText.Insert((line, column), codePoint);
            Assert.Equal(formattedLines.Length, monospaceText.GetFormattedLineCount());
            for (int i = 0; i < formattedLines.Length; i++)
            {
                var l = formattedLines[i];
                Assert.Equal(l.Length, monospaceText.GetFormattedColumnCount(i));
                for (int j = 0; j < l.Length; j++)
                {
                    var location = monospaceText.LocationToText((i, j));
                    Assert.Equal((int)l[j], monospaceText[location].codePoint);
                }
            }
        }
    }
}
