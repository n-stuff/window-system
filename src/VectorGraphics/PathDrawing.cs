using NStuff.Geometry;
using NStuff.GraphicsBackend;
using NStuff.Tessellation;
using System;
using System.Collections.Generic;

namespace NStuff.VectorGraphics
{
    /// <summary>
    /// Represents a complex shape composed of curves.
    /// </summary>
    public class PathDrawing
    {
        private readonly List<PathDrawingCommand> commands = new List<PathDrawingCommand>();
        private readonly List<(double x, double y)> points = new List<(double x, double y)>();
        private readonly List<List<(double x, double y)>> polygon = new List<List<(double x, double y)>>();
        private readonly List<(double x, double y)> fillVertices = new List<(double x, double y)>();
        private readonly List<(double x, double y)> strokeVertices = new List<(double x, double y)>();
        private AffineTransform transform = new AffineTransform(m11: 1, m22: 1);
        private double strokeWidth = 1;
        private double strokeMiterLimit = 4;
        private StrokeLinecap strokeLinecap;
        private StrokeLineJoin strokeLineJoin;
        private WindingRule windingRule;

        /// <summary>
        /// Gets or sets the transform to apply to all coordinates of the drawing.
        /// </summary>
        /// <value>An affine transform. The initial value is the identity matrix.</value>
        public AffineTransform Transform {
            get => transform;
            set {
                if (transform != value)
                {
                    ClearCachedValues();
                    transform = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the color to use to fill this shape.
        /// </summary>
        /// <value>An RGBA color. The initial value is fully transparent black.</value>
        public RgbaColor FillColor { get; set; }

        /// <summary>
        /// Gets or sets the color to use to stroke this shape.
        /// </summary>
        /// <value>An RGBA color. The initial value is fully transparent black.</value>
        public RgbaColor StrokeColor { get; set; }

        /// <summary>
        /// Gets or sets the width of the stroke on this shape.
        /// </summary>
        /// <value>A positive number. Default value is <c>1</c>.</value>
        public double StrokeWidth {
            get => strokeWidth;
            set {
                if (strokeWidth != value)
                {
                    strokeVertices.Clear();
                    strokeWidth = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the limit on the ratio of the miter length to the stroke width.
        /// </summary>
        /// <value>A positive number. Default value is <c>4</c>.</value>
        public double StrokeMiterLimit {
            get => strokeMiterLimit;
            set {
                if (strokeMiterLimit != value)
                {
                    strokeVertices.Clear();
                    strokeMiterLimit = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the shape to be used at the end of open subpaths of this shape when they are stroked.
        /// </summary>
        /// <value>One of the enumerated values that specify the line cap. Default value is <c>Butt</c>.</value>
        public StrokeLinecap StrokeLinecap {
            get => strokeLinecap;
            set {
                if (strokeLinecap != value)
                {
                    strokeVertices.Clear();
                    strokeLinecap = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the shape to be used at the corners of paths of this shape when they are stroked.
        /// </summary>
        /// <value>One of the enumerated values that specify the line join. Default value is <c>Miter</c>.</value>
        public StrokeLineJoin StrokeLineJoin {
            get => strokeLineJoin;
            set {
                if (strokeLineJoin != value)
                {
                    strokeVertices.Clear();
                    strokeLineJoin = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the algorithm used to determine what parts of the framebuffer are parts of the shape.
        /// </summary>
        /// <value>One of the enumerated values that specify the winding rule. Default value is <c>NonZero</c>.</value>
        public WindingRule WindingRule {
            get => windingRule;
            set {
                if (windingRule != value)
                {
                    ClearCachedValues();
                    windingRule = value;
                }
            }
        }

        /// <summary>
        /// Changes the current point.
        /// </summary>
        /// <param name="currentPoint">The new current point.</param>
        public void Move((double x, double y) currentPoint)
        {
            ClearCachedValues();
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
            ClearCachedValues();
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
            ClearCachedValues();
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
            ClearCachedValues();
            commands.Add(PathDrawingCommand.CubicBezierTo);
            points.Add(firstControlPoint);
            points.Add(secondControlPoint);
            points.Add(endPoint);
        }

        /// <summary>
        /// Draws this shape.
        /// </summary>
        /// <param name="context">The context used to draw this shape.</param>
        public void Draw(DrawingContext context)
        {
            if (context.Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (commands.Count == 0 || (StrokeColor.Alpha == 0 && FillColor.Alpha == 0))
            {
                return;
            }
            if (FillColor.Alpha > 0)
            {
                var vertices = TessellatePolygon(context);

                context.SetColor(FillColor);
                context.SetTransform(Transform);
                context.SetupPlainColorRendering();

                for (int i = 0; i < vertices.Count; i += 3)
                {
                    context.AppendTriangleVertices(vertices[i], vertices[i + 1], vertices[i + 2]);
                }
            }

            if (StrokeColor.Alpha > 0 && strokeWidth > 0)
            {
                var vertices = StrokeAndTessellatePolygon(context);

                context.SetColor(StrokeColor);
                context.SetTransform(Transform);
                context.SetupPlainColorRendering();

                for (int i = 0; i < vertices.Count; i += 3)
                {
                    context.AppendTriangleVertices(vertices[i], vertices[i + 1], vertices[i + 2]);
                }
            }
        }

        private List<List<(double x, double y)>> GetApproximatedPolygon(DrawingContext context)
        {
            if (polygon.Count == 0)
            {
                var bezierApproximator = context.BezierApproximator;
                bezierApproximator.DistanceTolerance = 0.5 / (context.PixelScaling * (Math.Abs(Transform.M11) + Math.Abs(Transform.M22)) / 2);
                var index = 0;
                var contour = default(List<(double x, double y)>);
                bezierApproximator.PointComputed = (x, y) => contour!.Add((x, y));
                for (int i = 0; i < commands.Count; i++)
                {
                    var command = commands[i];
                    switch (command)
                    {
                        case PathDrawingCommand.MoveTo:
                            contour = new List<(double x, double y)>();
                            polygon.Add(contour);
                            contour.Add(points[index++]);
                            break;

                        case PathDrawingCommand.LineTo:
                            contour!.Add(points[index++]);
                            break;

                        case PathDrawingCommand.QuadraticBezierTo:
                            {
                                var (sx, sy) = contour![^1];
                                var (cx, cy) = points[index++];
                                var (ex, ey) = points[index++];
                                bezierApproximator.ApproximateQuadratic(sx, sy, cx, cy, ex, ey);
                            }
                            break;

                        case PathDrawingCommand.CubicBezierTo:
                            {
                                var (sx, sy) = contour![^1];
                                var (c1x, c1y) = points[index++];
                                var (c2x, c2y) = points[index++];
                                var (ex, ey) = points[index++];
                                bezierApproximator.ApproximateCubic(sx, sy, c1x, c1y, c2x, c2y, ex, ey);
                            }
                            break;

                        default:
                            throw new InvalidOperationException("Unhandled command: " + command);
                    }

                }
            }
            return polygon;
        }

        private List<(double x, double y)> TessellatePolygon(DrawingContext context)
        {
            if (fillVertices.Count == 0)
            {
                var polygon = GetApproximatedPolygon(context);
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
                    fillVertices.Add((x, y));
                }
            }
            return fillVertices;
        }

        private List<(double x, double y)> StrokeAndTessellatePolygon(DrawingContext context)
        {
            if (strokeVertices.Count == 0)
            {
                var polygon = GetApproximatedPolygon(context);
                var polylineStroker = context.PolylineStroker;
                polylineStroker.DistanceTolerance = 0.5 / (context.PixelScaling * (Math.Abs(Transform.M11) + Math.Abs(Transform.M22)) / 2);
                polylineStroker.StrokeLinecap = StrokeLinecap;
                polylineStroker.StrokeLineJoin = StrokeLineJoin;
                polylineStroker.StrokeMiterLimit = StrokeMiterLimit;
                polylineStroker.StrokeWidth = StrokeWidth;
                var tessellator = context.Tessellator;
                tessellator.WindingRule = WindingRule.NonZero;
                for (int i = 0; i < polygon.Count; i++)
                {
                    var contour = polygon[i];
                    polylineStroker.Stroke(contour);
                    while (tessellator.Move())
                    {
                        var (x, y, _) = tessellator.Vertex;
                        strokeVertices.Add((x, y));
                    }
                }
            }
            return strokeVertices;
        }

        private void ClearCachedValues()
        {
            polygon.Clear();
            fillVertices.Clear();
            strokeVertices.Clear();
        }
    }
}
