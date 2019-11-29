using System.Collections.Generic;

namespace NStuff.WindowSystem.ManualTest.Terrain
{
    internal class ChunkPool<T>
    {
        private readonly Dictionary<long, Chunk<T>> chunks = new Dictionary<long, Chunk<T>>();
        private readonly HashSet<Chunk<T>> allChunks = new HashSet<Chunk<T>>();
        private readonly HashSet<long> chunksInRange = new HashSet<long>();

        internal int X { get; set; }
        internal int Z { get; set; }
        internal int MaxDistance { get; set; }
        internal int Height { get; set; }

        internal List<Chunk<T>> InsideRange { get; } = new List<Chunk<T>>();
        internal List<Chunk<T>> Disposed { get; } = new List<Chunk<T>>();
        internal List<(int x, int y, int z)> Uninitialized { get; } = new List<(int x, int y, int z)>();

        internal void Update()
        {
            InsideRange.Clear();
            Disposed.Clear();
            Uninitialized.Clear();
            chunksInRange.Clear();

            int squaredMaxDistance = MaxDistance * MaxDistance;

            for (int x = X - MaxDistance; x <= X + MaxDistance; x++)
            {
                for (int z = Z - MaxDistance; z <= Z + MaxDistance; z++)
                {
                    int dx = X - x;
                    int dz = Z - z;
                    if (dx * dx + dz * dz <= squaredMaxDistance + 1)
                    {
                        for (int h = 0; h < Height; h++)
                        {
                            if (chunks.TryGetValue(Chunk<T>.GetId(x, h, z), out var c))
                            {
                                InsideRange.Add(c);
                                chunksInRange.Add(c.Id);
                            }
                            else
                            {
                                Uninitialized.Add((x, h, z));
                            }
                        }
                    }
                }
            }
            foreach (var c in allChunks)
            {
                if (chunksInRange.Contains(c.Id))
                {
                    continue;
                }
                int x = c.X - X;
                int z = c.Z - Z;
                if (x * x + z * z < squaredMaxDistance + 3)
                {
                    Disposed.Add(c);
                    chunks.Remove(c.Id);
                }
            }
        }

        internal void AddInsideRange(Chunk<T> chunk)
        {
            chunks.Add(chunk.Id, chunk);
            allChunks.Add(chunk);
            InsideRange.Add(chunk);
        }
    }
}
