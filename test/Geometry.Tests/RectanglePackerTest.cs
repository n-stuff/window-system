using System.Collections.Generic;
using Xunit;

namespace NStuff.Geometry.Tests
{
    public class RectanglePackerTest
    {
        [Fact]
        public void FullRectangle()
        {
            DoPack(1, 1, new (int, int)[] {
                (1, 1)
            }, 0);
        }

        [Fact]
        public void FourRectangles()
        {
            DoPack(2, 2, new (int, int)[] {
                (1, 1),
                (1, 1),
                (1, 1),
                (1, 1)
            }, 0);
        }

        [Fact]
        public void PackWasted()
        {
            DoPack(3, 3, new (int, int)[] {
                (1, 1),
                (1, 2),
                (1, 1),
                (3, 1),
                (1, 1),
                (1, 1)
            }, 0);
        }

        [Fact]
        public void PackWasted2()
        {
            DoPack(3, 4, new (int, int)[] {
                (1, 1),
                (1, 2),
                (1, 1),
                (3, 1),
                (1, 1),
                (1, 1)
            }, 3);
        }

        [Fact]
        public void PackWasted3()
        {
            DoPack(4, 4, new (int, int)[] {
                (2, 2),
                (3, 1),
                (1, 2)
            }, 7);
        }

        [Fact]
        public void PackWasted4()
        {
            DoPack(4, 4, new (int, int)[] {
                (2, 2),
                (3, 1),
                (1, 1),
                (1, 1)
            }, 7);
        }

        [Fact]
        public void PackWasted5()
        {
            DoPack(4, 4, new (int, int)[] {
                (2, 2),
                (3, 1),
                (1, 2),
                (4, 1)
            }, 3);
        }

        [Fact]
        public void PackWasted6()
        {
            DoPack(4, 4, new (int, int)[] {
                (2, 2),
                (3, 1),
                (1, 2),
                (4, 1),
                (1, 2)
            }, 1);
        }

        [Fact]
        public void PackWasted7()
        {
            DoPack(3, 3, new (int, int)[] {
                (1, 2),
                (3, 1),
                (1, 1),
                (1, 1),
                (2, 1)
            }, 0);
        }

        private void DoPack(int width, int height, (int width, int height)[] areas, int freeSpace)
        {
            var rectanglePacker = new RectanglePacker(width, height);
            var packedRectangles = new List<(int left, int top, int width, int height)>();
            foreach(var a in areas)
            {
                Assert.True(rectanglePacker.TryPackRectangle(a.width, a.height, out var left, out var top));
                packedRectangles.Add((left, top, a.width, a.height));
            }

            for (int i = 0; i < packedRectangles.Count; i++)
            {
                for (int j = 0; j < packedRectangles.Count; j++)
                {
                    if (i != j)
                    {
                        Assert.False(DoRectanglesOverlap(packedRectangles[i], packedRectangles[j]));
                    }
                }
            }

            var packedSurface = 0;
            foreach (var r in packedRectangles)
            {
                packedSurface += r.width * r.height;
            }
            var freeSurface = 0;
            foreach (var r in rectanglePacker.FreeRectangles)
            {
                freeSurface += r.width * r.height;
            }
            Assert.Equal(freeSpace, freeSurface);
            Assert.Equal(width * height, packedSurface + freeSurface);
        }

        private static bool DoRectanglesOverlap((int left, int top, int width, int height) r0, (int left, int top, int width, int height) r1)
        {
            bool xOverlap = IsInRange(r0.left, r1.left, r1.left + r1.width) ||
                            IsInRange(r1.left, r0.left, r0.left + r0.width);

            bool yOverlap = IsInRange(r0.top, r1.top, r1.top + r1.height) ||
                            IsInRange(r1.top, r0.top, r0.top + r0.height);

            return xOverlap && yOverlap;
        }

        private static bool IsInRange(int value, int min, int max) => value >= min && value < max;
    }
}
