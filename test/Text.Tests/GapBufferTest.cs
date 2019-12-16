using System;
using System.Collections.Generic;

using Xunit;

namespace NStuff.Text.Tests
{
    public class GapBufferTest
    {
        [Fact]
        public void InsertTest()
        {
            var gb = new GapBuffer<int>(4);
            gb.Insert(0, 1);

            Assert.Equal(1, gb.Count);
            Assert.Equal(1, gb[0]);
        }

        [Fact]
        public void InsertTest2()
        {
            var l = new List<int>(4);
            var gb = new GapBuffer<int>(4);
            Fill(gb, l, 1000);

            Assert.Equal(l.Count, gb.Count);
            for (int i = 0; i < l.Count; i++)
            {
                Assert.Equal(l[i], gb[i]);
            }
        }

        private void Fill(GapBuffer<int> gb, List<int> l, int count)
        {
            var rng = new Random(0);
            for (int i = 0; i < count; i++)
            {
                var n = rng.Next(i);
                l.Insert(n, i);
                gb.Insert(n, i);
            }
        }

        [Fact]
        public void EnlargeTest()
        {
            var gb = new GapBuffer<int>(2);
            gb.Insert(0, 3);
            gb.Insert(1, 4);
            gb.Insert(0, 1);
            gb.Insert(3, 5);
            gb.Insert(1, 2);

            Assert.Equal(5, gb.Count);
            Assert.Equal(1, gb[0]);
            Assert.Equal(2, gb[1]);
            Assert.Equal(3, gb[2]);
            Assert.Equal(4, gb[3]);
            Assert.Equal(5, gb[4]);
        }

        [Fact]
        public void RemoveTest()
        {
            var gb = new GapBuffer<int>(4);
            gb.InsertRange(0, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0, 9);

            gb.RemoveRange(3, 4);

            Assert.Equal(5, gb.Count);
            Assert.Equal(1, gb[0]);
            Assert.Equal(2, gb[1]);
            Assert.Equal(3, gb[2]);
            Assert.Equal(8, gb[3]);
            Assert.Equal(9, gb[4]);
        }

        [Fact]
        public void RemoveTest2()
        {
            var gb = new GapBuffer<int>(4);
            gb.InsertRange(0, new int[] { 1, 2, 3, 4, 6, 7, 8, 9 }, 0, 8);
            gb.Insert(4, 5);

            gb.RemoveRange(3, 4);

            Assert.Equal(5, gb.Count);
            Assert.Equal(1, gb[0]);
            Assert.Equal(2, gb[1]);
            Assert.Equal(3, gb[2]);
            Assert.Equal(8, gb[3]);
            Assert.Equal(9, gb[4]);
        }

        [Fact]
        public void InsertTest3()
        {
            var l = new List<int>(4);
            var gb = new GapBuffer<int>(4);
            Fill(gb, l, 1000);

            var rng = new Random(1);
            for (int i = 0; i < 500; i++)
            {
                var n = rng.Next(i);
                l.RemoveAt(n);
                gb.RemoveAt(n);
            }

            Assert.Equal(l.Count, gb.Count);
            for (int i = 0; i < l.Count; i++)
            {
                Assert.Equal(l[i], gb[i]);
            }
        }
    }
}
