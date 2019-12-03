using System;
using System.Collections.Generic;

namespace NStuff.Typography.Font
{
    internal class TtfGlyph
    {
        private OpenType? openType;

        internal double Scale { get; private set; }
        internal double XMin { get; private set; }
        internal double XMax { get; private set; }
        internal double YMin { get; private set; }
        internal double YMax { get; private set; }
        internal List<uint> ContourEndPoints { get; } = new List<uint>();
        internal List<bool> OnCurveFlags { get; } = new List<bool>();
        internal List<(double x, double y)> Points { get; } = new List<(double x, double y)>();

        internal void Setup(OpenType openType, double pixelSize)
        {
            if (openType.GetGlyphOutlineKind() != GlyphOutlineKind.Ttf)
            {
                throw new ArgumentException(nameof(openType));
            }
            this.openType = openType;
            Scale = pixelSize / openType.GetUnitsPerEm();
        }

        internal bool DecodeGlyph(uint glyphIndex)
        {
            ContourEndPoints.Clear();
            OnCurveFlags.Clear();
            Points.Clear();
            if (ReadGlyph(glyphIndex, 1, 0, 0, 1, Scale, Scale))
            {
                var p = Points[0];
                XMin = XMax = p.x;
                YMin = YMax = p.y;
                for (int i = 1; i < Points.Count; i++)
                {
                    p = Points[i];
                    if (p.x < XMin)
                    {
                        XMin = p.x;
                    }
                    else if (p.x > XMax)
                    {
                        XMax = p.x;
                    }
                    if (p.y < YMin)
                    {
                        YMin = p.y;
                    }
                    else if (p.y > YMax)
                    {
                        YMax = p.y;
                    }
                }
                return true;
            }
            return false;
        }

        private bool ReadGlyph(uint glyphIndex, double at0, double at1, double at2, double at3, double d1, double d2)
        {
            if (!openType!.TryGetGlyphDataBlock(glyphIndex, out OpenType.DataBlock glyphBlock))
            {
                return false;
            }
            int contourCount = openType.ReadInt16(glyphBlock, 0);
            if (contourCount > 0)
            {
                uint firstPoint = (uint)Points.Count;
                for (uint i = 0; i < contourCount; i++)
                {
                    ContourEndPoints.Add(firstPoint + openType.ReadUInt16(glyphBlock, 10 + i * 2));
                }

                uint io = 10 + (uint)contourCount * 2 + 2;
                uint il = openType.ReadUInt16(glyphBlock, io - 2);
                uint d = io + il;
                uint fd = d;
                uint n = 1u + openType.ReadUInt16(glyphBlock, 10 + (uint)contourCount * 2 - 2);

                uint xCount = 0;
                for (uint i = 0; i < n; i++)
                {
                    var flags = openType.ReadByte(glyphBlock, d++);
                    uint xSize = ((flags & 2) != 0) ? 1u : (((flags & 16) == 0) ? 2u : 0u);
                    if ((flags & 8) != 0)
                    {
                        var repeat = openType.ReadByte(glyphBlock, d++);
                        i += repeat;
                        xSize += xSize * repeat;
                    }
                    xCount += xSize;
                }

                uint xd = d;
                uint yd = d + xCount;
                int x = 0;
                int y = 0;
                for (uint i = 0; i < n;)
                {
                    var flags = openType.ReadByte(glyphBlock, fd++);
                    byte repeat = 0;
                    if ((flags & 8) != 0)
                    {
                        repeat = openType.ReadByte(glyphBlock, fd++);
                    }
                    for (int j = 0; j <= repeat; j++, i++)
                    {
                        if ((flags & 2) != 0)
                        {
                            int dx = openType.ReadByte(glyphBlock, xd++);
                            x += ((flags & 16) != 0) ? dx : -dx;
                        }
                        else if ((flags & 16) == 0)
                        {
                            x += openType.ReadInt16(glyphBlock, xd);
                            xd += 2;
                        }
                        if ((flags & 4) != 0)
                        {
                            int dy = openType.ReadByte(glyphBlock, yd++);
                            y += ((flags & 32) != 0) ? dy : -dy;
                        }
                        else if ((flags & 32) == 0)
                        {
                            y += openType.ReadInt16(glyphBlock, yd);
                            yd += 2;
                        }

                        Points.Add((d1 * (at0 * x + at2 * y), d2 * (at1 * x + at3 * y)));
                        OnCurveFlags.Add((flags & 1) != 0);
                    }
                }
            }
            else
            {
                uint d = 10;
                uint flags;
                do
                {
                    uint firstPoint = (uint)Points.Count;
                    flags = openType.ReadUInt16(glyphBlock, d);
                    d += 2;
                    uint componentIndex = openType.ReadUInt16(glyphBlock, d);
                    d += 2;

                    double lat0 = 1f;
                    double lat1 = 0f;
                    double lat2 = 0f;
                    double lat3 = 1f;
                    double tx;
                    double ty;

                    int i1;
                    int i2;
                    if ((flags & 1) != 0)
                    {
                        i1 = openType.ReadInt16(glyphBlock, d);
                        d += 2;
                        i2 = openType.ReadInt16(glyphBlock, d);
                        d += 2;
                    }
                    else
                    {
                        i1 = openType.ReadSByte(glyphBlock, d);
                        d += 1;
                        i2 = openType.ReadSByte(glyphBlock, d);
                        d += 1;
                    }

                    if ((flags & (1 << 3)) != 0)
                    {
                        lat0 = lat3 = openType.ReadInt16(glyphBlock, d) / 16384f;
                        d += 2;
                        lat1 = lat2 = 0;
                    }
                    else if ((flags & (1 << 6)) != 0)
                    {
                        lat0 = openType.ReadInt16(glyphBlock, d) / 16384f;
                        d += 2;
                        lat1 = lat2 = 0;
                        lat3 = openType.ReadInt16(glyphBlock, d) / 16384f;
                        d += 2;
                    }
                    else if ((flags & (1 << 7)) != 0)
                    {
                        lat0 = openType.ReadInt16(glyphBlock, d) / 16384f;
                        d += 2;
                        lat1 = openType.ReadInt16(glyphBlock, d) / 16384f;
                        d += 2;
                        lat2 = openType.ReadInt16(glyphBlock, d) / 16384f;
                        d += 2;
                        lat3 = openType.ReadInt16(glyphBlock, d) / 16384f;
                        d += 2;
                    }

                    double ld1 = Math.Sqrt(lat0 * lat0 + lat1 * lat1) * Scale;
                    double ld2 = Math.Sqrt(lat2 * lat2 + lat3 * lat3) * Scale;

                    ReadGlyph(componentIndex, lat0, lat1, lat2, lat3, ld1, ld2);

                    if ((flags & 2) != 0)
                    {
                        tx = i1 * ld1;
                        ty = i2 * ld2;
                    }
                    else
                    {
                        var (px, py) = Points[i1];
                        var (nx, ny) = Points[(int)firstPoint + i2];

                        tx = px - nx;
                        ty = py - ny;
                    }

                    for (int i = (int)firstPoint; i < Points.Count; i++)
                    {
                        var (x, y) = Points[i];
                        Points[i] = (x + tx, y + ty);
                    }
                }
                while ((flags & (1 << 5)) != 0);
                if (at0 != 1f || at1 != 0f || at2 != 0f || at3 != 1f)
                {
                    for (int i = 0; i < Points.Count; i++)
                    {
                        var (x, y) = Points[i];
                        Points[i] = (d1 * (at0 * x + at2 * y), d2 * (at1 * x + at3 * y));
                    }
                }
            }
            return Points.Count > 0;
        }
    }
}
