using NStuff.Geometry;
using NStuff.GraphicsBackend;
using NStuff.Tessellation;
using System;
using System.Collections.Generic;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    /// <summary>
    /// Represents a complex shape composed of curves.
    /// </summary>
    public class PathDrawing : DrawingBase
    {
        internal readonly List<PathDrawingCommand> commands = new List<PathDrawingCommand>();
        internal readonly List<(double x, double y)> points = new List<(double x, double y)>();

        public RgbaColor FillColor { get; set; }
        public RgbaColor StrokeColor { get; set; }
        public double StrokeWidth { get; set; } = 1;
        public double StrokeMiterLimit { get; set; } = 4;
        public StrokeLinecap StrokeLinecap { get; set; }
        public StrokeLineJoin StrokeLineJoin { get; set; }
        public WindingRule WindingRule { get; set; } = WindingRule.NonZero;

        /// <summary>
        /// Changes the current point.
        /// </summary>
        /// <param name="currentPoint">The new current point.</param>
        public void Move((double x, double y) currentPoint)
        {
            commands.Add(PathDrawingCommand.MoveTo);
            points.Add(currentPoint);
        }

        /// <summary>
        /// Appends a line starting from the current point and ending at the supplied point.
        /// <paramref name="endPoint"/> becomes the current point.
        /// </summary>
        /// <param name="endPoint">The end point of the line.</param>
        public void AddLine((double x, double y) endPoint)
        {
            if (commands.Count == 0)
            {
                throw new InvalidOperationException();
            }
            commands.Add(PathDrawingCommand.LineTo);
            points.Add(endPoint);
        }

        /// <summary>
        /// Appends a quadratic bezier curve starting from the current point, with the specified control point,
        /// and ending at the specified end point.
        /// </summary>
        /// <param name="controlPoint">The control point of the curve.</param>
        /// <param name="endPoint">The end point of the curve.</param>
        public void AddQuadraticBezier((double x, double y) controlPoint, (double x, double y) endPoint)
        {
            if (commands.Count == 0)
            {
                throw new InvalidOperationException();
            }
            commands.Add(PathDrawingCommand.QuadraticBezierTo);
            points.Add(controlPoint);
            points.Add(endPoint);
        }

        /// <summary>
        /// Appends a cubic bezier curve starting from the current point, with the specified control points,
        /// and ending at the specified end point.
        /// </summary>
        /// <param name="firstControlPoint">The first control point of the curve.</param>
        /// <param name="secondControlPoint">The second control point of the curve.</param>
        /// <param name="endPoint">The end point of the curve.</param>
        public void AddCubicBezier((double x, double y) firstControlPoint, (double x, double y) secondControlPoint, (double x, double y) endPoint)
        {
            if (commands.Count == 0)
            {
                throw new InvalidOperationException();
            }
            commands.Add(PathDrawingCommand.CubicBezierTo);
            points.Add(firstControlPoint);
            points.Add(secondControlPoint);
            points.Add(endPoint);
        }

        internal override void Draw(DrawingContext context)
        {
            if (commands.Count == 0 || (StrokeColor.Alpha == 0 && FillColor.Alpha == 0))
            {
                return;
            }
            var polygon = GetApproximatedPolygon(context);

            int vertexCount;
            void DrawTriangles()
            {
                context.Backend.UpdateVertexBuffer(context.VertexBuffer, context.Vertices, 0, vertexCount);
                context.VertexRanges[0] = new VertexRange(0, vertexCount);
                context.Backend.UpdateVertexRangeBuffer(context.SingleVertexRangeBuffer, context.VertexRanges, 0, 1);

                context.CommandBuffers[0] = context.DrawIndirectCommandBuffer;
                context.Backend.SubmitCommands(context.CommandBuffers, 0, 1);
                vertexCount = 0;
            }

            if (FillColor.Alpha > 0)
            {
                var triangles = TessellatePolygon(context, polygon);

                context.Colors[0] = FillColor;
                context.Backend.UpdateUniformBuffer(context.SingleColorBuffer, context.Colors, 0, 1);
                context.Transforms[0] = new AffineTransform(m11: 1, m22: 1);
                context.Backend.UpdateUniformBuffer(context.SingleTransformBuffer, context.Transforms, 0, 1);

                context.CommandBuffers[0] = context.SetupPlainColorCommandBuffer;
                context.Backend.SubmitCommands(context.CommandBuffers, 0, 1);

                vertexCount = 0;
                for (int i = 0; i < triangles.Count; i++)
                {
                    var (x, y) = triangles[i];
                    context.Vertices[vertexCount++] = new PointCoordinates(x, y);
                    if (vertexCount % 3 == 0)
                    {
                        if (vertexCount + 3 > context.Vertices.Length)
                        {
                            DrawTriangles();
                        }
                    }
                }
                if (vertexCount > 0)
                {
                    DrawTriangles();
                }
            }

            if (StrokeColor.Alpha > 0)
            {
                var triangles = TessellateStrokedPolygon(context, polygon);

                context.Colors[0] = StrokeColor;
                context.Backend.UpdateUniformBuffer(context.SingleColorBuffer, context.Colors, 0, 1);
                context.Transforms[0] = new AffineTransform(m11: 1, m22: 1);
                context.Backend.UpdateUniformBuffer(context.SingleTransformBuffer, context.Transforms, 0, 1);

                context.CommandBuffers[0] = context.SetupPlainColorCommandBuffer;
                context.Backend.SubmitCommands(context.CommandBuffers, 0, 1);

                vertexCount = 0;
                for (int i = 0; i < triangles.Count; i++)
                {
                    var (x, y) = triangles[i];
                    context.Vertices[vertexCount++] = new PointCoordinates(x, y);
                    if (vertexCount % 3 == 0)
                    {
                        if (vertexCount + 3 > context.Vertices.Length)
                        {
                            DrawTriangles();
                        }
                    }
                }
                if (vertexCount > 0)
                {
                    DrawTriangles();
                }
            }
        }


        private List<List<(double x, double y)>> GetApproximatedPolygon(DrawingContext context)
        {
            context.BezierApproximator.DistanceTolerance = 0.5 / context.PixelScaling;
            var polygon = new List<List<(double x, double y)>>();
            var index = 0;
            var transform = Transform;
            var contour = default(List<(double x, double y)>);
            context.BezierApproximator.PointComputed = (x, y) => contour!.Add((x, y));
            for (int i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                switch (command)
                {
                    case PathDrawingCommand.MoveTo:
                        contour = new List<(double x, double y)>();
                        polygon.Add(contour);
                        contour.Add(DrawingContext.TransformPoint(points[index++], ref transform));
                        break;

                    case PathDrawingCommand.LineTo:
                        contour!.Add(DrawingContext.TransformPoint(points[index++], ref transform));
                        break;

                    case PathDrawingCommand.QuadraticBezierTo:
                        {
                            var (sx, sy) = contour![^1];
                            var (cx, cy) = DrawingContext.TransformPoint(points[index++], ref transform);
                            var (ex, ey) = DrawingContext.TransformPoint(points[index++], ref transform);
                            context.BezierApproximator.ApproximateQuadratic(sx, sy, cx, cy, ex, ey);
                        }
                        break;

                    case PathDrawingCommand.CubicBezierTo:
                        {
                            var (sx, sy) = contour![^1];
                            var (c1x, c1y) = DrawingContext.TransformPoint(points[index++], ref transform);
                            var (c2x, c2y) = DrawingContext.TransformPoint(points[index++], ref transform);
                            var (ex, ey) = DrawingContext.TransformPoint(points[index++], ref transform);
                            context.BezierApproximator.ApproximateCubic(sx, sy, c1x, c1y, c2x, c2y, ex, ey);
                        }
                        break;

                    default:
                        throw new InvalidOperationException("Unhandled command: " + command);
                }

            }
            return polygon;
        }

        private List<(double x, double y)> TessellatePolygon(DrawingContext context, List<List<(double x, double y)>> polygon)
        {
            var triangles = new List<(double x, double y)>();
            var tessellator = context.Tessellator;
            tessellator.WindingRule = WindingRule;
            tessellator.BeginPolygon(0);
            for (int i = 0; i < polygon.Count; i++)
            {
                var contour = polygon[i];
                tessellator.BeginContour();
                for (int j = 0; j < contour.Count; j++)
                {
                    var (x, y) = contour[j];
                    tessellator.AddVertex(x, y, 0);
                }
                tessellator.EndContour();
            }
            tessellator.EndPolygon();
            while (tessellator.Move())
            {
                var (x, y, _) = tessellator.Vertex;
                triangles.Add((x, y));
            }
            return triangles;
        }

        private List<(double x, double y)> TessellateStrokedPolygon(DrawingContext context, List<List<(double x, double y)>> polygon)
        {
            var triangles = new List<(double x, double y)>();
            var polylineStroker = context.PolylineStroker;
            polylineStroker.DistanceTolerance = 0.5 / context.PixelScaling;
            polylineStroker.StrokeLinecap = StrokeLinecap;
            polylineStroker.StrokeLineJoin = StrokeLineJoin;
            polylineStroker.StrokeMiterLimit = StrokeMiterLimit;
            polylineStroker.StrokeWidth = StrokeWidth * (Math.Abs(Transform.M11) + Math.Abs(Transform.M22)) / 2;
            var tessellator = context.Tessellator;
            tessellator.WindingRule = WindingRule.NonZero;
            for (int i = 0; i < polygon.Count; i++)
            {
                var contour = polygon[i];
                polylineStroker.Stroke(contour);
                while (tessellator.Move())
                {
                    var (x, y, _) = tessellator.Vertex;
                    triangles.Add((x, y));
                }
            }
            return triangles;
        }
    }
}
