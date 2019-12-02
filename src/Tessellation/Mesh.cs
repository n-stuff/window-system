#nullable disable
using static NStuff.Tessellation.GeometryHelper;

namespace NStuff.Tessellation
{
    internal class Mesh<TVertexData>
    {
        internal Vertex<TVertexData> VertexHead { get; }
        internal Face<TVertexData> FaceHead { get; }
        internal HalfEdge<TVertexData> EdgeHead { get; }

        public Mesh()
        {
            VertexHead = new Vertex<TVertexData>();
            FaceHead = new Face<TVertexData>();
            EdgeHead = new HalfEdge<TVertexData> { First = true };
            var symmetricHead = new HalfEdge<TVertexData>();

            VertexHead.Next = VertexHead.Previous = VertexHead;
            FaceHead.Next = FaceHead.Previous = FaceHead;
            EdgeHead.Next = EdgeHead;
            EdgeHead.Symmetric = symmetricHead;
            symmetricHead.Next = symmetricHead;
            symmetricHead.Symmetric = EdgeHead;
        }

        public HalfEdge<TVertexData> MakeEdge()
        {
            var result = MakeEdge(EdgeHead);
            MakeVertex(new Vertex<TVertexData>(), result, VertexHead);
            MakeVertex(new Vertex<TVertexData>(), result.Symmetric, VertexHead);
            MakeFace(new Face<TVertexData>(), result, FaceHead);
            return result;
        }

        public void Splice(HalfEdge<TVertexData> origin, HalfEdge<TVertexData> destination)
        {
            if (origin == destination)
            {
                return;
            }

            bool joiningVertices;
            if (destination.OriginVertex != origin.OriginVertex)
            {
                joiningVertices = true;
                destination.OriginVertex.Kill(origin.OriginVertex);
            }
            else
            {
                joiningVertices = false;
            }

            bool joiningLoops;
            if (destination.LeftFace != origin.LeftFace)
            {
                joiningLoops = true;
                destination.LeftFace.Kill(origin.LeftFace);
            }
            else
            {
                joiningLoops = false;
            }

            destination.Splice(origin);

            if (!joiningVertices)
            {
                MakeVertex(new Vertex<TVertexData>(), destination, origin.OriginVertex);
                origin.OriginVertex.Edge = origin;
            }
            if (!joiningLoops)
            {
                MakeFace(new Face<TVertexData>(), destination, origin.LeftFace);
                origin.LeftFace.Edge = origin;
            }
        }

        public void Delete(HalfEdge<TVertexData> edge)
        {
            var symmetric = edge.Symmetric;

            bool joiningLoops;
            if (edge.LeftFace != edge.RightFace)
            {
                joiningLoops = true;
                edge.LeftFace.Kill(edge.RightFace);
            }
            else
            {
                joiningLoops = false;
            }

            if (edge.OriginNext == edge)
            {
                edge.OriginVertex.Kill(null);
            }
            else
            {
                edge.RightFace.Edge = edge.OriginPrevious;
                edge.OriginVertex.Edge = edge.OriginNext;
                edge.Splice(edge.OriginPrevious);
                if (!joiningLoops)
                {
                    var newFace = new Face<TVertexData>();
                    MakeFace(newFace, edge, edge.LeftFace);
                }
            }

            if (symmetric.OriginNext == symmetric)
            {
                symmetric.OriginVertex.Kill(null);
                symmetric.LeftFace.Kill(null);
            }
            else
            {
                edge.LeftFace.Edge = symmetric.OriginPrevious;
                symmetric.OriginVertex.Edge = symmetric.OriginNext;
                symmetric.Splice(symmetric.OriginPrevious);
            }

            edge.Kill();
        }

        public HalfEdge<TVertexData> AddEdgeVertex(HalfEdge<TVertexData> origin)
        {
            var newEdge = MakeEdge(origin);
            var newSymmetric = newEdge.Symmetric;
            newEdge.Splice(origin.LeftFaceNext);
            newEdge.OriginVertex = origin.DestinationVertex;
            var newVertex = new Vertex<TVertexData>();
            MakeVertex(newVertex, newSymmetric, newEdge.OriginVertex);
            newEdge.LeftFace = newSymmetric.LeftFace = origin.LeftFace;
            return newEdge;
        }

        public HalfEdge<TVertexData> SplitEdge(HalfEdge<TVertexData> origin)
        {
            var newEdge = AddEdgeVertex(origin).Symmetric;
            origin.Symmetric.Splice(origin.Symmetric.OriginPrevious);
            origin.Symmetric.Splice(newEdge);
            origin.DestinationVertex = newEdge.OriginVertex;
            newEdge.DestinationVertex.Edge = newEdge.Symmetric;
            newEdge.RightFace = origin.RightFace;
            newEdge.Winding = origin.Winding;
            newEdge.Symmetric.Winding = origin.Symmetric.Winding;
            return newEdge;
        }

        public HalfEdge<TVertexData> Connect(HalfEdge<TVertexData> origin, HalfEdge<TVertexData> destination)
        {
            var newEdge = MakeEdge(origin);
            var newSymmetric = newEdge.Symmetric;
            var joiningLoops = false;
            if (destination.LeftFace != origin.LeftFace)
            {
                joiningLoops = true;
                destination.LeftFace.Kill(origin.LeftFace);
            }
            newEdge.Splice(origin.LeftFaceNext);
            newSymmetric.Splice(destination);
            newEdge.OriginVertex = origin.DestinationVertex;
            newSymmetric.OriginVertex = destination.OriginVertex;
            newEdge.LeftFace = newSymmetric.LeftFace = origin.LeftFace;
            origin.LeftFace.Edge = newSymmetric;
            if (!joiningLoops)
            {
                var newFace = new Face<TVertexData>();
                MakeFace(newFace, newEdge, origin.LeftFace);
            }
            return newEdge;
        }

        public void SetWinding(int value, bool keepOnlyBoundary)
        {
            HalfEdge<TVertexData> nextEdge;
            for (var e = EdgeHead.Next; e != EdgeHead; e = nextEdge)
            {
                nextEdge = e.Next;
                if (e.RightFace.Inside != e.LeftFace.Inside)
                {
                    e.Winding = (e.LeftFace.Inside) ? value : -value;
                }
                else
                {
                    if (!keepOnlyBoundary)
                    {
                        e.Winding = 0;
                    }
                    else
                    {
                        Delete(e);
                    }
                }
            }
        }

        public void RemoveDegenerateFaces()
        {
            for (var f = FaceHead.Next; f != FaceHead; f = f.Next)
            {
                var e = f.Edge;
                if (e.LeftFaceNext.LeftFaceNext == e)
                {
                    e.OriginNext.AddWinding(e);
                    Delete(e);
                }
            }
        }

        public void TessellateInterior()
        {
            for (var f = FaceHead.Next; f != FaceHead; f = f.Next)
            {
                if (f.Inside)
                {
                    TessellateRegion(f);
                }
            }
        }

        private void TessellateRegion(Face<TVertexData> face)
        {
            var upEdge = face.Edge;
            for (; upEdge.DestinationVertex.CompareTo(upEdge.OriginVertex) <= 0; upEdge = upEdge.LeftFacePrevious) ;
            for (; upEdge.OriginVertex.CompareTo(upEdge.DestinationVertex) <= 0; upEdge = upEdge.LeftFaceNext) ;
            var lowEdge = upEdge.LeftFacePrevious;
            while (upEdge.LeftFaceNext != lowEdge)
            {
                if (upEdge.DestinationVertex.CompareTo(lowEdge.OriginVertex) <= 0)
                {
                    while (lowEdge.LeftFaceNext != upEdge && (EdgeGoesLeft(lowEdge.LeftFaceNext)
                        || GetEdgeSign(lowEdge.OriginVertex, lowEdge.DestinationVertex, lowEdge.LeftFaceNext.DestinationVertex) <= 0))
                    {
                        lowEdge = Connect(lowEdge.LeftFaceNext, lowEdge).Symmetric;
                    }
                    lowEdge = lowEdge.LeftFacePrevious;
                }
                else
                {
                    while (lowEdge.LeftFaceNext != upEdge && (EdgeGoesRight(upEdge.LeftFacePrevious)
                        || GetEdgeSign(upEdge.DestinationVertex, upEdge.OriginVertex, upEdge.LeftFacePrevious.OriginVertex) >= 0))
                    {
                        upEdge = Connect(upEdge, upEdge.LeftFacePrevious).Symmetric;
                    }
                    upEdge = upEdge.LeftFaceNext;
                }
            }
            while (lowEdge.LeftFaceNext.LeftFaceNext != upEdge)
            {
                lowEdge = Connect(lowEdge.LeftFaceNext, lowEdge).Symmetric;
            }
        }

        private static HalfEdge<TVertexData> MakeEdge(HalfEdge<TVertexData> nextEdge)
        {
            var result = new HalfEdge<TVertexData> { First = true };
            var symmetric = new HalfEdge<TVertexData>();

            if (!nextEdge.First)
            {
                nextEdge = nextEdge.Symmetric;
            }

            var previousEdge = nextEdge.Symmetric.Next;
            symmetric.Next = previousEdge;
            previousEdge.Symmetric.Next = result;
            result.Next = nextEdge;
            nextEdge.Symmetric.Next = symmetric;

            result.Symmetric = symmetric;
            result.OriginNext = result;
            result.LeftFaceNext = symmetric;

            symmetric.Symmetric = result;
            symmetric.OriginNext = symmetric;
            symmetric.LeftFaceNext = result;

            return result;
        }

        private static void MakeVertex(Vertex<TVertexData> newVertex, HalfEdge<TVertexData> edgeOrigin, Vertex<TVertexData> nextVertex)
        {
            var previousVertex = nextVertex.Previous;
            newVertex.Previous = previousVertex;
            previousVertex.Next = newVertex;
            newVertex.Next = nextVertex;
            nextVertex.Previous = newVertex;

            newVertex.Edge = edgeOrigin;

            var e = edgeOrigin;
            do
            {
                e.OriginVertex = newVertex;
                e = e.OriginNext;
            }
            while (e != edgeOrigin);
        }

        private static void MakeFace(Face<TVertexData> newFace, HalfEdge<TVertexData> edgeOrigin, Face<TVertexData> nextFace)
        {
            var previousFace = nextFace.Previous;
            newFace.Previous = previousFace;
            previousFace.Next = newFace;
            newFace.Next = nextFace;
            nextFace.Previous = newFace;

            newFace.Edge = edgeOrigin;
            newFace.Inside = nextFace.Inside;

            var e = edgeOrigin;
            do
            {
                e.LeftFace = newFace;
                e = e.LeftFaceNext;
            }
            while (e != edgeOrigin);
        }
    }
}
