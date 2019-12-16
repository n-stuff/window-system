using System;
using System.Collections.Generic;

using Xunit;

namespace NStuff.Text.Tests
{
    public class CodePointBufferTest
    {
        [Theory]
        [InlineData(1, 2, 3, 4, 5)]
        [InlineData(1000, 2000, 3000, 4000, 5000)]
        [InlineData(100000, 100001, 100002, 100003, 100004)]
        [InlineData(1, 1000, 100001, 2, 2000, 100002)]
        public void InsertTest(params int[] input)
        {
            var b = new CodePointBuffer(4);
            for (int i = 0; i < input.Length; i++)
            {
                b.Insert(i, input[i]);
            }

            Assert.Equal(input.Length, b.Count);
            for (int i = 0; i < input.Length; i++)
            {
                Assert.Equal(input[i], b[i]);
            }
        }

        [Fact]
        public void InsertTest2()
        {
            var l = new List<int>(4);
            var b = new CodePointBuffer(4);
            Fill(ref b, l, 1000, 0xFF);
            Fill(ref b, l, 1000, 0xFFFF);
            Fill(ref b, l, 1000, 0x10FFFF);

            Assert.Equal(l.Count, b.Count);
            for (int i = 0; i < l.Count; i++)
            {
                Assert.Equal(l[i], b[i]);
            }
        }

        private void Fill(ref CodePointBuffer b, List<int> l, int count, int range)
        {
            var rng = new Random(0);
            for (int i = 0; i < count; i++)
            {
                var n = rng.Next(i);
                var c = rng.Next(range + 1);
                l.Insert(n, c);
                b.Insert(n, c);
            }
        }

        [Fact]
        public void RemoveTest()
        {
            var b = new CodePointBuffer(4);
            b.InsertRange(0, new int[] { 1, 2000, 3, 4, 5, 6, 7, 8, 9 }, 0, 9);

            b.RemoveRange(3, 4);

            Assert.Equal(5, b.Count);
            Assert.Equal(1, b[0]);
            Assert.Equal(2000, b[1]);
            Assert.Equal(3, b[2]);
            Assert.Equal(8, b[3]);
            Assert.Equal(9, b[4]);
        }

        [Fact]
        public void RemoveTest2()
        {
            var b = new CodePointBuffer(4);
            b.InsertRange(0, new int[] { 1, 2, 3, 4, 6, 7, 80000, 9 }, 0, 8);
            b.Insert(4, 5);

            b.RemoveRange(3, 4);

            Assert.Equal(5, b.Count);
            Assert.Equal(1, b[0]);
            Assert.Equal(2, b[1]);
            Assert.Equal(3, b[2]);
            Assert.Equal(80000, b[3]);
            Assert.Equal(9, b[4]);
        }
    }
}
