#nullable disable
using System;

using static NStuff.Tessellation.GeometryHelper;

namespace NStuff.Tessellation
{
    internal class MeshBuilder<TPolygonData, TVertexData>
    {
        internal const double MaxCoordinate = 1e150;
        private const double SentinelCoord = MaxCoordinate * 4;

        private readonly IEdgeCombinator<TPolygonData, TVertexData> edgeCombinator;
        private PriorityQueue<Vertex<TVertexData>> priorityQueue;
        private readonly ActiveRegion<TVertexData> regionHead = new ActiveRegion<TVertexData>();
        private Vertex<TVertexData> eventVertex;
        private TPolygonData polygonData;
        private HalfEdge<TVertexData> lastEdge;

        internal WindingRule WindingRule { get; set; }
        internal OutputKind OutputKind { get; set; }

        internal Mesh<TVertexData> Mesh { get; private set; }

        internal MeshBuilder(IEdgeCombinator<TPolygonData, TVertexData> edgeCombinator) => this.edgeCombinator = edgeCombinator;

        internal void BeginPolygon(TPolygonData data)
        {
            polygonData = data;
            Mesh = new Mesh<TVertexData>();
            lastEdge = null;
        }

        internal void BeginContour() => lastEdge = null;

        internal void EndContour()
        {
        }

        internal void AddVertex(double x, double y, TVertexData data)
        {
            var e = lastEdge;
            if (lastEdge == null)
            {
                e = Mesh.MakeEdge();
                Mesh.Splice(e, e.Symmetric);
            }
            else
            {
                Mesh.SplitEdge(e);
                e = e.LeftFaceNext;
            }
            lastEdge = e;
            e.Winding = 1;
            e.Symmetric.Winding = -1;

            var v = e.OriginVertex;
            v.Data = data;
            v.X = x;
            v.Y = y;
        }

        internal void EndPolygon()
        {
            ComputeInterior();
            if (OutputKind == OutputKind.BoundaryOnly)
            {
                Mesh.SetWinding(1, true);
            }
            else
            {
                Mesh.TessellateInterior();
            }
            polygonData = default;
        }

        private void InitializeActiveRegions()
        {
            regionHead.Above = regionHead;
            regionHead.Below = regionHead;
            AddSentinel(-SentinelCoord);
            AddSentinel(SentinelCoord);
        }

        private void AddSentinel(double value)
        {
            var edge = Mesh.MakeEdge();
            edge.OriginVertex.X = SentinelCoord;
            edge.OriginVertex.Y = value;
            edge.DestinationVertex.X = -SentinelCoord;
            edge.DestinationVertex.Y = value;
            eventVertex = edge.DestinationVertex;

            var region = new ActiveRegion<TVertexData> { UpperEdge = edge };
            InsertBefore(regionHead, region);
        }

        private void InsertBefore(ActiveRegion<TVertexData> node, ActiveRegion<TVertexData> newNode)
        {
            do
            {
                node = node.Below;
            }
            while (node != regionHead && !EdgeLessOrEqual(node.UpperEdge, newNode.UpperEdge));

            newNode.Above = node.Above;
            node.Above.Below = newNode;
            newNode.Below = node;
            node.Above = newNode;
        }

        private ActiveRegion<TVertexData> Search(HalfEdge<TVertexData> upperEdge)
        {
            var node = regionHead;
            do
            {
                node = node.Above;
            }
            while (node != regionHead && !EdgeLessOrEqual(upperEdge, node.UpperEdge));
            return (node == regionHead) ? null : node;
        }

        private bool EdgeLessOrEqual(HalfEdge<TVertexData> e1, HalfEdge<TVertexData> e2)
        {
            if (e1.DestinationVertex == eventVertex)
            {
                if (e2.DestinationVertex == eventVertex)
                {
                    if (e1.OriginVertex.CompareTo(e2.OriginVertex) <= 0)
                    {
                        return GetEdgeSign(e2.DestinationVertex, e1.OriginVertex, e2.OriginVertex) <= 0;
                    }
                    else
                    {
                        return GetEdgeSign(e1.DestinationVertex, e2.OriginVertex, e1.OriginVertex) >= 0;
                    }
                }
                else
                {
                    return GetEdgeSign(e2.DestinationVertex, eventVertex, e2.OriginVertex) <= 0;
                }
            }
            if (e2.DestinationVertex == eventVertex)
            {
                return GetEdgeSign(e1.DestinationVertex, eventVertex, e1.OriginVertex) >= 0;
            }

            var t1 = EvaluateEdge(e1.DestinationVertex, eventVertex, e1.OriginVertex);
            var t2 = EvaluateEdge(e2.DestinationVertex, eventVertex, e2.OriginVertex);
            return t1 >= t2;
        }

        private void ComputeInterior()
        {
            RemoveDegenerateEdges();
            InitializePriorityQueue();
            InitializeActiveRegions();
            while (priorityQueue.Count > 0)
            {
                var v = priorityQueue.Dequeue();
                while (priorityQueue.Count > 0)
                {
                    var nextVertex = priorityQueue.Peek();
                    if (nextVertex.CompareTo(v) != 0)
                    {
                        break;
                    }
                    priorityQueue.Dequeue();
                    SpliceMergeVertices(v.Edge, nextVertex.Edge);
                }
                SweepEvent(v);
            }

            priorityQueue = null;
            while (regionHead.Above != regionHead)
            {
                DeleteRegion(regionHead.Above);
            }

            Mesh.RemoveDegenerateFaces();
        }

        private void RemoveDegenerateEdges()
        {
            var headEdge = Mesh.EdgeHead;
            HalfEdge<TVertexData> nextEdge;
            HalfEdge<TVertexData> leftNext;
            for (var e = headEdge.Next; e != headEdge; e = nextEdge)
            {
                nextEdge = e.Next;
                leftNext = e.LeftFaceNext;

                if (e.OriginVertex.CompareTo(e.DestinationVertex) == 0 && e.LeftFaceNext.LeftFaceNext != e)
                {
                    SpliceMergeVertices(leftNext, e);
                    Mesh.Delete(e);
                    e = leftNext;
                    leftNext = e.LeftFaceNext;
                }
                if (leftNext.LeftFaceNext == e)
                {
                    if (leftNext != e)
                    {
                        if (leftNext == nextEdge || leftNext == nextEdge.Symmetric)
                        {
                            nextEdge = nextEdge.Next;
                        }
                        Mesh.Delete(leftNext);
                    }
                    if (e == nextEdge || e == nextEdge.Symmetric)
                    {
                        nextEdge = nextEdge.Next;
                    }
                    Mesh.Delete(e);
                }
            }
        }

        private void SpliceMergeVertices(HalfEdge<TVertexData> e1, HalfEdge<TVertexData> e2)
        {
            CallCombine(e1.OriginVertex, e1.OriginVertex.Data, e2.OriginVertex.Data, default, default, 0.5, 0.5, 0, 0);
            Mesh.Splice(e1, e2);
        }

        private void CallCombine(Vertex<TVertexData> intersection,
            TVertexData data1, TVertexData data2, TVertexData data3, TVertexData data4,
            double weight1, double weight2, double weight3, double weight4)
        {
            var data = edgeCombinator.CombineEdges(intersection.X, intersection.Y,
                (data1, weight1), (data2, weight2), (data3, weight3), (data4, weight4),
                polygonData);
            intersection.Data = data;
        }

        private void InitializePriorityQueue()
        {
            priorityQueue = new PriorityQueue<Vertex<TVertexData>>(16);
            var vertexHead = Mesh.VertexHead;
            for (var v = vertexHead.Next; v != vertexHead; v = v.Next)
            {
                priorityQueue.Enqueue(v);
            }
        }

        private void SweepEvent(Vertex<TVertexData> eventVertex)
        {
            this.eventVertex = eventVertex;
            var e = eventVertex.Edge;
            while (e.ActiveRegion == null)
            {
                e = e.OriginNext;
                if (e == eventVertex.Edge)
                {
                    ConnectLeftVertex(eventVertex);
                    return;
                }
            }
            var upRegion = GetTopLeftRegion(e.ActiveRegion);
            var region = upRegion.Below;
            var topLeft = region.UpperEdge;
            var bottomLeft = FinishLeftRegions(region, null);
            if (bottomLeft.OriginNext == topLeft)
            {
                ConnectRightVertex(upRegion, bottomLeft);
            }
            else
            {
                AddRightEdges(upRegion, bottomLeft.OriginNext, topLeft, topLeft, true);
            }
        }

        private void ConnectRightVertex(ActiveRegion<TVertexData> upRegion, HalfEdge<TVertexData> bottomLeft)
        {
            var topLeft = bottomLeft.OriginNext;
            var lowRegion = upRegion.Below;
            var upEdge = upRegion.UpperEdge;
            var lowEdge = lowRegion.UpperEdge;
            var degenerate = false;

            if (upEdge.DestinationVertex != lowEdge.DestinationVertex)
            {
                CheckForIntersect(upRegion);
            }
            if (upEdge.OriginVertex.CompareTo(eventVertex) == 0)
            {
                Mesh.Splice(topLeft.OriginPrevious, upEdge);
                upRegion = GetTopLeftRegion(upRegion);
                topLeft = upRegion.Below.UpperEdge;
                FinishLeftRegions(upRegion.Below, lowRegion);
                degenerate = true;
            }
            if (lowEdge.OriginVertex.CompareTo(eventVertex) == 0)
            {
                Mesh.Splice(bottomLeft, lowEdge.OriginPrevious);
                bottomLeft = FinishLeftRegions(lowRegion, null);
                degenerate = true;
            }
            if (degenerate)
            {
                AddRightEdges(upRegion, bottomLeft.OriginNext, topLeft, topLeft, true);
                return;
            }

            var newEdge = (lowEdge.OriginVertex.CompareTo(upEdge.OriginVertex) <= 0) ? lowEdge.OriginPrevious : upEdge;
            newEdge = Mesh.Connect(bottomLeft.LeftFacePrevious, newEdge);
            AddRightEdges(upRegion, newEdge, newEdge.OriginNext, newEdge.OriginNext, false);
            newEdge.Symmetric.ActiveRegion.FixUpperEdge = true;
            WalkDirtyRegions(upRegion);
        }

        private void ConnectLeftVertex(Vertex<TVertexData> eventVertex)
        {
            var upRegion = Search(eventVertex.Edge.Symmetric);
            var lowRegion = upRegion.Below;
            var upEdge = upRegion.UpperEdge;
            var lowEdge = lowRegion.UpperEdge;

            if (GetEdgeSign(upEdge.DestinationVertex, eventVertex, upEdge.OriginVertex) == 0)
            {
                ConnectLeftDegenerate(upRegion, eventVertex);
                return;
            }
            var region = (lowEdge.DestinationVertex.CompareTo(upEdge.DestinationVertex) <= 0) ? upRegion : lowRegion;
            if (upRegion.Inside || region.FixUpperEdge)
            {
                HalfEdge<TVertexData> newEdge;
                if (region == upRegion)
                {
                    newEdge = Mesh.Connect(eventVertex.Edge.Symmetric, upEdge.LeftFaceNext);
                }
                else
                {
                    var t = Mesh.Connect(lowEdge.RightPrevious.Symmetric, eventVertex.Edge);
                    newEdge = t.Symmetric;
                }
                if (region.FixUpperEdge)
                {
                    FixUpperEdge(region, newEdge);
                }
                else
                {
                    ComputeWinding(AddRegionBelow(upRegion, newEdge));
                }
                SweepEvent(eventVertex);
            }
            else
            {
                AddRightEdges(upRegion, eventVertex.Edge, eventVertex.Edge, null, true);
            }
        }

        private void ComputeWinding(ActiveRegion<TVertexData> region)
        {
            region.Winding = region.Above.Winding + region.UpperEdge.Winding;
            region.Inside = WindingInside(region.Winding);
        }

        private void ConnectLeftDegenerate(ActiveRegion<TVertexData> upRegion, Vertex<TVertexData> eventVertex)
        {
            var e = upRegion.UpperEdge;
            if (e.OriginVertex.CompareTo(eventVertex) == 0)
            {
                SpliceMergeVertices(e, eventVertex.Edge);
                return;
            }
            if (e.DestinationVertex.CompareTo(eventVertex) != 0)
            {
                Mesh.SplitEdge(e.Symmetric);
                if (upRegion.FixUpperEdge)
                {
                    Mesh.Delete(e.OriginNext);
                    upRegion.FixUpperEdge = false;
                }
                Mesh.Splice(eventVertex.Edge, e);
                SweepEvent(eventVertex);
                return;
            }
            upRegion = upRegion.TopRight;
            var region = upRegion.Below;
            var topRight = region.UpperEdge.Symmetric;
            var topLeft = topRight.OriginNext;
            var lastEdge = topLeft;
            if (region.FixUpperEdge)
            {
                DeleteRegion(region);
                Mesh.Delete(topRight);
                topRight = topLeft.OriginPrevious;
            }
            Mesh.Splice(eventVertex.Edge, topRight);
            if (!EdgeGoesLeft(topLeft))
            {
                topLeft = null;
            }
            AddRightEdges(upRegion, topRight.OriginNext, lastEdge, topLeft, true);
        }

        private void DeleteRegion(ActiveRegion<TVertexData> region)
        {
            region.UpperEdge.ActiveRegion = null;
            region.Delete();
        }

        private void AddRightEdges(ActiveRegion<TVertexData> upRegion, HalfEdge<TVertexData> firstEdge,
            HalfEdge<TVertexData> lastEdge, HalfEdge<TVertexData> topLeft, bool cleanup)
        {
            var e = firstEdge;
            do
            {
                AddRegionBelow(upRegion, e.Symmetric);
                e = e.OriginNext;
            }
            while (e != lastEdge);
            if (topLeft == null)
            {
                topLeft = upRegion.Below.UpperEdge.RightPrevious;
            }
            var previousRegion = upRegion;
            var previousEdge = topLeft;
            var firstTime = true;
            for (;;)
            {
                var region = previousRegion.Below;
                e = region.UpperEdge.Symmetric;
                if (e.OriginVertex != previousEdge.OriginVertex)
                {
                    break;
                }
                if (e.OriginNext != previousEdge)
                {
                    Mesh.Splice(e.OriginPrevious, e);
                    Mesh.Splice(previousEdge.OriginPrevious, e);
                }
                region.Winding = previousRegion.Winding - e.Winding;
                region.Inside = WindingInside(region.Winding);
                previousRegion.Dirty = true;
                if (!firstTime && CheckForRightSplice(previousRegion))
                {
                    e.AddWinding(previousEdge);
                    DeleteRegion(previousRegion);
                    Mesh.Delete(previousEdge);
                }
                firstTime = false;
                previousRegion = region;
                previousEdge = e;
            }
            previousRegion.Dirty = true;
            if (cleanup)
            {
                WalkDirtyRegions(previousRegion);
            }
        }

        private ActiveRegion<TVertexData> AddRegionBelow(ActiveRegion<TVertexData> regionAbove, HalfEdge<TVertexData> newUp)
        {
            var newRegion = new ActiveRegion<TVertexData> { UpperEdge = newUp };
            InsertBefore(regionAbove, newRegion);
            newUp.ActiveRegion = newRegion;
            return newRegion;
        }

        private bool WindingInside(int n)
        {
            return WindingRule switch
            {
                WindingRule.Odd => (n & 1) != 0,
                WindingRule.NonZero => n != 0,
                WindingRule.Positive => n > 0,
                WindingRule.Negative => n < 0,
                WindingRule.AbsGreaterOrEqual2 => Math.Abs(n) >= 2,
                _ => throw new InvalidOperationException("Unhandled winding rule: " + WindingRule),
            };
        }

        private bool CheckForRightSplice(ActiveRegion<TVertexData> upRegion)
        {
            var lowRegion = upRegion.Below;
            var upEdge = upRegion.UpperEdge;
            var lowEdge = lowRegion.UpperEdge;
            if (upEdge.OriginVertex.CompareTo(lowEdge.OriginVertex) <= 0)
            {
                if (GetEdgeSign(lowEdge.DestinationVertex, upEdge.OriginVertex, lowEdge.OriginVertex) > 0)
                {
                    return false;
                }
                if (upEdge.OriginVertex.CompareTo(lowEdge.OriginVertex) != 0)
                {
                    Mesh.SplitEdge(lowEdge.Symmetric);
                    Mesh.Splice(upEdge, lowEdge.OriginPrevious);
                    upRegion.Dirty = lowRegion.Dirty = true;
                }
                else if (upEdge.OriginVertex != lowEdge.OriginVertex)
                {
                    priorityQueue.Remove(upEdge.OriginVertex);
                    SpliceMergeVertices(lowEdge.OriginPrevious, upEdge);
                }
            }
            else
            {
                if (GetEdgeSign(upEdge.DestinationVertex, lowEdge.OriginVertex, upEdge.OriginVertex) < 0)
                {
                    return false;
                }
                upRegion.Above.Dirty = upRegion.Dirty = true;
                Mesh.SplitEdge(upEdge.Symmetric);
                Mesh.Splice(lowEdge.OriginPrevious, upEdge);
            }
            return true;
        }

        private bool CheckForLeftSplice(ActiveRegion<TVertexData> upRegion)
        {
            var lowRegion = upRegion.Below;
            var upEdge = upRegion.UpperEdge;
            var lowEdge = lowRegion.UpperEdge;
            if (upEdge.DestinationVertex.CompareTo(lowEdge.DestinationVertex) <= 0)
            {
                if (GetEdgeSign(upEdge.DestinationVertex, lowEdge.DestinationVertex, upEdge.OriginVertex) < 0)
                {
                    return false;
                }
                upRegion.Above.Dirty = upRegion.Dirty = true;
                var e = Mesh.SplitEdge(upEdge);
                Mesh.Splice(lowEdge.Symmetric, e);
                e.LeftFace.Inside = upRegion.Inside;
            }
            else
            {
                if (GetEdgeSign(lowEdge.DestinationVertex, upEdge.DestinationVertex, lowEdge.OriginVertex) > 0)
                {
                    return false;
                }
                upRegion.Dirty = lowRegion.Dirty = true;
                var e = Mesh.SplitEdge(lowEdge);
                Mesh.Splice(upEdge.LeftFaceNext, lowEdge.Symmetric);
                e.RightFace.Inside = upRegion.Inside;
            }
            return true;
        }

        private readonly Vertex<TVertexData> CheckForIntersectTemporaryVertex = new Vertex<TVertexData>();

        private bool CheckForIntersect(ActiveRegion<TVertexData> upRegion)
        {
            var lowRegion = upRegion.Below;
            var upEdge = upRegion.UpperEdge;
            var lowEdge = lowRegion.UpperEdge;
            var upOrigin = upEdge.OriginVertex;
            var lowOrigin = lowEdge.OriginVertex;

            if (upOrigin == lowOrigin)
            {
                return false;
            }

            var upDestination = upEdge.DestinationVertex;
            var lowDestination = lowEdge.DestinationVertex;
            var yMinUp = Math.Min(upOrigin.Y, upDestination.Y);
            var yMaxLow = Math.Max(lowOrigin.Y, lowDestination.Y);
            if (yMinUp > yMaxLow)
            {
                return false;
            }

            if (upOrigin.CompareTo(lowOrigin) <= 0)
            {
                if (GetEdgeSign(lowDestination, upOrigin, lowOrigin) > 0)
                {
                    return false;
                }
            }
            else
            {
                if (GetEdgeSign(upDestination, lowOrigin, upOrigin) < 0)
                {
                    return false;
                }
            }

            var intersection = CheckForIntersectTemporaryVertex;
            IntersectEdges(upDestination, upOrigin, lowDestination, lowOrigin, intersection);

            var eventVertex = this.eventVertex;
            if (intersection.CompareTo(eventVertex) <= 0)
            {
                intersection.X = eventVertex.X;
                intersection.Y = eventVertex.Y;
            }
            var minOrigin = (upOrigin.CompareTo(lowOrigin) <= 0) ? upOrigin : lowOrigin;
            if (minOrigin.CompareTo(intersection) <= 0)
            {
                intersection.X = minOrigin.X;
                intersection.Y = minOrigin.Y;
            }

            if (intersection.CompareTo(upOrigin) == 0 || intersection.CompareTo(lowOrigin) == 0)
            {
                CheckForRightSplice(upRegion);
                return false;
            }

            if ((upDestination.CompareTo(eventVertex) != 0 && GetEdgeSign(upDestination, eventVertex, intersection) >= 0) ||
                (lowDestination.CompareTo(eventVertex) != 0 && GetEdgeSign(lowDestination, eventVertex, intersection) <= 0))
            {
                if (lowDestination == eventVertex)
                {
                    Mesh.SplitEdge(upEdge.Symmetric);
                    Mesh.Splice(lowEdge.Symmetric, upEdge);
                    upRegion = GetTopLeftRegion(upRegion);
                    upEdge = upRegion.Below.UpperEdge;
                    FinishLeftRegions(upRegion.Below, lowRegion);
                    AddRightEdges(upRegion, upEdge.OriginPrevious, upEdge, upEdge, true);
                    return true;
                }
                if (upDestination == eventVertex)
                {
                    Mesh.SplitEdge(lowEdge.Symmetric);
                    Mesh.Splice(upEdge.LeftFaceNext, lowEdge.OriginPrevious);
                    lowRegion = upRegion;
                    upRegion = upRegion.TopRight;
                    var e = upRegion.Below.UpperEdge.RightPrevious;
                    lowRegion.UpperEdge = lowEdge.OriginPrevious;
                    lowEdge = FinishLeftRegions(lowRegion, null);
                    AddRightEdges(upRegion, lowEdge.OriginNext, upEdge.RightPrevious, e, true);
                    return true;
                }
                if (GetEdgeSign(upDestination, eventVertex, intersection) >= 0)
                {
                    upRegion.Above.Dirty = upRegion.Dirty = true;
                    Mesh.SplitEdge(upEdge.Symmetric);
                    upEdge.OriginVertex.X = eventVertex.X;
                    upEdge.OriginVertex.Y = eventVertex.Y;
                }
                if (GetEdgeSign(lowDestination, eventVertex, intersection) <= 0)
                {
                    upRegion.Dirty = lowRegion.Dirty = true;
                    Mesh.SplitEdge(lowEdge.Symmetric);
                    lowEdge.OriginVertex.X = eventVertex.X;
                    lowEdge.OriginVertex.Y = eventVertex.Y;
                }
                return false;
            }

            Mesh.SplitEdge(upEdge.Symmetric);
            Mesh.SplitEdge(lowEdge.Symmetric);
            Mesh.Splice(lowEdge.OriginPrevious, upEdge);
            upEdge.OriginVertex.X = intersection.X;
            upEdge.OriginVertex.Y = intersection.Y;
            priorityQueue.Enqueue(upEdge.OriginVertex);
            GetIntersectData(upEdge.OriginVertex, upOrigin, upDestination, lowOrigin, lowDestination);
            upRegion.Above.Dirty = upRegion.Dirty = lowRegion.Dirty = true;
            return false;
        }

        private void GetIntersectData(Vertex<TVertexData> intersection,
            Vertex<TVertexData> upOrigin, Vertex<TVertexData> upDestination,
            Vertex<TVertexData> lowOrigin, Vertex<TVertexData> lowDestination)
        {
            var (weight1, weight2) = GetVertexWeights(intersection, upOrigin, upDestination);
            var (weight3, weight4) = GetVertexWeights(intersection, lowOrigin, lowDestination);
            CallCombine(intersection, upOrigin.Data, upDestination.Data, lowOrigin.Data, lowDestination.Data,
                weight1, weight2, weight3, weight4);
        }

        private (double, double) GetVertexWeights(Vertex<TVertexData> intersection, Vertex<TVertexData> origin, Vertex<TVertexData> destination)
        {
            var y1 = GetL1Distance(origin, intersection);
            var y2 = GetL1Distance(destination, intersection);
            return (0.5 * y2 / (y1 + y2), 0.5 * y1 / (y1 + y2));
        }

        private ActiveRegion<TVertexData> GetTopLeftRegion(ActiveRegion<TVertexData> region)
        {
            region = region.TopLeft;
            if (region.FixUpperEdge)
            {
                var e = Mesh.Connect(region.Below.UpperEdge.Symmetric, region.UpperEdge.LeftFaceNext);
                if (e == null)
                {
                    return null;
                }
                FixUpperEdge(region, e);
                region = region.Above;
            }
            return region;
        }

        private void FixUpperEdge(ActiveRegion<TVertexData> region, HalfEdge<TVertexData> newEdge)
        {
            Mesh.Delete(region.UpperEdge);
            region.FixUpperEdge = false;
            region.UpperEdge = newEdge;
            newEdge.ActiveRegion = region;
        }

        private void WalkDirtyRegions(ActiveRegion<TVertexData> upRegion)
        {
            var lowRegion = upRegion.Below;
            for (;;)
            {
                while (lowRegion.Dirty)
                {
                    upRegion = lowRegion;
                    lowRegion = lowRegion.Below;
                }
                if (!upRegion.Dirty)
                {
                    lowRegion = upRegion;
                    upRegion = upRegion.Above;
                    if (upRegion == null || !upRegion.Dirty)
                    {
                        return;
                    }
                }
                upRegion.Dirty = false;
                var upEdge = upRegion.UpperEdge;
                var lowEdge = lowRegion.UpperEdge;
                if (upEdge.DestinationVertex != lowEdge.DestinationVertex)
                {
                    if (CheckForLeftSplice(upRegion))
                    {
                        if (lowRegion.FixUpperEdge)
                        {
                            DeleteRegion(lowRegion);
                            Mesh.Delete(lowEdge);
                            lowRegion = upRegion.Below;
                            lowEdge = lowRegion.UpperEdge;
                        }
                        else if (upRegion.FixUpperEdge)
                        {
                            DeleteRegion(upRegion);
                            Mesh.Delete(upEdge);
                            upRegion = lowRegion.Above;
                            upEdge = upRegion.UpperEdge;
                        }
                    }
                }
                if (upEdge.OriginVertex != lowEdge.OriginVertex)
                {
                    if (upEdge.DestinationVertex != lowEdge.DestinationVertex &&
                        !upRegion.FixUpperEdge && !lowRegion.FixUpperEdge &&
                        (upEdge.DestinationVertex == eventVertex || lowEdge.DestinationVertex == eventVertex))
                    {
                        if (CheckForIntersect(upRegion))
                        {
                            return;
                        }
                    }
                    else
                    {
                        CheckForRightSplice(upRegion);
                    }
                }
                if (upEdge.OriginVertex == lowEdge.OriginVertex && upEdge.DestinationVertex == lowEdge.DestinationVertex)
                {
                    lowEdge.AddWinding(upEdge);
                    DeleteRegion(upRegion);
                    Mesh.Delete(upEdge);
                    upRegion = lowRegion.Above;
                }
            }
        }

        private HalfEdge<TVertexData> FinishLeftRegions(ActiveRegion<TVertexData> firstRegion, ActiveRegion<TVertexData> lastRegion)
        {
            var previousRegion = firstRegion;
            var previousEdge = firstRegion.UpperEdge;
            while (previousRegion != lastRegion)
            {
                previousRegion.FixUpperEdge = false;
                var region = previousRegion.Below;
                var e = region.UpperEdge;
                if (e.OriginVertex != previousEdge.OriginVertex)
                {
                    if (!region.FixUpperEdge)
                    {
                        FinishRegion(previousRegion);
                        break;
                    }
                    e = Mesh.Connect(previousEdge.LeftFacePrevious, e.Symmetric);
                    FixUpperEdge(region, e);
                }
                if (previousEdge.OriginNext != e)
                {
                    Mesh.Splice(e.OriginPrevious, e);
                    Mesh.Splice(previousEdge, e);
                }
                FinishRegion(previousRegion);
                previousEdge = region.UpperEdge;
                previousRegion = region;
            }
            return previousEdge;
        }

        private void FinishRegion(ActiveRegion<TVertexData> region)
        {
            var e = region.UpperEdge;
            var f = e.LeftFace;
            f.Inside = region.Inside;
            f.Edge = e;
            DeleteRegion(region);
        }
    }
}
