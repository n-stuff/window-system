using System;

namespace NStuff.Tessellation
{
    /// <summary>
    /// Provides a common base class for tessellators.
    /// </summary>
    /// <typeparam name="TPolygonData">The type of data associated with a polygon.</typeparam>
    /// <typeparam name="TVertexData">The type of data associated with a vertex.</typeparam>
    /// <typeparam name="TVertexDataContainer">The type that contains the vertex data and tesselator specific data.</typeparam>
    public abstract class TessellatorBase<TPolygonData, TVertexData, TVertexDataContainer>
    {
        internal enum State
        {
            Dormant,
            InPolygon,
            InContour
        }

        internal State state;
        internal readonly ITessellateHandler<TPolygonData, TVertexData> handler;
        internal MeshBuilder<TPolygonData, TVertexDataContainer>? meshBuilder;
        internal TPolygonData polygonData = default!;

        /// <summary>
        /// The winding rule to determine whether a part of a polygon is inside of outside of this polygon.
        /// </summary>
        public WindingRule WindingRule { get; set; }

        internal TessellatorBase(ITessellateHandler<TPolygonData, TVertexData> handler) => this.handler = handler;

        internal void RequireState(State state)
        {
            if (this.state != state)
            {
                if (this.state < state)
                {
                    switch (this.state)
                    {
                        case State.Dormant:
                            throw new InvalidOperationException("Missing call to BeginPolygon()");
                        case State.InPolygon:
                            throw new InvalidOperationException("Missing call to BeginContour()");
                    }
                }
                else
                {
                    switch (this.state)
                    {
                        case State.InPolygon:
                            throw new InvalidOperationException("Missing call to EndPolygon()");
                        case State.InContour:
                            throw new InvalidOperationException("Missing call to EndContour()");
                    }
                }
            }
        }

        internal abstract void CallHandlerAddVertex(Vertex<TVertexDataContainer> vertexDataContainer);

        internal void CallHandlerForBoundary()
        {
            var faceHead = meshBuilder!.Mesh.FaceHead;
            for (var f = faceHead.Next; f != faceHead; f = f.Next)
            {
                if (f.Inside)
                {
                    handler.Begin(PrimitiveKind.LineLoop, polygonData);
                    var e = f.Edge;
                    do
                    {
                        var v = e.OriginVertex;
                        CallHandlerAddVertex(v);
                        e = e.LeftFaceNext;
                    }
                    while (e != f.Edge);
                    handler.End(polygonData);
                }
            }
        }

        internal void CallHandlerForTriangles()
        {
            var faceHead = meshBuilder!.Mesh.FaceHead;
            var firstFace = true;
            for (var f = faceHead.Next; f != faceHead; f = f.Next)
            {
                if (f.Inside)
                {
                    if (firstFace)
                    {
                        handler.Begin(PrimitiveKind.Triangles, polygonData);
                        firstFace = false;
                    }
                    var first = true;
                    var edgeFlag = false;
                    var e = f.Edge;
                    do
                    {
                        if (first)
                        {
                            first = false;
                            edgeFlag = !e.RightFace.Inside;
                            handler.FlagEdges(edgeFlag);
                        }
                        else
                        {
                            var newFlag = !e.RightFace.Inside;
                            if (edgeFlag != newFlag)
                            {
                                edgeFlag = newFlag;
                                handler.FlagEdges(edgeFlag);
                            }
                        }
                        var v = e.OriginVertex;
                        CallHandlerAddVertex(v);
                        e = e.LeftFaceNext;
                    }
                    while (e != f.Edge);
                }
            }
            if (!firstFace)
            {
                handler.End(polygonData);
            }
        }

        internal void ComputeStripsAndFans()
        {
            Face<TVertexDataContainer>? isolatedTriangles = null;
            var faceHead = meshBuilder!.Mesh.FaceHead;
            for (var f = faceHead.Next; f != faceHead; f = f.Next)
            {
                if (f.Inside && !f.Marked)
                {
                    ComputeMaximumFaceGroup(f, ref isolatedTriangles);
                }
            }
            if (isolatedTriangles != null)
            {
                handler.Begin(PrimitiveKind.Triangles, polygonData);
                for (; isolatedTriangles != null; isolatedTriangles = isolatedTriangles.Trail)
                {
                    var e = isolatedTriangles.Edge;
                    do
                    {
                        var v = e.OriginVertex;
                        CallHandlerAddVertex(v);
                        e = e.LeftFaceNext;
                    }
                    while (e != isolatedTriangles.Edge);
                }
                handler.End(polygonData);
            }
        }

        private void ComputeMaximumFaceGroup(Face<TVertexDataContainer> face, ref Face<TVertexDataContainer>? isolatedTriangles)
        {
            var e = face.Edge;
            (int size, HalfEdge<TVertexDataContainer> startEdge, PrimitiveKind primitiveKind) max = (1, e, PrimitiveKind.Triangles);
            var newFace = ComputeMaximumFan(e);
            if (newFace.size > max.size)
            {
                max = newFace;
            }
            newFace = ComputeMaximumFan(e.LeftFaceNext);
            if (newFace.size > max.size)
            {
                max = newFace;
            }
            newFace = ComputeMaximumFan(e.LeftFacePrevious);
            if (newFace.size > max.size)
            {
                max = newFace;
            }
            newFace = ComputeMaximumStrip(e);
            if (newFace.size > max.size)
            {
                max = newFace;
            }
            newFace = ComputeMaximumStrip(e.LeftFaceNext);
            if (newFace.size > max.size)
            {
                max = newFace;
            }
            newFace = ComputeMaximumStrip(e.LeftFacePrevious);
            if (newFace.size > max.size)
            {
                max = newFace;
            }

            switch (max.primitiveKind)
            {
                case PrimitiveKind.Triangles:
                    AddToTrail(max.startEdge.LeftFace, ref isolatedTriangles);
                    break;
                case PrimitiveKind.TriangleFan:
                    {
                        e = max.startEdge;
                        handler.Begin(PrimitiveKind.TriangleFan, polygonData);
                        var v = e.OriginVertex;
                        CallHandlerAddVertex(v);
                        v = e.DestinationVertex;
                        CallHandlerAddVertex(v);
                        while (e.LeftFace.Inside && !e.LeftFace.Marked)
                        {
                            e.LeftFace.Marked = true;
                            e = e.OriginNext;
                            v = e.DestinationVertex;
                            CallHandlerAddVertex(v);
                        }
                        handler.End(polygonData);
                    }
                    break;
                case PrimitiveKind.TriangleStrip:
                    {
                        e = max.startEdge;
                        handler.Begin(PrimitiveKind.TriangleStrip, polygonData);
                        var v = e.OriginVertex;
                        CallHandlerAddVertex(v);
                        v = e.DestinationVertex;
                        CallHandlerAddVertex(v);
                        while (e.LeftFace.Inside && !e.LeftFace.Marked)
                        {
                            e.LeftFace.Marked = true;
                            e = e.LeftFaceNext.Symmetric;
                            v = e.OriginVertex;
                            CallHandlerAddVertex(v);
                            if (!e.LeftFace.Inside || e.LeftFace.Marked)
                            {
                                break;
                            }
                            e.LeftFace.Marked = true;
                            e = e.OriginNext;
                            v = e.DestinationVertex;
                            CallHandlerAddVertex(v);
                        }
                        handler.End(polygonData);
                    }
                    break;
            }
        }

        private (int size, HalfEdge<TVertexDataContainer> startEdge, PrimitiveKind primitiveKind) ComputeMaximumFan(HalfEdge<TVertexDataContainer> edge)
        {
            Face<TVertexDataContainer>? trail = null;
            HalfEdge<TVertexDataContainer> e;
            var size = 0;
            for (e = edge; e.LeftFace.Inside && !e.LeftFace.Marked; e = e.OriginNext)
            {
                AddToTrail(e.LeftFace, ref trail);
                size++;
            }
            for (e = edge; e.RightFace.Inside && !e.RightFace.Marked; e = e.OriginPrevious)
            {
                AddToTrail(e.RightFace, ref trail);
                size++;
            }
            while (trail != null)
            {
                trail.Marked = false;
                trail = trail.Trail;
            }
            return (size, e, PrimitiveKind.TriangleFan);
        }

        private void AddToTrail(Face<TVertexDataContainer> face, ref Face<TVertexDataContainer>? trail)
        {
            face.Trail = trail;
            trail = face;
            face.Marked = true;
        }

        private (int size, HalfEdge<TVertexDataContainer> startEdge, PrimitiveKind primitiveKind) ComputeMaximumStrip(HalfEdge<TVertexDataContainer> edge)
        {
            var tailSize = 0;
            var headSize = 0;
            var trail = default(Face<TVertexDataContainer>);
            HalfEdge<TVertexDataContainer> e;
            HalfEdge<TVertexDataContainer> tailEdge;
            HalfEdge<TVertexDataContainer> headEdge;
            for (e = edge; e.LeftFace.Inside && !e.LeftFace.Marked; ++tailSize, e = e.OriginNext)
            {
                AddToTrail(e.LeftFace, ref trail);
                tailSize++;
                e = e.LeftFaceNext.Symmetric;
                if (!e.LeftFace.Inside || e.LeftFace.Marked)
                {
                    break;
                }
                AddToTrail(e.LeftFace, ref trail);
            }
            tailEdge = e;

            for (e = edge; e.RightFace.Inside && !e.RightFace.Marked; ++headSize, e = e.RightPrevious.Symmetric)
            {
                AddToTrail(e.RightFace, ref trail);
                headSize++;
                e = e.OriginPrevious;
                if (!e.RightFace.Inside || e.RightFace.Marked)
                {
                    break;
                }
                AddToTrail(e.RightFace, ref trail);
            }
            headEdge = e;

            var size = headSize + tailSize;
            HalfEdge<TVertexDataContainer> startEdge;
            if ((tailSize & 1) == 0)
            {
                startEdge = tailEdge.Symmetric;
            }
            else if ((headSize & 1) == 0)
            {
                startEdge = headEdge;
            }
            else
            {
                size--;
                startEdge = headEdge.OriginNext;
            }
            while (trail != null)
            {
                trail.Marked = false;
                trail = trail.Trail;
            }
            return (size, startEdge, PrimitiveKind.TriangleStrip);
        }
    }
}
