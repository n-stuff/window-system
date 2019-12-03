using System;

namespace NStuff.Typography.Rasterization
{
    internal class PolygonRasterizer
    {
        private double[] accumulationBuffer = Array.Empty<double>();
        private uint width;
        private uint height;

        internal void Setup(uint width, uint height)
        {
            this.width = width;
            this.height = height;
            if (accumulationBuffer.Length < width * height)
            {
                accumulationBuffer = new double[width * height];
            }
            else
            {
                Array.Clear(accumulationBuffer, 0, accumulationBuffer.Length);
            }
        }

        internal void RasterizeEdge(double x0, double y0, double x1, double y1)
        {
            if (y0 == y1)
            {
                return;
            }
            double direction;
            if (y0 < y1)
            {
                direction = 1d;
            }
            else
            {
                direction = -1d;
                var t = x0;
                x0 = x1;
                x1 = t;
                t = y0;
                y0 = y1;
                y1 = t;
            }

            double dxdy = (x1 - x0) / (y1 - y0);
            double x = x0;
            if (y0 < 0d)
            {
                x -= y0 * dxdy;
            }
            uint ymax = Math.Min(height, (uint)Math.Ceiling(y1));
            for (uint y = (uint)Math.Max(0d, y0); y < ymax; y++)
            {
                int lineStart = (int)(y * width);
                double dy = Math.Min(y + 1d, y1) - Math.Max(y, y0);
                double xNext = x + dxdy * dy;
                double d = dy * direction;
                double xa;
                double xb;
                if (x < xNext)
                {
                    xa = x;
                    xb = xNext;
                }
                else
                {
                    xa = xNext;
                    xb = x;
                }
                double xaFloor = Math.Floor(xa);
                int xai = (int)xaFloor;
                double xbCeiling = Math.Ceiling(xb);
                int xbi = (int)xbCeiling;
                int i = lineStart + xai;
                if (xbi <= xai + 1)
                {
                    double xmf = 0.5d * (x + xNext) - xaFloor;
                    if (i >= 0)
                    {
                        accumulationBuffer[i] += d - d * xmf;
                    }
                    if (i + 1 < accumulationBuffer.Length)
                    {
                        accumulationBuffer[i + 1] += d * xmf;
                    }
                }
                else
                {
                    double s = 1d / (xb - xa);
                    double xaf = xa - xaFloor;
                    double a0 = 0.5d * s * (1d - xaf) * (1d - xaf);
                    double xbf = xb - xbCeiling + 1d;
                    double am = 0.5d * s * xbf * xbf;
                    if (i >= 0)
                    {
                        accumulationBuffer[i] += d * a0;
                    }
                    if (xbi == xai + 2)
                    {
                        accumulationBuffer[i + 1] += d * (1d - a0 - am);
                    }
                    else
                    {
                        double a1 = s * (1.5d - xaf);
                        accumulationBuffer[i + 1] += d * (a1 - a0);
                        for (int xi = xai + 2; xi < xbi - 1; xi++)
                        {
                            accumulationBuffer[lineStart + xi] += d * s;
                        }
                        double a2 = a1 + ((double)xbi - xai - 3d) * s;
                        accumulationBuffer[lineStart + xbi - 1] += d * (1d - a2 - am);
                    }
                    if (lineStart + xbi < accumulationBuffer.Length)
                    {
                        accumulationBuffer[lineStart + xbi] += d * am;
                    }
                }
                x = xNext;
            }
        }

        internal void DrawPolygon(byte[] bitmap, uint bitmapWidth, int x0, int y0)
        {
            int index = 0;
            double n = 0d;
            int y1 = (int)height + y0;
            int x1 = (int)width + x0;
            for (int y = y0; y < y1; y++)
            {
                var off = y * bitmapWidth;
                for (int x = x0; x < x1; x++)
                {
                    n += accumulationBuffer[index++];
                    var b = (byte)(255d * Math.Min(1d, Math.Abs(n)));
                    bitmap[off + x] = b;
                }
            }
        }
    }
}
