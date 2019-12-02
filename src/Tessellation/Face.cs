#nullable disable
namespace NStuff.Tessellation
{
    internal class Face<TVertexData>
    {
        internal Face<TVertexData> Next { get; set; }
        internal Face<TVertexData> Previous { get; set; }
        internal HalfEdge<TVertexData> Edge { get; set; }
        internal Face<TVertexData> Trail { get; set; }
        internal bool Marked { get; set; }
        internal bool Inside { get; set; }

        internal void Kill(Face<TVertexData> newLeftFace)
        {
            var startEdge = Edge;
            var current = startEdge;
            do
            {
                current.LeftFace = newLeftFace;
                current = current.LeftFaceNext;
            }
            while (current != startEdge);

            var previousFace = Previous;
            var nextFace = Next;
            nextFace.Previous = previousFace;
            previousFace.Next = nextFace;
        }
    }
}
