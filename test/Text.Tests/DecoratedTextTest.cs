using Xunit;

namespace NStuff.Text.Test
{
    public class DecoratedTextTest
    {
        [Theory]
        [InlineData("a", 0, 0, 0, 1, 1)]
        [InlineData("ab", 0, 0, 0, 1, 1)]
        [InlineData("ab", 0, 1, 0, 2, 1)]
        [InlineData("abc", 0, 1, 0, 2, 1)]
        [InlineData("ab\nc", 0, 1, 1, 1, 1)]
        [InlineData("ab\ncd\ne", 0, 1, 1, 2, 1)]
        public void DecorateRangeTest(string text, int startLine, int startColumn, int endLine, int endColumn, byte decoration)
        {
            var decoratedText = ConvertStringToDecoratedText(text);
            decoratedText.DecorateRange(decoration, (startLine, startColumn), (endLine, endColumn));
            for (int i = 0; i < startColumn; i++)
            {
                Assert.Equal(0, decoratedText[(startLine, i)].decoration);
            }
            if (startLine == endLine)
            {
                for (int i = startColumn; i < endColumn; i++)
                {
                    Assert.Equal(decoration, decoratedText[(startLine, i)].decoration);
                }
            }
            else
            {
                for (int i = startColumn; i < decoratedText.GetColumnCount(startLine); i++)
                {
                    Assert.Equal(decoration, decoratedText[(startLine, i)].decoration);
                }
                for (int i = startLine + 1; i < endLine; i++)
                {
                    for (int j = 0; j < decoratedText.GetColumnCount(i); j++)
                    {
                        Assert.Equal(decoration, decoratedText[(i, j)].decoration);
                    }
                }
                for (int i = 0; i < endColumn; i++)
                {
                    Assert.Equal(decoration, decoratedText[(endLine, i)].decoration);
                }
            }
            for (int i = endColumn; i < decoratedText.GetColumnCount(endLine); i++)
            {
                Assert.Equal(0, decoratedText[(endLine, i)].decoration);
            }
        }

        private static DecoratedText<byte> ConvertStringToDecoratedText(string text)
        {
            var decoratedText = new DecoratedText<byte>();
            decoratedText.Insert((0, 0), text);
            return decoratedText;
        }
    }
}
