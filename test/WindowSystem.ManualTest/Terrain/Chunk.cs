using System;

namespace NStuff.WindowSystem.ManualTest.Terrain
{
    internal class Chunk<T>
    {
        internal const int XMin = unchecked((int)0x8000_0000) >> 4;
        internal const int XMax = 0x7FFF_FFFF;
        internal const int YMin = sbyte.MinValue;
        internal const int YMax = sbyte.MaxValue;
        internal const int ZMin = XMin;
        internal const int ZMax = XMax;

        private readonly T[,,] data = new T[16, 16, 16];
        private ulong coordinates;

        internal int Version { get; set; }

        internal int X => (int)((coordinates & 0xFFF_FFFFUL) << 4) >> 4;

        internal int Y => (byte)((coordinates >> 56) & 0xFFUL);

        internal int Z => (int)(((coordinates >> 28) & 0xFFF_FFFFUL) << 4) >> 4;

        internal static long GetId(int x, int y, int z)
        {
            if (x < XMin || x > XMax)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y < YMin || y > YMax)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
            if (z < ZMin || z > ZMax)
            {
                throw new ArgumentOutOfRangeException(nameof(z));
            }
            return (long)(((ulong)x & 0xFFF_FFFFUL) | (((ulong)z & 0xFFF_FFFFUL) << 28) | ((ulong)y << 56));
        }

        internal long Id => (long)coordinates;

        internal T this[int x, int y, int z] {
            get => data[x, y, z];
            set => data[x, y, z] = value;
        }

        internal void SetLocation(int x, int y, int z) => coordinates = (ulong)GetId(x, y, z);
    }
}
