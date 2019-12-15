using System;

namespace NStuff.Typography.Font
{
    internal class CffGlyphDecoder : GlyphDecoder
    {
        private OpenType? openType;
        private OpenType.DataBlock globalSubrs;
        private OpenType.DataBlock subrs;
        private OpenType.DataBlock charString;
        private uint cursor;
        private readonly double[] stack = new double[48];
        private uint stackIndex;
        private readonly OpenType.DataBlock[] subrStack = new OpenType.DataBlock[10];
        private readonly uint[] cursorStack = new uint[10];
        private uint subrStackIndex;
        private bool inHeader;
        private uint maskBits;

        private double x;
        private double y;
        private uint instruction;
        private uint i;
        private bool clearStack;
        private double dx1;
        private double dy1;
        private double dx2;
        private double dy2;
        private double dx3;
        private double dy3;
        private double dx4;
        private double dy4;
        private double dx5;
        private double dy5;
        private double dx6;
        private double dy6;
        private double sx;
        private double sy;

        internal override bool Setup(OpenType openType, double pixelSize, uint glyphIndex)
        {
            Reset();
            if (openType.GetGlyphOutlineKind() != GlyphOutlineKind.Cff)
            {
                throw new ArgumentException(nameof(openType));
            }
            this.openType = openType;
            globalSubrs = openType.GetGlobalSubrs();
            subrs = openType.GetGlyphSubrs(glyphIndex);
            charString = openType.GetCharString(glyphIndex);
            Scale = pixelSize / openType.GetUnitsPerEm();

            bool first = true;
            XMin = 0;
            XMax = 0;
            YMin = 0;
            YMax = 0;
            while (Move())
            {
                if (first)
                {
                    first = false;
                    XMin = X;
                    YMin = Y;
                    XMax = X;
                    YMax = Y;
                }
                else
                {
                    if (X < XMin)
                    {
                        XMin = X;
                    }
                    else if (X > XMax)
                    {
                        XMax = X;
                    }
                    if (Y < YMin)
                    {
                        YMin = Y;
                    }
                    else if (Y > YMax)
                    {
                        YMax = Y;
                    }
                }
                switch (PathCommand)
                {
                    case PathCommand.MoveTo:
                        break;
                    case PathCommand.LineTo:
                        break;

                    case PathCommand.CubicBezierTo:
                        if (Cx1 < XMin)
                        {
                            XMin = Cx1;
                        }
                        else if (Cx1 > XMax)
                        {
                            XMax = Cx1;
                        }
                        if (Cy1 < YMin)
                        {
                            YMin = Cy1;
                        }
                        else if (Cy1 > YMax)
                        {
                            YMax = Cy1;
                        }
                        goto case PathCommand.QuadraticBezierTo;

                    case PathCommand.QuadraticBezierTo:
                        if (Cx < XMin)
                        {
                            XMin = Cx;
                        }
                        else if (Cx > XMax)
                        {
                            XMax = Cx;
                        }
                        if (Cy < YMin)
                        {
                            YMin = Cy;
                        }
                        else if (Cy > YMax)
                        {
                            YMax = Cy;
                        }
                        break;
                }
            }
            Reset();
            return XMin != 0 || XMax != 0;
        }

        private void Reset()
        {
            if (subrStackIndex > 0)
            {
                charString = subrStack[0];
            }
            cursor = 0;
            stackIndex = 0;
            subrStackIndex = 0;
            inHeader = true;
            maskBits = 0;
            x = 0;
            y = 0;
            i = 0;
            instruction = 0;
            sx = 0;
            sy = 0;
        }

        internal override bool Move()
        {
            for (;;)
            {
                switch (instruction)
                {
                    case 0x0E: // endchar
                        return false;

                    case 0x04: // vmoveto
                        sx = x;
                        sy = y += stack[stackIndex - 1];
                        SetPath(PathCommand.MoveTo, x, y, 0, 0, 0, 0);
                        instruction = 0;
                        stackIndex = 0;
                        return true;

                    case 0x15: // rmoveto
                        sx = x += stack[stackIndex - 2];
                        sy = y += stack[stackIndex - 1];
                        SetPath(PathCommand.MoveTo, x, y, 0, 0, 0, 0);
                        instruction = 0;
                        stackIndex = 0;
                        return true;

                    case 0x16: // hmoveto
                        sx = x += stack[stackIndex - 1];
                        sy = y;
                        SetPath(PathCommand.MoveTo, x, y, 0, 0, 0, 0);
                        instruction = 0;
                        stackIndex = 0;
                        return true;

                    case 0x05: // rlineto
                        x += stack[i];
                        y += stack[i + 1];
                        i += 2;
                        SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                        if (i + 1 < stackIndex)
                        {
                            instruction = 0x05;
                        }
                        else
                        {
                            instruction = 0;
                            stackIndex = 0;
                        }
                        return true;

                    case 0x06: // hlineto
                        x += stack[i++];
                        SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                        if (i < stackIndex)
                        {
                            instruction = 0x07;
                        }
                        else
                        {
                            instruction = 0;
                            stackIndex = 0;
                        }
                        return true;

                    case 0x07: // vlineto
                        y += stack[i++];
                        SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                        if (i < stackIndex)
                        {
                            instruction = 0x06;
                        }
                        else
                        {
                            instruction = 0;
                            stackIndex = 0;
                        }
                        return true;

                    case 0x1E: // vhcurveto
                        SetCurveTo(0, stack[i], stack[i + 1], stack[i + 2], stack[i + 3], (stackIndex - i == 5) ? stack[i + 4] : 0d);
                        i += 4;
                        if (i + 3 < stackIndex)
                        {
                            instruction = 0x1F;
                        }
                        else
                        {
                            instruction = 0;
                            stackIndex = 0;
                        }
                        return true;

                    case 0x1F: // hvcurveto
                        SetCurveTo(stack[i], 0, stack[i + 1], stack[i + 2], (stackIndex - i == 5) ? stack[i + 4] : 0d, stack[i + 3]);
                        i += 4;
                        if (i + 3 < stackIndex)
                        {
                            instruction = 0x1E;
                        }
                        else
                        {
                            instruction = 0;
                            stackIndex = 0;
                        }
                        return true;

                    case 0x08: // rrcurveto
                        SetCurveTo(stack[i], stack[i + 1], stack[i + 2], stack[i + 3], stack[i + 4], stack[i + 5]);
                        i += 6;
                        if (i + 5 < stackIndex)
                        {
                            instruction = 0x08;
                        }
                        else
                        {
                            instruction = 0;
                            stackIndex = 0;
                        }
                        return true;

                    case 0x18: // rcurveline
                        if (i + 5 < stackIndex - 2)
                        {
                            SetCurveTo(stack[i], stack[i + 1], stack[i + 2], stack[i + 3], stack[i + 4], stack[i + 5]);
                            i += 6;
                            return true;
                        }
                        else
                        {
                            x += stack[i];
                            y += stack[i + 1];
                            SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                            instruction = 0;
                            stackIndex = 0;
                            return true;
                        }

                    case 0x19: // rlinecurve
                        if (i + 1 < stackIndex - 6)
                        {
                            x += stack[i];
                            y += stack[i + 1];
                            i += 2;
                            SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                            return true;
                        }
                        else
                        {
                            SetCurveTo(stack[i], stack[i + 1], stack[i + 2], stack[i + 3], stack[i + 4], stack[i + 5]);
                            instruction = 0;
                            stackIndex = 0;
                            return true;
                        }

                    case 0x1A: // vvcurveto
                        SetCurveTo(0d, stack[i], stack[i + 1], stack[i + 2], 0d, stack[i + 3]);
                        i += 4;
                        if (i + 3 < stackIndex)
                        {
                            instruction = 0x1A;
                        }
                        else
                        {
                            instruction = 0;
                            stackIndex = 0;
                        }
                        return true;

                    case 0x1B: // hhcurveto
                        SetCurveTo(stack[i], 0d, stack[i + 1], stack[i + 2], stack[i + 3], 0d);
                        i += 4;
                        if (i + 3 < stackIndex)
                        {
                            instruction = 0x1B;
                        }
                        else
                        {
                            instruction = 0;
                            stackIndex = 0;
                        }
                        return true;

                    case 0x22: // hflex
                        SetCurveTo(dx4, 0d, dx5, -dy2, dx6, 0d);
                        instruction = 0;
                        stackIndex = 0;
                        return true;

                    case 0x23: // flex
                        SetCurveTo(dx4, dy4, dx5, dy5, dx6, dy6);
                        instruction = 0;
                        stackIndex = 0;
                        return true;

                    case 0x24: // hflex1
                        SetCurveTo(dx4, 0, dx5, dy5, dx6, -(dy1 + dy2 + dy5));
                        instruction = 0;
                        stackIndex = 0;
                        return true;

                    case 0x25: // flex1
                        SetCurveTo(dx4, dy4, dx5, dy5, dx6, dy6);
                        instruction = 0;
                        stackIndex = 0;
                        return true;
                }
                bool exit = false;
                while (cursor < charString.length && !exit)
                {
                    i = 0;
                    clearStack = true;
                    instruction = 0;
                    uint b0 = openType!.ReadByte(charString, cursor++);
                    switch (b0)
                    {
                        case 0x01: // hstem
                        case 0x03: // vstem
                        case 0x12: // hstemhm
                        case 0x17: // vstemhm
                            maskBits += (stackIndex / 2);
                            break;

                        case 0x13:
                        case 0x14:
                            if (inHeader)
                            {
                                maskBits += (stackIndex / 2);
                                inHeader = false;
                            }
                            cursor += (maskBits + 7) / 8;
                            break;

                        case 0x0E: // endchar
                            if (x != sx || y != sy)
                            {
                                SetPath(PathCommand.LineTo, sx, sy, 0, 0, 0, 0);
                                instruction = 0x0E;
                                return true;
                            }
                            return false;

                        case 0x04: // vmoveto
                        case 0x15: // rmoveto
                        case 0x16: // hmoveto
                            inHeader = false;
                            instruction = b0;
                            if (x != sx || y != sy)
                            {
                                SetPath(PathCommand.LineTo, sx, sy, 0, 0, 0, 0);
                                return true;
                            }
                            clearStack = false;
                            exit = true;
                            break;

                        case 0x05: // rlineto
                            x += stack[i];
                            y += stack[i + 1];
                            i += 2;
                            SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                            if (i + 1 < stackIndex)
                            {
                                instruction = 0x05;
                            }
                            else
                            {
                                stackIndex = 0;
                            }
                            return true;

                        case 0x06: // hlineto
                            x += stack[i++];
                            SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                            if (i < stackIndex)
                            {
                                instruction = 0x07;
                            }
                            else
                            {
                                stackIndex = 0;
                            }
                            return true;

                        case 0x07: // vlineto
                            y += stack[i++];
                            SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                            if (i < stackIndex)
                            {
                                instruction = 0x06;
                            }
                            else
                            {
                                stackIndex = 0;
                            }
                            return true;

                        case 0x1E: // vhcurveto
                            SetCurveTo(0, stack[i], stack[i + 1], stack[i + 2], stack[i + 3], (stackIndex - i == 5) ? stack[i + 4] : 0d);
                            i += 4;
                            if (i + 3 < stackIndex)
                            {
                                instruction = 0x1F;
                            }
                            else
                            {
                                stackIndex = 0;
                            }
                            return true;

                        case 0x1F: // hvcurveto
                            SetCurveTo(stack[i], 0, stack[i + 1], stack[i + 2], (stackIndex - i == 5) ? stack[i + 4] : 0f, stack[i + 3]);
                            i += 4;
                            if (i + 3 < stackIndex)
                            {
                                instruction = 0x1E;
                            }
                            else
                            {
                                stackIndex = 0;
                            }
                            return true;

                        case 0x08: // rrcurveto
                            SetCurveTo(stack[i], stack[i + 1], stack[i + 2], stack[i + 3], stack[i + 4], stack[i + 5]);
                            i += 6;
                            if (i + 5 < stackIndex)
                            {
                                instruction = 0x08;
                            }
                            else
                            {
                                stackIndex = 0;
                            }
                            return true;

                        case 0x18: // rcurveline
                            SetCurveTo(stack[i], stack[i + 1], stack[i + 2], stack[i + 3], stack[i + 4], stack[i + 5]);
                            i += 6;
                            instruction = 0x18;
                            return true;

                        case 0x19: // rlinecurve
                            x += stack[i];
                            y += stack[i + 1];
                            i += 2;
                            SetPath(PathCommand.LineTo, x, y, 0, 0, 0, 0);
                            instruction = 0x19;
                            return true;

                        case 0x1A: // vvcurveto
                            double f = ((stackIndex & 1) != 0) ? stack[i++] : 0d;
                            SetCurveTo(f, stack[i], stack[i + 1], stack[i + 2], 0d, stack[i + 3]);
                            i += 4;
                            if (i + 3 < stackIndex)
                            {
                                instruction = 0x1A;
                            }
                            else
                            {
                                stackIndex = 0;
                            }
                            return true;

                        case 0x1B: // hhcurveto
                            f = ((stackIndex & 1) != 0) ? stack[i++] : 0d;
                            SetCurveTo(stack[i], f, stack[i + 1], stack[i + 2], stack[i + 3], 0d);
                            i += 4;
                            if (i + 3 < stackIndex)
                            {
                                instruction = 0x1B;
                            }
                            else
                            {
                                stackIndex = 0;
                            }
                            return true;

                        case 0x0A: // callsubr
                            {
                                if (subrStackIndex >= 10)
                                {
                                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.SubrStackOverflow));
                                }
                                uint v = (uint)stack[--stackIndex];
                                cursorStack[subrStackIndex] = cursor;
                                subrStack[subrStackIndex++] = charString;
                                charString = openType.GetSubr(subrs, v);
                                cursor = 0;
                                clearStack = false;
                            }
                            break;

                        case 0x1D: // callgsubr
                            {
                                if (subrStackIndex >= 10)
                                {
                                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.SubrStackOverflow));
                                }
                                uint v = (uint)stack[--stackIndex];
                                cursorStack[subrStackIndex] = cursor;
                                subrStack[subrStackIndex++] = charString;
                                charString = openType.GetSubr(globalSubrs, v);
                                cursor = 0;
                                clearStack = false;
                            }
                            break;

                        case 0x0B: // return
                            charString = subrStack[--subrStackIndex];
                            cursor = cursorStack[subrStackIndex];
                            clearStack = false;
                            break;

                        case 0x0C: // two-byte escape
                            {
                                uint b1 = openType.ReadByte(charString, cursor++);
                                switch (b1)
                                {
                                    case 0x22: // hflex
                                        dx1 = stack[0];
                                        dx2 = stack[1];
                                        dy2 = stack[2];
                                        dx3 = stack[3];
                                        dx4 = stack[4];
                                        dx5 = stack[5];
                                        dx6 = stack[6];
                                        SetCurveTo(dx1, 0, dx2, dy2, dx3, 0);
                                        instruction = 0x22;
                                        return true;

                                    case 0x23: // flex
                                        dx1 = stack[0];
                                        dy1 = stack[1];
                                        dx2 = stack[2];
                                        dy2 = stack[3];
                                        dx3 = stack[4];
                                        dy3 = stack[5];
                                        dx4 = stack[6];
                                        dy4 = stack[7];
                                        dx5 = stack[8];
                                        dy5 = stack[9];
                                        dx6 = stack[10];
                                        dy6 = stack[11];
                                        SetCurveTo(dx1, dy1, dx2, dy2, dx3, dy3);
                                        instruction = 0x23;
                                        return true;

                                    case 0x24: // hflex1
                                        dx1 = stack[0];
                                        dy1 = stack[1];
                                        dx2 = stack[2];
                                        dy2 = stack[3];
                                        dx3 = stack[4];
                                        dx4 = stack[5];
                                        dx5 = stack[6];
                                        dy5 = stack[7];
                                        dx6 = stack[8];
                                        SetCurveTo(dx1, dy1, dx2, dy2, dx3, 0);
                                        instruction = 0x24;
                                        return true;

                                    case 0x25: // flex1
                                        dx1 = stack[0];
                                        dy1 = stack[1];
                                        dx2 = stack[2];
                                        dy2 = stack[3];
                                        dx3 = stack[4];
                                        dy3 = stack[5];
                                        dx4 = stack[6];
                                        dy4 = stack[7];
                                        dx5 = stack[8];
                                        dy5 = stack[9];
                                        dx6 = dy6 = stack[10];
                                        double dx = dx1 + dx2 + dx3 + dx4 + dx5;
                                        double dy = dy1 + dy2 + dy3 + dy4 + dy5;
                                        if (Math.Abs(dx) > Math.Abs(dy))
                                        {
                                            dy6 = -dy;
                                        }
                                        else
                                        {
                                            dx6 = -dx;
                                        }
                                        SetCurveTo(dx1, dy1, dx2, dy2, dx3, dy3);
                                        instruction = 0x25;
                                        return true;

                                    default:
                                        throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFontFormat));
                                }
                            }

                        default:
                            if (b0 != 255u && b0 != 28 && (b0 < 32 || b0 > 254))
                            {
                                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFontFormat));
                            }
                            if (stackIndex >= 48)
                            {
                                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.OperandStackOverflow));
                            }
                            if (b0 == 255u)
                            {
                                stack[stackIndex++] = openType.ReadInt32(charString, cursor) / 0x10000;
                                cursor += 4;
                            }
                            else
                            {
                                uint c = cursor - 1;
                                stack[stackIndex++] = openType.ReadInt32(charString, ref c);
                                cursor = c;
                            }
                            clearStack = false;
                            break;
                    }
                    if (clearStack)
                    {
                        stackIndex = 0;
                    }
                }
                if (charString.length == 0)
                {
                    return false;
                }
                if (!exit)
                {
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFontFormat));
                }
            }
        }

        private void SetPath(PathCommand pathCommand, double x, double y, double cx, double cy, double cx1, double cy1)
        {
            PathCommand = pathCommand;
            X = x * Scale;
            Y = y * Scale;
            Cx = cx * Scale;
            Cy = cy * Scale;
            Cx1 = cx1 * Scale;
            Cy1 = cy1 * Scale;
        }

        private void SetCurveTo(double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
        {
            double cx = x + dx1;
            double cy = y + dy1;
            double cx1 = cx + dx2;
            double cy1 = cy + dy2;
            x = cx1 + dx3;
            y = cy1 + dy3;
            SetPath(PathCommand.CubicBezierTo, x, y, cx, cy, cx1, cy1);
        }
    }
}
