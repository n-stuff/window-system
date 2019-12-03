using System;

namespace NStuff.Geometry
{
    /// <summary>
    /// Provides methods to approximate Bézier curves by a set of consecutive points.
    /// </summary>
    public class BezierApproximator
    {
        private double collinearityEpsilon;
        private double distanceToleranceSquared;

        /// <summary>
        /// The threshold used to decide whether two segments are collinear.
        /// </summary>
        public double CollinearityEpsilon { get; set; } = 1e-3d;

        /// <summary>
        /// The threshold used to decide whether an approximation is acceptable.
        /// </summary>
        public double DistanceTolerance { get; set; } = 0.5;

        /// <summary>
        /// An action invoked when a new point has been computed.
        /// </summary>
        public Action<double, double>? PointComputed { get; set; }

        /// <summary>
        /// Approximates a quadratic Bézier curve by recursive subdivision.
        /// <see cref="PointComputed"/> is not called for (x1, y1).
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point of the curve.</param>
        /// <param name="y1">The y-coordinate of the first point of the curve.</param>
        /// <param name="x2">The x-coordinate of the control point of the curve.</param>
        /// <param name="y2">The y-coordinate of the control point of the curve.</param>
        /// <param name="x3">The x-coordinate of the last point of the curve.</param>
        /// <param name="y3">The y-coordinate of the last point of the curve.</param>
        public void ApproximateQuadratic(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            collinearityEpsilon = CollinearityEpsilon;
            distanceToleranceSquared = DistanceTolerance * DistanceTolerance;
            ApproximateQuadratic(x1, y1, x2, y2, x3, y3, 0);
            OnPointComputed(x3, y3);
        }

        private void ApproximateQuadratic(double x1, double y1, double x2, double y2, double x3, double y3, int depth)
        {
            for (;;)
            {
                if (depth > 32)
                {
                    return;
                }

                double x12 = (x1 + x2) / 2;
                double y12 = (y1 + y2) / 2;
                double x23 = (x2 + x3) / 2;
                double y23 = (y2 + y3) / 2;
                double x123 = (x12 + x23) / 2;
                double y123 = (y12 + y23) / 2;

                double dx = x3 - x1;
                double dy = y3 - y1;
                double d = Math.Abs(((x2 - x3) * dy - (y2 - y3) * dx));
                double da;

                if (d > collinearityEpsilon)
                {
                    if (d * d <= distanceToleranceSquared * (dx * dx + dy * dy))
                    {
                        OnPointComputed(x123, y123);
                        return;
                    }
                }
                else
                {
                    da = dx * dx + dy * dy;
                    if (da == 0)
                    {
                        d = SquaredDistance(x1, y1, x2, y2);
                    }
                    else
                    {
                        d = ((x2 - x1) * dx + (y2 - y1) * dy) / da;
                        if (d > 0 && d < 1)
                        {
                            return;
                        }
                        if (d <= 0)
                        {
                            d = SquaredDistance(x2, y2, x1, y1);
                        }
                        else if (d >= 1)
                        {
                            d = SquaredDistance(x2, y2, x3, y3);
                        }
                        else
                        {
                            d = SquaredDistance(x2, y2, x1 + d * dx, y1 + d * dy);
                        }
                    }
                    if (d < distanceToleranceSquared)
                    {
                        OnPointComputed(x2, y2);
                        return;
                    }
                }

                depth++;
                ApproximateQuadratic(x1, y1, x12, y12, x123, y123, depth);
                x1 = x123;
                y1 = y123;
                x2 = x23;
                y2 = y23;
            }
        }

        /// <summary>
        /// Approximates a cubic Bézier curve by recursive subdivision.
        /// <see cref="PointComputed"/> is not called for (x1, y1).
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point of the curve.</param>
        /// <param name="y1">The y-coordinate of the first point of the curve.</param>
        /// <param name="x2">The x-coordinate of the first control point of the curve.</param>
        /// <param name="y2">The y-coordinate of the first control point of the curve.</param>
        /// <param name="x3">The x-coordinate of the second control point of the curve.</param>
        /// <param name="y3">The y-coordinate of the second control point of the curve.</param>
        /// <param name="x4">The x-coordinate of the last point of the curve.</param>
        /// <param name="y4">The y-coordinate of the last point of the curve.</param>
        public void ApproximateCubic(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            collinearityEpsilon = CollinearityEpsilon;
            distanceToleranceSquared = DistanceTolerance * DistanceTolerance;
            ApproximateCubic(x1, y1, x2, y2, x3, y3, x4, y4, 0);
            OnPointComputed(x4, y4);
        }

        private void ApproximateCubic(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, int depth)
        {
            for (;;)
            {
                if (depth > 32)
                {
                    return;
                }

                double x12 = (x1 + x2) / 2;
                double y12 = (y1 + y2) / 2;
                double x23 = (x2 + x3) / 2;
                double y23 = (y2 + y3) / 2;
                double x34 = (x3 + x4) / 2;
                double y34 = (y3 + y4) / 2;
                double x123 = (x12 + x23) / 2;
                double y123 = (y12 + y23) / 2;
                double x234 = (x23 + x34) / 2;
                double y234 = (y23 + y34) / 2;
                double x1234 = (x123 + x234) / 2;
                double y1234 = (y123 + y234) / 2;

                double dx = x4 - x1;
                double dy = y4 - y1;

                double d2 = Math.Abs((x2 - x4) * dy - (y2 - y4) * dx);
                double d3 = Math.Abs((x3 - x4) * dy - (y3 - y4) * dx);
                double da1, da2, k;

                bool d2NonCollinear = d2 > collinearityEpsilon;
                bool d3NonCollinear = d3 > collinearityEpsilon;
                if (d3NonCollinear)
                {
                    if (d2NonCollinear)
                    {
                        if ((d2 + d3) * (d2 + d3) <= distanceToleranceSquared * (dx * dx + dy * dy))
                        {
                            OnPointComputed(x23, y23);
                            return;
                        }
                    }
                    else
                    {
                        if (d3 * d3 <= distanceToleranceSquared * (dx * dx + dy * dy))
                        {
                            OnPointComputed(x23, y23);
                            return;
                        }
                    }
                }
                else if (d2NonCollinear)
                {
                    if (d2 * d2 <= distanceToleranceSquared * (dx * dx + dy * dy))
                    {
                        OnPointComputed(x23, y23);
                        return;
                    }
                }
                else
                {
                    k = dx * dx + dy * dy;
                    if (k == 0)
                    {
                        d2 = SquaredDistance(x1, y1, x2, y2);
                        d3 = SquaredDistance(x4, y4, x3, y3);
                    }
                    else
                    {
                        k = 1 / k;
                        da1 = x2 - x1;
                        da2 = y2 - y1;
                        d2 = k * (da1 * dx + da2 * dy);
                        da1 = x3 - x1;
                        da2 = y3 - y1;
                        d3 = k * (da1 * dx + da2 * dy);
                        if (d2 > 0 && d2 < 1 && d3 > 0 && d3 < 1)
                        {
                            return;
                        }
                        if (d2 <= 0)
                        {
                            d2 = SquaredDistance(x2, y2, x1, y1);
                        }
                        else if (d2 >= 1)
                        {
                            d2 = SquaredDistance(x2, y2, x4, y4);
                        }
                        else
                        {
                            d2 = SquaredDistance(x2, y2, x1 + d2 * dx, y1 + d2 * dy);
                        }

                        if (d3 <= 0)
                        {
                            d3 = SquaredDistance(x3, y3, x1, y1);
                        }
                        else if (d3 >= 1)
                        {
                            d3 = SquaredDistance(x3, y3, x4, y4);
                        }
                        else
                        {
                            d3 = SquaredDistance(x3, y3, x1 + d3 * dx, y1 + d3 * dy);
                        }
                    }
                    if (d2 > d3)
                    {
                        if (d2 < distanceToleranceSquared)
                        {
                            OnPointComputed(x2, y2);
                            return;
                        }
                    }
                    else
                    {
                        if (d3 < distanceToleranceSquared)
                        {
                            OnPointComputed(x3, y3);
                            return;
                        }
                    }
                }

                depth++;
                ApproximateCubic(x1, y1, x12, y12, x123, y123, x1234, y1234, depth);
                x1 = x1234;
                y1 = y1234;
                x2 = x234;
                y2 = y234;
                x3 = x34;
                y3 = y34;
            }
        }

        private void OnPointComputed(double x, double y) => PointComputed?.Invoke(x, y);

        private static double SquaredDistance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return dx * dx + dy * dy;
        }
    }
}
