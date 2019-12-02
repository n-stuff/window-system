using System;

namespace NStuff.Tessellation
{
    /// <summary>
    /// Provides methods to compute graphics primitives from 3D polygons.
    /// </summary>
    /// <typeparam name="TPolygonData">The type of data associated with a polygon.</typeparam>
    /// <typeparam name="TVertexData">The type of data associated with a vertex.</typeparam>
    public class Tessellator3D<TPolygonData, TVertexData> :
        TessellatorBase<TPolygonData, TVertexData, Tessellator3D<TPolygonData, TVertexData>.VertexData>,
        IEdgeCombinator<TPolygonData, Tessellator3D<TPolygonData, TVertexData>.VertexData>
    {
        /// <summary>
        /// Used internally to store 3D coordinates along with actual data.
        /// </summary>
#pragma warning disable CA1034, CA1815
        public struct VertexData
#pragma warning restore CA1034, CA1815
        {
            internal TVertexData data;
            internal double x;
            internal double y;
            internal double z;
        }

        private readonly SimpleTessellator3D<TVertexData> simpleTessellator = new SimpleTessellator3D<TVertexData>();
        private bool firstMove;
        private Face<VertexData>? currentFace;
        private HalfEdge<VertexData>? currentEdge;

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
        public (double x, double y, double z, TVertexData data) Vertex => simpleTessellator.Vertex;

        /// <summary>
        /// The normal used to project the polygon in 2D to apply the tessellation algorithm.
        /// </summary>
        public (double x, double y, double z) Normal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tessellator3D{TPolygonData, TVertexData}"/> class
        /// using the provided <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">A handler defining callbacks to be called by this tessellator.</param>
        public Tessellator3D(ITessellateHandler<TPolygonData, TVertexData> handler) : base(handler)
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
                meshBuilder!.EndContour();
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
        /// <param name="z">The z-coordinate of the vertex.</param>
        /// <param name="data">The data to associate with this vertex.</param>
        public void AddVertex(double x, double y, double z, TVertexData data)
        {
            RequireState(State.InContour);
            if (x < -MeshBuilder<TPolygonData, VertexData>.MaxCoordinate || x > MeshBuilder<TPolygonData, VertexData>.MaxCoordinate)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y < -MeshBuilder<TPolygonData, VertexData>.MaxCoordinate || y > MeshBuilder<TPolygonData, VertexData>.MaxCoordinate)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
            if (z < -MeshBuilder<TPolygonData, VertexData>.MaxCoordinate || z > MeshBuilder<TPolygonData, VertexData>.MaxCoordinate)
            {
                throw new ArgumentOutOfRangeException(nameof(z));
            }

            if (meshBuilder == null)
            {
                if (simpleTessellator.AddVertex(x, y, z, data))
                {
                    return;
                }
                CreateMeshBuilder();
            }
            meshBuilder!.AddVertex(0, 0, new VertexData { data = data, x = x, y = y, z = z });
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
                if (simpleTessellator.Tessellate(handler, Normal, WindingRule, polygonData, OutputKind))
                {
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
                    polygonData = default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
                    return;
                }
                CreateMeshBuilder();
                meshBuilder!.EndContour();
            }
            ProjectPolygon();
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
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
            polygonData = default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
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
                    simpleTessellator.Vertex = (v.Data.x, v.Data.y, v.Data.z, v.Data.data);

                    currentEdge = currentEdge.LeftFaceNext;
                    if (currentEdge == currentFace!.Edge)
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
                while (!currentFace!.Inside)
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
            meshBuilder = new MeshBuilder<TPolygonData, VertexData>(this)
            {
                OutputKind = OutputKind,
                WindingRule = WindingRule
            };
            meshBuilder.BeginPolygon(polygonData);
            meshBuilder.BeginContour();
            var count = simpleTessellator.Count;
            for (int i = 0; i < count; i++)
            {
                var (x, y, z, data) = simpleTessellator.Vertices[i];
                meshBuilder.AddVertex(0, 0, new VertexData { data = data, x = x, y = y, z = z });
            }
        }

        VertexData IEdgeCombinator<TPolygonData, VertexData>.CombineEdges(double x, double y,
            (VertexData data, double weight) origin1, (VertexData data, double weight) destination1,
            (VertexData data, double weight) origin2, (VertexData data, double weight) destination2, TPolygonData polygonData)
        {
            var x3d = origin1.weight * origin1.data.x + destination1.weight * destination1.data.x;
            var y3d = origin1.weight * origin1.data.y + destination1.weight * destination1.data.y;
            var z3d = origin1.weight * origin1.data.z + destination1.weight * destination1.data.z;
            x3d += origin2.weight * origin2.data.x + destination2.weight * destination2.data.x;
            y3d += origin2.weight * origin2.data.y + destination2.weight * destination2.data.y;
            z3d += origin2.weight * origin2.data.z + destination2.weight * destination2.data.z;
            return new VertexData
            {
                data = handler.CombineEdges(x3d, y3d, z3d,
                    (origin1.data.data, origin1.weight), (destination1.data.data, destination1.weight),
                    (origin2.data.data, origin2.weight), (destination2.data.data, destination2.weight),
                    polygonData),
                x = x3d,
                y = y3d,
                z = z3d
            };
        }

        private void ProjectPolygon()
        {
            var normalComputed = false;
            var normal = Normal;
            if (normal.x == 0d && normal.y == 0 && normal.z == 0)
            {
                normal = ComputeNormal();
                normalComputed = true;
            }
            var xpux = 0d;
            var xpuy = 0d;
            var xpuz = 0d;
            var ypux = 0d;
            var ypuy = 0d;
            var ypuz = 0d;
            switch (GetLongAxis(normal.x, normal.y, normal.z))
            {
                case 0:
                    xpuy = 1;
                    ypuz = (normal.x > 0) ? 1 : -1;
                    break;
                case 1:
                    xpuz = 1;
                    ypux = (normal.y > 0) ? 1 : -1;
                    break;
                case 2:
                    xpux = 1;
                    ypuy = (normal.z > 0) ? 1 : -1;
                    break;
            }
            var vertexHead = meshBuilder!.Mesh.VertexHead;
            for (var v = vertexHead.Next; v != vertexHead; v = v.Next)
            {
                v.X = v.Data.x * xpux + v.Data.y * xpuy + v.Data.z * xpuz;
                v.Y = v.Data.x * ypux + v.Data.y * ypuy + v.Data.z * ypuz;
            }
            if (normalComputed)
            {
                CheckOrientation();
            }
        }

        private void CheckOrientation()
        {
            var area = 0d;
            var faceHead = meshBuilder!.Mesh.FaceHead;
            for (var f = faceHead.Next; f != faceHead; f = f.Next)
            {
                var e = f.Edge;
                if (e.Winding <= 0)
                {
                    continue;
                }
                do
                {
                    area += (e.OriginVertex.X - e.DestinationVertex.X) *
                        (e.OriginVertex.Y + e.DestinationVertex.Y);
                    e = e.LeftFaceNext;
                }
                while (e != f.Edge);
            }
            if (area < 0)
            {
                var vertexHead = meshBuilder.Mesh.VertexHead;
                for (var v = vertexHead.Next; v != vertexHead; v = v.Next)
                {
                    v.Y = -v.Y;
                }
            }
        }

        private (double x, double y, double z) ComputeNormal()
        {
            var vertexHead = meshBuilder!.Mesh.VertexHead;
            var xMin = 2 * MeshBuilder<TPolygonData, VertexData>.MaxCoordinate;
            var yMin = xMin;
            var zMin = xMin;
            var xMax = -2 * MeshBuilder<TPolygonData, VertexData>.MaxCoordinate;
            var yMax = xMax;
            var zMax = xMax;
            var minVertexX = default(Vertex<VertexData>);
            var minVertexY = minVertexX;
            var minVertexZ = minVertexX;
            var maxVertexX = minVertexX;
            var maxVertexY = minVertexX;
            var maxVertexZ = minVertexX;

            for (var v = vertexHead.Next; v != vertexHead; v = v.Next)
            {
                var c = v.Data.x;
                if (c < xMin)
                {
                    xMin = c;
                    minVertexX = v;
                }
                if (c > xMax)
                {
                    xMax = c;
                    maxVertexX = v;
                }
                c = v.Data.y;
                if (c < yMin)
                {
                    yMin = c;
                    minVertexY = v;
                }
                if (c > yMax)
                {
                    yMax = c;
                    maxVertexY = v;
                }
                c = v.Data.z;
                if (c < zMin)
                {
                    zMin = c;
                    minVertexZ = v;
                }
                if (c > zMax)
                {
                    zMax = c;
                    maxVertexZ = v;
                }
            }

            Vertex<VertexData> v1, v2;
            if (yMax - yMin > xMax - xMin)
            {
                if (zMax - zMin > yMax - yMin)
                {
                    if (zMin >= zMax)
                    {
                        return (0, 0, 0);
                    }
                    v1 = minVertexZ!;
                    v2 = maxVertexZ!;
                }
                else
                {
                    if (yMin >= yMax)
                    {
                        return (0, 0, 0);
                    }
                    v1 = minVertexY!;
                    v2 = maxVertexY!;
                }
            }
            else
            {
                if (zMax - zMin > xMax - xMin)
                {
                    if (zMin >= zMax)
                    {
                        return (0, 0, 0);
                    }
                    v1 = minVertexZ!;
                    v2 = maxVertexZ!;
                }
                else
                {
                    if (xMin >= xMax)
                    {
                        return (0, 0, 0);
                    }
                    v1 = minVertexX!;
                    v2 = maxVertexX!;
                }
            }

            var maxLen2 = 0d;
            var xNormal = 0d;
            var yNormal = 0d;
            var zNormal = 0d;
            var xd1 = v1.Data.x - v2.Data.x;
            var yd1 = v1.Data.y - v2.Data.y;
            var zd1 = v1.Data.z - v2.Data.z;
            double xd2;
            double yd2;
            double zd2;
            double xtn;
            double ytn;
            double ztn;
            for (var v = vertexHead.Next; v != vertexHead; v = v.Next)
            {
                xd2 = v.Data.x - v2.Data.x;
                yd2 = v.Data.y - v2.Data.y;
                zd2 = v.Data.z - v2.Data.z;
                xtn = yd1 * zd2 - zd1 * yd2;
                ytn = zd1 * xd2 - xd1 * zd2;
                ztn = xd1 * yd2 - yd1 * xd2;
                var tLen2 = xtn * xtn + ytn * xtn + ztn * ztn;
                if (tLen2 > maxLen2)
                {
                    maxLen2 = tLen2;
                    xNormal = xtn;
                    yNormal = ytn;
                    zNormal = ztn;
                }
            }

            if (maxLen2 <= 0d)
            {
                switch (GetLongAxis(xd1, yd1, zd1))
                {
                    case 0:
                        return (1d, 0d, 0d);
                    case 1:
                        return (0d, 1d, 0d);
                    case 2:
                        return (0d, 0d, 1d);
                }
            }
            return (xNormal, yNormal, zNormal);
        }

        private static int GetLongAxis(double x, double y, double z)
        {
            if (Math.Abs(y) > Math.Abs(x))
            {
                return (Math.Abs(z) > Math.Abs(y)) ? 2 : 1;
            }
            return (Math.Abs(z) > Math.Abs(x)) ? 2 : 0;
        }

        internal override void CallHandlerAddVertex(Vertex<VertexData> v) => handler.AddVertex(v.Data.x, v.Data.y, v.Data.z, v.Data.data);
    }
}
