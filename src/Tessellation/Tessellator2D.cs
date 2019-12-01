using System;

namespace NStuff.Tessellation
{
    /// <summary>
    /// Provides methods to compute graphics primitives from 2D polygons.
    /// </summary>
    /// <typeparam name="TPolygonData">The type of data associated with a polygon.</typeparam>
    /// <typeparam name="TVertexData">The type of data associated with a vertex.</typeparam>
    public class Tessellator2D<TPolygonData, TVertexData> :
        TessellatorBase<TPolygonData, TVertexData, TVertexData>,
        IEdgeCombinator<TPolygonData, TVertexData>
    {
        private readonly SimpleTessellator2D<TVertexData> simpleTessellator = new SimpleTessellator2D<TVertexData>();
        private bool firstMove;
        private Face<TVertexData> currentFace;
        private HalfEdge<TVertexData> currentEdge;

        /// <summary>
        /// The kind of output expected from the tessellator.
        /// </summary>
        public OutputKind OutputKind {
            get => simpleTessellator.OutputKind;
            set => simpleTessellator.OutputKind = value;
        }

        /// <summary>
        /// A value indicating whether, in enumerator mode, the current vertex is the first vertex.
        /// </summary>
        public bool FirstVertex { get; private set; }

        /// <summary>
        /// A value indicating whether, in enumerator mode, the current vertex is located on the boundary of the polygon.
        /// </summary>
        public bool OnPolygonBoundary { get; private set; }

        /// <summary>
        /// Gets, in enumerator mode, the current vertex.
        /// </summary>
        public (double x, double y, TVertexData data) Vertex => simpleTessellator.Vertex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tessellator2D{TPolygonData, TVertexData}"/> class using
        /// the supplied <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">A handler providing callbacks to be called by this tessellator.</param>
        public Tessellator2D(ITessellateHandler<TPolygonData, TVertexData> handler) : base(handler)
        {
        }

        /// <summary>
        /// Notifies the tessellator that a polygon will be specified.
        /// </summary>
        /// <param name="data">An arbitrary data associated with the polygon.</param>
        public void BeginPolygon(TPolygonData data)
        {
            RequireState(State.Dormant);
            state = State.InPolygon;
            polygonData = data;
            meshBuilder = null;
            simpleTessellator.Count = 0;
        }

        /// <summary>
        /// Notifies the tessellator that a contour of a polygon will be specified.
        /// </summary>
        public void BeginContour()
        {
            RequireState(State.InPolygon);
            state = State.InContour;
            if (meshBuilder == null && simpleTessellator.Count > 0)
            {
                CreateMeshBuilder();
                meshBuilder.EndContour();
            }
            meshBuilder?.BeginContour();
        }

        /// <summary>
        /// Notifies the tessellator that a contour specification ended.
        /// </summary>
        public void EndContour()
        {
            RequireState(State.InContour);
            state = State.InPolygon;
            meshBuilder?.EndContour();
        }

        /// <summary>
        /// Adds a vertex to the current contour.
        /// </summary>
        /// <param name="x">The x-coordinate of the vertex.</param>
        /// <param name="y">The y-coordinate of the vertex.</param>
        /// <param name="data">The data to associate with this vertex.</param>
        public void AddVertex(double x, double y, TVertexData data)
        {
            RequireState(State.InContour);
            if (x < -MeshBuilder<TPolygonData, TVertexData>.MaxCoordinate || x > MeshBuilder<TPolygonData, TVertexData>.MaxCoordinate)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y < -MeshBuilder<TPolygonData, TVertexData>.MaxCoordinate || y > MeshBuilder<TPolygonData, TVertexData>.MaxCoordinate)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            if (meshBuilder == null)
            {
                if (simpleTessellator.AddVertex(x, y, data))
                {
                    return;
                }
                CreateMeshBuilder();
            }
            meshBuilder.AddVertex(x, y, data);
        }

        /// <summary>
        /// Notifies the tessellator that a polygon specification ended.
        /// </summary>
        public void EndPolygon()
        {
            RequireState(State.InPolygon);
            state = State.Dormant;
            if (meshBuilder == null)
            {
                if (simpleTessellator.Tessellate(handler, WindingRule, polygonData))
                {
                    polygonData = default;
                    return;
                }
                CreateMeshBuilder();
                meshBuilder.EndContour();
            }
            meshBuilder.EndPolygon();
            switch (OutputKind)
            {
                case OutputKind.BoundaryEnumerator:
                case OutputKind.TriangleEnumerator:
                    currentFace = meshBuilder.Mesh.FaceHead.Next;
                    currentEdge = null;
                    OnPolygonBoundary = true;
                    return;

                case OutputKind.BoundaryOnly:
                    CallHandlerForBoundary();
                    break;

                case OutputKind.TrianglesOnly:
                    CallHandlerForTriangles();
                    break;

                case OutputKind.AnyPrimitives:
                    ComputeStripsAndFans();
                    break;

                default:
                    throw new InvalidOperationException("Unhandled output kind: " + OutputKind);
            }
            meshBuilder = null;
            polygonData = default;
        }

        /// <summary>
        /// Advances to the next vertex, if any.
        /// </summary>
        /// <returns><c>true</c> if a vertex is available.</returns>
        public bool Move()
        {
            if (meshBuilder == null)
            {
                FirstVertex = firstMove;
                firstMove = false;
                return simpleTessellator.Move();
            }
            FirstVertex = false;
            for (;;)
            {
                if (currentEdge != null)
                {
                    OnPolygonBoundary = !currentEdge.RightFace.Inside;
                    var v = currentEdge.OriginVertex;
                    simpleTessellator.Vertex = (v.X, v.Y, v.Data);

                    currentEdge = currentEdge.LeftFaceNext;
                    if (currentEdge == currentFace.Edge)
                    {
                        currentEdge = null;
                        currentFace = currentFace.Next;
                    }
                    return true;
                }
                var faceHead = meshBuilder.Mesh.FaceHead;
                if (currentFace == faceHead)
                {
                    return false;
                }
                while (!currentFace.Inside)
                {
                    currentFace = currentFace.Next;
                    if (currentFace == faceHead)
                    {
                        return false;
                    }
                }
                if (firstMove || OutputKind == OutputKind.BoundaryEnumerator)
                {
                    firstMove = false;
                    FirstVertex = true;
                }
                currentEdge = currentFace.Edge;
            }
        }

        private void CreateMeshBuilder()
        {
            meshBuilder = new MeshBuilder<TPolygonData, TVertexData>(this)
            {
                OutputKind = OutputKind,
                WindingRule = WindingRule
            };
            meshBuilder.BeginPolygon(polygonData);
            meshBuilder.BeginContour();
            var count = simpleTessellator.Count;
            for (int i = 0; i < count; i++)
            {
                var (x, y, data) = simpleTessellator.Vertices[i];
                meshBuilder.AddVertex(x, y, data);
            }
        }

        TVertexData IEdgeCombinator<TPolygonData, TVertexData>.CombineEdges(double x, double y,
            (TVertexData data, double weight) origin1, (TVertexData data, double weight) destination1,
            (TVertexData data, double weight) origin2, (TVertexData data, double weight) destination2, TPolygonData polygonData)
        {
            return handler.CombineEdges(x, y, 0, origin1, destination1, origin2, destination2, polygonData);
        }

        internal override void CallHandlerAddVertex(Vertex<TVertexData> v) => handler.AddVertex(v.X, v.Y, 0, v.Data);
    }
}
