using System;
using System.Collections.Generic;

namespace NStuff.Geometry
{
    /// <summary>
    /// Provides methods to compute the stroke of a polyline.
    /// </summary>
    public class PolylineStroker
    {
        private readonly IPolylineStrokeHandler handler;

        /// <summary>
        /// Gets or sets the width of the stroke.
        /// </summary>
        public double StrokeWidth { get; set; } = 1d;

        /// <summary>
        /// Gets or sets a value indicating the shape of the ends of an open polyline.
        /// </summary>
        public StrokeLinecap StrokeLinecap { get; set; }

        /// <summary>
        /// Gets or sets a value indicationg the shape of the corners of a polyline.
        /// </summary>
        public StrokeLineJoin StrokeLineJoin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the limit on the extent of a line join of kind <see cref="StrokeLineJoin.Miter"/>.
        /// That value is multiplied by <see cref="StrokeWidth"/> to compute the maximal value of the extent.
        /// </summary>
        public double StrokeMiterLimit { get; set; } = 4d;

        /// <summary>
        /// Gets the maximum distance between an approximated segment and an arc.
        /// </summary>
        public double DistanceTolerance { get; set; } = 0.5;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolylineStroker"/> class.
        /// </summary>
        /// <param name="handler">The handler called when a new element of a stroke was computed.</param>
        public PolylineStroker(IPolylineStrokeHandler handler) => this.handler = handler;

        /// <summary>
        /// Strokes the specified polyline.
        /// </summary>
        /// <param name="points">The points composing the polyline to stroke.</param>
        public void Stroke(List<(double x, double y)> points)
        {
            if (points.Count < 2 || StrokeWidth <= 0d)
            {
                return;
            }
            var pointCount = 1;
            var p1 = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                var (x, y) = points[i];
                if (x != p1.x || y != p1.y)
                {
                    pointCount++;
                }
            }
            if (pointCount < 2)
            {
                return;
            }
            var p0 = points[^1];
            var closed = p0.x == p1.x && p0.y == p1.y;

            handler.BeginPolygon();
            handler.BeginContour();

            var offset = StrokeWidth / 2;
            var lastIndex = points.Count - 1;
            while (p0.x == p1.x && p0.y == p1.y && lastIndex > 0)
            {
                p0 = points[--lastIndex];
            }
            var pp0 = default((double x, double y));
            var pp1 = default((double x, double y));
            if (closed)
            {
                ComputeParallelSegment(offset, p0, p1, out pp0, out pp1);
            }
            for (var i = 1; i < points.Count; i++)
            {
                var p2 = points[i];
                if (p1.x == p2.x && p1.y == p2.y)
                {
                    continue;
                }
                ComputeParallelSegment(offset, p1, p2, out var pp2, out var pp3);
                var addPp2 = true;
                if (i > 1 || closed)
                {
                    addPp2 = ComputeJoin(p0, p1, p2, pp0, pp1, pp2, pp3);
                }
                if (i == 1 && !closed)
                {
                    switch (StrokeLinecap)
                    {
                        case StrokeLinecap.Round:
                            {
                                ComputeParallelSegment(offset, p2, p1, out var tpp0, out var tpp1);
                                ComputeHalfCircle(StrokeWidth / 2, p1, tpp1);
                            }
                            break;
                        case StrokeLinecap.Square:
                            {
                                ComputeParallelSegment(offset, p2, p1, out var tpp0, out var tpp1);
                                ComputeParallelSegment(offset, tpp1, pp2, out var spp0, out var spp1);
                                handler.AddPoint(spp0.x, spp0.y);
                                handler.AddPoint(spp1.x, spp1.y);
                                addPp2 = false;
                            }
                            break;
                    }
                }
                if (addPp2)
                {
                    handler.AddPoint(pp2.x, pp2.y);
                }
                p0 = p1;
                p1 = p2;
                pp0 = pp2;
                pp1 = pp3;
            }
            if (closed)
            {
                handler.AddPoint(pp1.x, pp1.y);
                handler.EndContour();
                handler.BeginContour();
                p0 = points[1];
                p1 = points[points.Count - 1];
                ComputeParallelSegment(offset, p0, p1, out pp0, out pp1);
            }
            else
            {
                switch (StrokeLinecap)
                {
                    case StrokeLinecap.Round:
                        ComputeHalfCircle(StrokeWidth / 2, p1, pp1);
                        break;
                    case StrokeLinecap.Square:
                        ComputeParallelSegment(offset, p1, p0, out var tpp0, out var tpp1);
                        ComputeParallelSegment(offset, pp1, tpp0, out var spp0, out var spp1);
                        handler.AddPoint(spp0.x, spp0.y);
                        handler.AddPoint(spp1.x, spp1.y);
                        break;
                    default:
                        handler.AddPoint(pp1.x, pp1.y);
                        break;
                }
            }
            for (var i = points.Count - 2; i >= 0; --i)
            {
                var p2 = points[i];
                if (p1.x == p2.x && p1.y == p2.y)
                {
                    continue;
                }
                ComputeParallelSegment(offset, p1, p2, out var pp2, out var pp3);
                var addPp2 = true;
                if (i < points.Count - 2 || closed)
                {
                    addPp2 = ComputeJoin(p0, p1, p2, pp0, pp1, pp2, pp3);
                }
                if (i == points.Count - 2 && !closed && StrokeLinecap != StrokeLinecap.Butt)
                {
                    addPp2 = false;
                }
                if (addPp2)
                {
                    handler.AddPoint(pp2.x, pp2.y);
                }
                p0 = p1;
                p1 = p2;
                pp0 = pp2;
                pp1 = pp3;
            }
            if (!closed)
            {
                switch (StrokeLinecap)
                {
                    case StrokeLinecap.Square:
                        break;
                    default:
                        handler.AddPoint(pp1.x, pp1.y);
                        break;
                }
            }

            handler.EndContour();
            handler.EndPolygon();
        }

        private bool ComputeJoin((double x, double y) p0, (double x, double y) p1, (double x, double y) p2,
            (double x, double y) pp0, (double x, double y) pp1, (double x, double y) pp2, (double x, double y) pp3)
        {
            var x0 = p1.x - p0.x;
            var y0 = p1.y - p0.y;
            var x1 = p2.x - p1.x;
            var y1 = p2.y - p1.y;
            var cross = x0 * y1 - y0 * x1;
            if (cross > 0)
            {
                if (StrokeLineJoin == StrokeLineJoin.Miter)
                {
                    if (ComputeIntersection(pp0, pp1, pp2, pp3, out var pi) == Intersection.Outside)
                    {
                        var dx = pi.x - p1.x;
                        var dy = pi.y - p1.y;
                        var miterLength = Math.Sqrt(dx * dx + dy * dy);
                        if ((miterLength / StrokeWidth) <= StrokeMiterLimit)
                        {
                            handler.AddPoint(pi.x, pi.y);
                            return false;
                        }
                    }
                }
                else if (StrokeLineJoin == StrokeLineJoin.Round)
                {
                    ComputeArc(StrokeWidth / 2, p1, pp1, pp2);
                    return true;
                }
            }
            else
            {
                if (ComputeIntersection(pp0, pp1, pp2, pp3, out var pi) == Intersection.Inside)
                {
                    handler.AddPoint(pi.x, pi.y);
                    return false;
                }
            }
            handler.AddPoint(pp1.x, pp1.y);
            return true;
        }

        private void ComputeArc(double r, (double x, double y) p1, (double x, double y) pp1, (double x, double y) pp2)
        {
            var epsilon = Math.Acos((r - DistanceTolerance) / r);
            handler.AddPoint(pp1.x, pp1.y);

            var startAngle = Math.Acos((pp1.x - p1.x) / r);
            var endAngle = Math.Acos((pp2.x - p1.x) / r);
            if (pp1.y - p1.y > 0)
            {
                startAngle = 2 * Math.PI - startAngle;
            }
            if (pp2.y - p1.y > 0)
            {
                endAngle = 2 * Math.PI - endAngle;
            }
            if (endAngle > startAngle)
            {
                if (endAngle - startAngle > Math.PI)
                {
                    endAngle -= 2 * Math.PI;
                }
            }
            else
            {
                if (startAngle - endAngle > Math.PI)
                {
                    startAngle -= 2 * Math.PI;
                }
            }
            if (startAngle > endAngle)
            {
                for (var a = startAngle - epsilon; a > endAngle; a -= epsilon)
                {
                    var x = r * Math.Cos(a);
                    var y = -r * Math.Sin(a);
                    handler.AddPoint(x + p1.x, y + p1.y);
                }
            }
            else
            {
                for (var a = startAngle + epsilon; a < endAngle; a += epsilon)
                {
                    var x = r * Math.Cos(a);
                    var y = -r * Math.Sin(a);
                    handler.AddPoint(x + p1.x, y + p1.y);
                }
            }
        }

        private void ComputeHalfCircle(double r, (double x, double y) p1, (double x, double y) pp1)
        {
            var epsilon = Math.Acos((r - DistanceTolerance) / r);
            handler.AddPoint(pp1.x, pp1.y);

            var startAngle = Math.Acos((pp1.x - p1.x) / r);
            if (pp1.y - p1.y > 0)
            {
                startAngle = 2 * Math.PI - startAngle;
            }
            for (var a = startAngle - epsilon; a > startAngle - Math.PI; a -= epsilon)
            {
                var x = r * Math.Cos(a);
                var y = -r * Math.Sin(a);
                handler.AddPoint(x + p1.x, y + p1.y);
            }
        }

        private static void ComputeParallelSegment(double offset, (double x, double y) p0, (double x, double y) p1,
            out (double x, double y) pp0, out (double x, double y) pp1)
        {
            var dx = p0.x - p1.x;
            var dy = p1.y - p0.y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            dy = offset * dy / length;
            dx = offset * dx / length;
            var x0 = p0.x + dy;
            var x1 = p1.x + dy;
            var y0 = p0.y + dx;
            var y1 = p1.y + dx;
            pp0 = (x0, y0);
            pp1 = (x1, y1);
        }

        private enum Intersection
        {
            ZeroLength,
            Parallel,
            Consecutive,
            Outside,
            Inside
        }

        private static Intersection ComputeIntersection(
            (double x, double y) p0,
            (double x, double y) p1,
            (double x, double y) p2,
            (double x, double y) p3,
            out (double x, double y) p)
        {
            if ((p0.x == p1.x && p0.y == p1.y) || (p2.x == p3.x && p2.y == p3.y))
            {
                p = (0, 0);
                return Intersection.ZeroLength;
            }
            if ((p0.x == p2.x && p0.y == p2.y) || (p1.x == p2.x && p1.y == p2.y) || (p0.x == p3.x && p0.y == p3.y) || (p1.x == p3.x && p1.y == p3.y))
            {
                p = (0, 0);
                return Intersection.Consecutive;
            }
            p1.x -= p0.x;
            p1.y -= p0.y;
            p2.x -= p0.x;
            p2.y -= p0.y;
            p3.x -= p0.x;
            p3.y -= p0.y;

            var length = Math.Sqrt(p1.x * p1.x + p1.y * p1.y);
            var cos = p1.x / length;
            var sin = p1.y / length;
            var x = p2.x * cos + p2.y * sin;
            p2.y = p2.y * cos - p2.x * sin;
            p2.x = x;
            x = p3.x * cos + p3.y * sin;
            p3.y = p3.y * cos - p3.x * sin;
            p3.x = x;

            if (p2.y == p3.y)
            {
                p = (0, 0);
                return Intersection.Parallel;
            }

            var coeff = p3.x + (p2.x - p3.x) * p3.y / (p3.y - p2.y);
            p = (p0.x + coeff * cos, p0.y + coeff * sin);
            return ((p2.x < 0 && p3.y < 0) || (p2.y >= 0 && p3.y >= 0)) ? Intersection.Outside : Intersection.Inside;
        }
    }
}
