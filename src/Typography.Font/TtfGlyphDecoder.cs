namespace NStuff.Typography.Font
{
    internal class TtfGlyphDecoder : GlyphDecoder
    {
        private readonly TtfGlyph glyph = new();
        private int index;
        private bool startOff;
        private bool wasOff;
        private double sx;
        private double sy;
        private double px;
        private double py;
        private double scx;
        private double scy;
        private bool pathClosed;
        private uint nextMove;
        private uint contour;
        private bool advanced;
        private bool moveTo;

        internal override bool Setup(OpenType openType, double pixelSize, uint glyphIndex)
        {
            Reset();
            glyph.Setup(openType, pixelSize);
            Scale = glyph.Scale;
            if (glyph.DecodeGlyph(glyphIndex))
            {
                XMin = glyph.XMin;
                YMin = glyph.YMin;
                XMax = glyph.XMax;
                YMax = glyph.YMax;
                Scale = glyph.Scale;
                return true;
            }
            return false;
        }

        private void Reset()
        {
            index = 0;
            startOff = false;
            wasOff = false;
            sx = 0;
            sy = 0;
            px = 0;
            py = 0;
            scx = 0;
            scy = 0;
            pathClosed = false;
            nextMove = 0;
            contour = 0;
            advanced = true;
        }

        internal override bool Move()
        {
            for (;;)
            {
                if (index >= glyph.Points.Count)
                {
                    if (!pathClosed)
                    {
                        ClosePath();
                        return true;
                    }
                    return false;
                }

                var onCurve = glyph.OnCurveFlags[index];
                var (x, y) = glyph.Points[index];

                if (advanced)
                {
                    if (nextMove == index)
                    {
                        if (!onCurve)
                        {
                            nextMove++;
                            moveTo = false;
                        }
                        else
                        {
                            moveTo = true;
                            nextMove = glyph.ContourEndPoints[(int)contour] + 1;
                            contour++;
                        }
                    }
                    else
                    {
                        moveTo = false;
                    }
                }

                if (startOff)
                {
                    if (!onCurve)
                    {
                        sx = (scx + x) / 2;
                        sy = (scy + y) / 2;
                    }
                    else
                    {
                        sx = x;
                        sy = y;
                    }
                    SetPath(PathCommand.MoveTo, sx, sy, 0, 0);
                    wasOff = false;
                    startOff = false;
                    index++;
                    advanced = true;
                    return true;
                }
                else
                {
                    if (moveTo)
                    {
                        if (index != 0 && !pathClosed)
                        {
                            ClosePath();
                            advanced = false;
                            return true;
                        }
                        else
                        {
                            startOff = !onCurve;
                            if (startOff)
                            {
                                scx = x;
                                scy = y;
                                advanced = false;
                            }
                            else
                            {
                                sx = x;
                                sy = y;
                                SetPath(PathCommand.MoveTo, sx, sy, 0, 0);
                                wasOff = false;
                                index++;
                                advanced = true;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (!onCurve)
                        {
                            if (wasOff)
                            {
                                SetPath(PathCommand.QuadraticBezierTo, (px + x) / 2, (py + y) / 2, px, py);
                                px = x;
                                py = y;
                                wasOff = true;
                                index++;
                                advanced = true;
                                return true;
                            }
                            px = x;
                            py = y;
                            wasOff = true;
                            index++;
                            advanced = true;
                        }
                        else
                        {
                            if (wasOff)
                            {
                                SetPath(PathCommand.QuadraticBezierTo, x, y, px, py);
                            }
                            else
                            {
                                SetPath(PathCommand.LineTo, x, y, 0, 0);
                            }
                            wasOff = false;
                            index++;
                            advanced = true;
                            return true;
                        }
                    }
                }
            }
        }

        private void ClosePath()
        {
            if (startOff)
            {
                if (wasOff)
                {
                    SetPath(PathCommand.QuadraticBezierTo, (px + scx) / 2, (py + scy) / 2, px, py);
                }
                else
                {
                    SetPath(PathCommand.QuadraticBezierTo, sx, sy, scx, scy);
                }
            }
            else
            {
                if (wasOff)
                {
                    SetPath(PathCommand.QuadraticBezierTo, sx, sy, px, py);
                }
                else
                {
                    SetPath(PathCommand.LineTo, sx, sy, 0, 0);
                }
            }
            pathClosed = true;
        }

        private void SetPath(PathCommand pathCommand, double x, double y, double cx, double cy)
        {
            PathCommand = pathCommand;
            X = x;
            Y = y;
            Cx = cx;
            Cy = cy;
            pathClosed = false;
        }
    }
}
