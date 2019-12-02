#nullable disable
using System;

namespace NStuff.Tessellation
{
    internal class Vertex<TVertexData> : PriorityQueue<Vertex<TVertexData>>.Item, IComparable<Vertex<TVertexData>>
    {
        internal Vertex<TVertexData> Next { get; set; }
        internal Vertex<TVertexData> Previous { get; set; }
        internal HalfEdge<TVertexData> Edge { get; set; }
        internal TVertexData Data { get; set; }
        internal double X { get; set; }
        internal double Y { get; set; }

        public int CompareTo(Vertex<TVertexData> other)
        {
            if (X < other.X)
            {
                return -1;
            }
            if (X > other.X)
            {
                return 1;
            }
            if (Y == other.Y)
            {
                return 0;
            }
            return (Y < other.Y) ? -1 : 1;
        }

        public int CompareToTransposed(Vertex<TVertexData> other)
        {
            if (Y < other.Y)
            {
                return -1;
            }
            if (Y > other.Y)
            {
                return 1;
            }
            if (X == other.X)
            {
                return 0;
            }
            return (X < other.X) ? -1 : 1;
        }

        internal void Kill(Vertex<TVertexData> newOrigin)
        {
            var startEdge = Edge;
            var e = startEdge;
            do
            {
                e.OriginVertex = newOrigin;
                e = e.OriginNext;
            }
            while (e != startEdge);

            var previousVertex = Previous;
            var nextVertex = Next;
            nextVertex.Previous = previousVertex;
            previousVertex.Next = nextVertex;
        }
    }
}
