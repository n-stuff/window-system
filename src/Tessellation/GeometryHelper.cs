using System;

namespace NStuff.Tessellation
{
    internal static class GeometryHelper
    {
        internal static double GetL1Distance<TVertexData>(Vertex<TVertexData> u, Vertex<TVertexData> v) => Math.Abs(u.X - v.X) + Math.Abs(u.Y - v.Y);

        internal static bool EdgeGoesLeft<TVertexData>(HalfEdge<TVertexData> edge) => edge.DestinationVertex.CompareTo(edge.OriginVertex) == 0;
        internal static bool EdgeGoesRight<TVertexData>(HalfEdge<TVertexData> edge) => edge.OriginVertex.CompareTo(edge.DestinationVertex) == 0;

        internal static double GetEdgeSign<TVertexData>(Vertex<TVertexData> u, Vertex<TVertexData> v, Vertex<TVertexData> w)
        {
            var gapLeft = v.X - u.X;
            var gapRight = w.X - v.X;
            if (gapLeft + gapRight > 0)
            {
                return (v.Y - w.Y) * gapLeft + (v.Y - u.Y) * gapRight;
            }
            return 0;
        }

        internal static double EvaluateEdge<TVertexData>(Vertex<TVertexData> u, Vertex<TVertexData> v, Vertex<TVertexData> w)
        {
            var gapLeft = v.X - u.X;
            var gapRight = w.X - v.X;
            var gap = gapLeft + gapRight;
            if (gap <= 0)
            {
                return 0;
            }
            if (gapLeft < gapRight)
            {
                return (v.Y - u.Y) + (u.Y - w.Y) * (gapLeft / gap);
            }
            else
            {
                return (v.Y - w.Y) + (w.Y - u.Y) * (gapRight / gap);
            }
        }

        internal static double GetEdgeSignTransposed<TVertexData>(Vertex<TVertexData> u, Vertex<TVertexData> v, Vertex<TVertexData> w)
        {
            var gapLeft = v.Y - u.Y;
            var gapRight = w.Y - v.Y;
            if (gapLeft + gapRight > 0)
            {
                return (v.X - w.X) * gapLeft + (v.X - u.X) * gapRight;
            }
            return 0;
        }

        internal static double EvaluateEdgeTransposed<TVertexData>(Vertex<TVertexData> u, Vertex<TVertexData> v, Vertex<TVertexData> w)
        {
            var gapLeft = v.Y - u.Y;
            var gapRight = w.Y - v.Y;
            var gap = gapLeft + gapRight;
            if (gap <= 0)
            {
                return 0;
            }
            if (gapLeft < gapRight)
            {
                return (v.X - u.X) + (u.X - w.X) * (gapLeft / gap);
            }
            else
            {
                return (v.X - w.X) + (w.X - u.X) * (gapRight / gap);
            }
        }

        internal static void IntersectEdges<TVertexData>(Vertex<TVertexData> o1, Vertex<TVertexData> d1,
            Vertex<TVertexData> o2, Vertex<TVertexData> d2, Vertex<TVertexData> v)
        {
            if (o1.CompareTo(d1) > 0)
            {
                Swap(ref o1, ref d1);
            }
            if (o2.CompareTo(d2) > 0)
            {
                Swap(ref o2, ref d2);
            }
            if (o1.CompareTo(o2) > 0)
            {
                Swap(ref o1, ref o2);
                Swap(ref d1, ref d2);
            }

            if (o2.CompareTo(d1) > 0)
            {
                v.X = (o2.X + d1.X) / 2;
            }
            else if (d1.CompareTo(d2) <= 0)
            {
                var z1 = EvaluateEdge(o1, o2, d1);
                var z2 = EvaluateEdge(o2, d1, d2);
                if (z1 + z2 < 0)
                {
                    z1 = -z1;
                    z2 = -z2;
                }
                v.X = Interpolate(z1, o2.X, z2, d1.X);
            }
            else
            {
                var z1 = GetEdgeSign(o1, o2, d1);
                var z2 = -GetEdgeSign(o1, d2, d1);
                if (z1 + z2 < 0)
                {
                    z1 = -z1;
                    z2 = -z2;
                }
                v.X = Interpolate(z1, o2.X, z2, d2.X);
            }

            if (o1.CompareToTransposed(d1) > 0)
            {
                Swap(ref o1, ref d1);
            }
            if (o2.CompareToTransposed(d2) > 0)
            {
                Swap(ref o2, ref d2);
            }
            if (o1.CompareToTransposed(o2) > 0)
            {
                Swap(ref o1, ref o2);
                Swap(ref d1, ref d2);
            }

            if (o2.CompareToTransposed(d1) > 0)
            {
                v.Y = (o2.Y + d1.Y) / 2;
            }
            else if (d1.CompareToTransposed(d2) <= 0)
            {
                var z1 = EvaluateEdgeTransposed(o1, o2, d1);
                var z2 = EvaluateEdgeTransposed(o2, d1, d2);
                if (z1 + z2 < 0)
                {
                    z1 = -z1;
                    z2 = -z2;
                }
                v.Y = Interpolate(z1, o2.Y, z2, d1.Y);
            }
            else
            {
                var z1 = GetEdgeSignTransposed(o1, o2, d1);
                var z2 = -GetEdgeSignTransposed(o1, d2, d1);
                if (z1 + z2 < 0)
                {
                    z1 = -z1;
                    z2 = -z2;
                }
                v.Y = Interpolate(z1, o2.Y, z2, d2.Y);
            }
        }

        private static double Interpolate(double a, double x, double b, double y)
        {
            a = (a < 0) ? 0 : a;
            b = (b < 0) ? 0 : b;
            return ((a <= b) ?
                ((b == 0) ?
                    ((x + y) / 2) : (x + (y - x) * (a / (a + b))))
                  : (y + (x - y) * (b / (a + b))));
        }

        private static void Swap<T>(ref T left, ref T right)
        {
            T t;
            t = left;
            left = right;
            right = t;
        }
    }
}
