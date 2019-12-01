namespace NStuff.Tessellation
{
    internal class HalfEdge<TVertexData>
    {
        internal HalfEdge<TVertexData> Next { get; set; }
        internal HalfEdge<TVertexData> Symmetric { get; set; }
        internal HalfEdge<TVertexData> OriginNext { get; set; }
        internal HalfEdge<TVertexData> LeftFaceNext { get; set; }
        internal Face<TVertexData> LeftFace { get; set; }
        internal ActiveRegion<TVertexData> ActiveRegion { get; set; }
        internal int Winding { get; set; }
        internal bool First { get; set; }
        internal Vertex<TVertexData> OriginVertex { get; set; }
        internal Vertex<TVertexData> DestinationVertex {
            get => Symmetric.OriginVertex;
            set => Symmetric.OriginVertex = value;
        }
        internal Face<TVertexData> RightFace {
            get => Symmetric.LeftFace;
            set => Symmetric.LeftFace = value;
        }
        internal HalfEdge<TVertexData> OriginPrevious => Symmetric.LeftFaceNext;
        internal HalfEdge<TVertexData> LeftFacePrevious => OriginNext.Symmetric;
        internal HalfEdge<TVertexData> RightPrevious => Symmetric.OriginNext;

        internal void AddWinding(HalfEdge<TVertexData> edge)
        {
            Winding += edge.Winding;
            Symmetric.Winding += edge.Symmetric.Winding;
        }

        internal void Splice(HalfEdge<TVertexData> other)
        {
            var originNext = OriginNext;
            var otherOriginNext = other.OriginNext;
            originNext.Symmetric.LeftFaceNext = other;
            otherOriginNext.Symmetric.LeftFaceNext = this;
            OriginNext = otherOriginNext;
            other.OriginNext = originNext;
        }

        internal void Kill()
        {
            var edge = this;
            if (!First)
            {
                edge = edge.Symmetric;
            }
            var nextEdge = edge.Next;
            var previousEdge = edge.Symmetric.Next;
            nextEdge.Symmetric.Next = previousEdge;
            previousEdge.Symmetric.Next = nextEdge;
        }
    }
}
