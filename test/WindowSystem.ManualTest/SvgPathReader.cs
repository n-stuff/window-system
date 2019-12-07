using System;

namespace NStuff.WindowSystem.ManualTest
{
    public enum PathCommandType : int
    {
        MoveTo = 'M',
        MoveToRelative = 'm',
        LineTo = 'L',
        LineToRelative = 'l',
        Closepath = 'Z',
        HorizontalLineTo = 'H',
        HorizontalLineToRelative = 'h',
        VerticalLineTo = 'V',
        VerticalLineToRelative = 'v',
        CurveTo = 'C',
        CurveToRelative = 'c',
        SmoothCurveTo = 'S',
        SmoothCurveToRelative = 's',
        QuadraticCurveTo = 'Q',
        QuadraticCurveToRelative = 'q',
        SmoothQuadraticCurveTo = 'T',
        SmoothQuadraticCurveToRelative = 't',
        EllipticalArc = 'A',
        EllipticalArcRelative = 'a'
    }

    public class SvgPathReader
    {
        private int index;
        private string pathData = default!;

        public string PathData {
            get => pathData;
            set {
                pathData = value;
                index = 0;
            }
        }

        public PathCommandType PathCommand { get; private set; }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float X1 { get; private set; }
        public float Y1 { get; private set; }
        public float X2 { get; private set; }
        public float Y2 { get; private set; }
        public float Rx { get; private set; }
        public float Ry { get; private set; }
        public float Angle { get; private set; }
        public bool LargeArcFlag { get; private set; }
        public bool SweepFlag { get; private set; }

        public bool Read()
        {
            SkipWhitespaces();
            if (CanRead)
            {
                char current = PathData[index];
                switch (current)
                {
                    case 'm':
                    case 'M':
                    case 'l':
                    case 'L':
                        index++;
                        ParseMoveToLineTo((PathCommandType)current);
                        break;
                    case 'h':
                    case 'H':
                        index++;
                        ParseHorizontalLineTo((PathCommandType)current);
                        break;
                    case 'v':
                    case 'V':
                        index++;
                        ParseVerticalLineTo((PathCommandType)current);
                        break;
                    case 'c':
                    case 'C':
                        index++;
                        ParseCurveTo((PathCommandType)current);
                        break;
                    case 's':
                    case 'S':
                        index++;
                        ParseSmoothCurveTo((PathCommandType)current);
                        break;
                    case 'z':
                    case 'Z':
                        index++;
                        PathCommand = PathCommandType.Closepath;
                        break;
                    case 'q':
                    case 'Q':
                        index++;
                        ParseQuadraticCurveTo((PathCommandType)current);
                        break;
                    case 't':
                    case 'T':
                        index++;
                        ParseSmoothQuadraticCurveTo((PathCommandType)current);
                        break;
                    case 'a':
                    case 'A':
                        index++;
                        ParseEllipticalArc((PathCommandType)current);
                        break;
                    default:
                        SkipCommaWhitespaces();
                        switch (PathCommand)
                        {
                            case PathCommandType.MoveToRelative:
                            case PathCommandType.LineToRelative:
                                ParseMoveToLineTo(PathCommandType.LineToRelative);
                                break;
                            case PathCommandType.MoveTo:
                            case PathCommandType.LineTo:
                                ParseMoveToLineTo(PathCommandType.LineTo);
                                break;
                            case PathCommandType.HorizontalLineTo:
                            case PathCommandType.HorizontalLineToRelative:
                                ParseHorizontalLineTo(PathCommand);
                                break;
                            case PathCommandType.VerticalLineTo:
                            case PathCommandType.VerticalLineToRelative:
                                ParseVerticalLineTo(PathCommand);
                                break;
                            case PathCommandType.CurveTo:
                            case PathCommandType.CurveToRelative:
                                ParseCurveTo(PathCommand);
                                break;
                            case PathCommandType.SmoothCurveTo:
                            case PathCommandType.SmoothCurveToRelative:
                                ParseSmoothCurveTo(PathCommand);
                                break;
                            case PathCommandType.QuadraticCurveTo:
                            case PathCommandType.QuadraticCurveToRelative:
                                ParseQuadraticCurveTo(PathCommand);
                                break;
                            case PathCommandType.SmoothQuadraticCurveTo:
                            case PathCommandType.SmoothQuadraticCurveToRelative:
                                ParseSmoothQuadraticCurveTo(PathCommand);
                                break;
                            case PathCommandType.EllipticalArc:
                            case PathCommandType.EllipticalArcRelative:
                                ParseEllipticalArc(PathCommand);
                                break;
                            case PathCommandType.Closepath:
                                break;
                        }
                        break;
                }
                return true;
            }
            return false;
        }

        private void ParseMoveToLineTo(PathCommandType command)
        {
            PathCommand = command;
            SkipWhitespaces();
            X = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y = Helpers.Parse(PathData, ref index);
        }

        private void ParseHorizontalLineTo(PathCommandType command)
        {
            PathCommand = command;
            SkipWhitespaces();
            X = Helpers.Parse(PathData, ref index);
        }
        private void ParseVerticalLineTo(PathCommandType command)
        {
            PathCommand = command;
            SkipWhitespaces();
            Y = Helpers.Parse(PathData, ref index);
        }

        private void ParseCurveTo(PathCommandType command)
        {
            PathCommand = command;
            SkipWhitespaces();
            X1 = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y1 = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            X2 = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y2 = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            X = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y = Helpers.Parse(PathData, ref index);
        }

        private void ParseSmoothCurveTo(PathCommandType command)
        {
            PathCommand = command;
            SkipWhitespaces();
            X2 = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y2 = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            X = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y = Helpers.Parse(PathData, ref index);
        }

        private void ParseQuadraticCurveTo(PathCommandType command)
        {
            PathCommand = command;
            SkipWhitespaces();
            X1 = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y1 = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            X = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y = Helpers.Parse(PathData, ref index);
        }

        private void ParseSmoothQuadraticCurveTo(PathCommandType command)
        {
            PathCommand = command;
            SkipWhitespaces();
            X = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y = Helpers.Parse(PathData, ref index);
        }

        private void ParseEllipticalArc(PathCommandType command)
        {
            PathCommand = command;
            SkipWhitespaces();
            Rx = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Ry = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Angle = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            LargeArcFlag = Helpers.ParseBoolean(PathData, ref index);
            SkipCommaWhitespaces();
            SweepFlag = Helpers.ParseBoolean(PathData, ref index);
            SkipCommaWhitespaces();
            X = Helpers.Parse(PathData, ref index);
            SkipCommaWhitespaces();
            Y = Helpers.Parse(PathData, ref index);
        }

        private void SkipWhitespaces()
        {
            while (CanRead)
            {
                int current = (int)PathData[index];
                switch (current)
                {
                    case 0x20:
                    case 0x9:
                    case 0xD:
                    case 0xA:
                        index++;
                        break;
                    default:
                        goto exit;
                }
            }
        exit:;
        }

        private void SkipCommaWhitespaces()
        {
            SkipWhitespaces();
            if (CanRead)
            {
                if (PathData[index] != ',')
                {
                    return;
                }
                index++;
                SkipWhitespaces();
            }
        }

        private bool CanRead => index < PathData.Length;
    }

    public static class Helpers
    {

        public static bool ParseBoolean(string s, ref int index)
        {
            char current = s[index++];
            if (current == '0') { return false; }
            if (current == '1') { return true; }
            throw new FormatException($"Invalid character {current} at index {index - 1}");
        }

        public static float Parse(string s, ref int index)
        {
            int mant = 0;
            int mantDig = 0;
            bool mantPos = true;
            bool mantRead = false;

            int exp = 0;
            int expDig = 0;
            int expAdj = 0;
            bool expPos = true;

            char current = s[index];

            switch (current)
            {
                case '-':
                    mantPos = false;
                    goto case '+';
                case '+':
                    if (!Read(s, ref index, ref current))
                    {
                        goto parseCompleted;
                    }
                    break;
            }
            // parse integer fragment
            switch (current)
            {
                case '0':
                    mantRead = true;
                    for (; ; )
                    {
                        if (!Read(s, ref index, ref current))
                        {
                            goto parseCompleted;
                        }
                        switch (current)
                        {
                            case '0':
                                break;
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                goto noneZero;
                            case '.':
                            case 'e':
                            case 'E':
                                goto intParsed;
                            default:
                                return 0.0f;
                        }
                    }
                noneZero:
                    goto case '1';
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    mantRead = true;
                    for (; ; )
                    {
                        if (mantDig < 9)
                        {
                            mantDig++;
                            mant = mant * 10 + (current - '0');
                        }
                        else
                        {
                            expAdj++;
                        }
                        if (!Read(s, ref index, ref current))
                        {
                            goto parseCompleted;
                        }
                        switch (current)
                        {
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                break;
                            default:
                                goto intParsed;
                        }
                    }
                case '.':
                    break;
                default:
                    throw new FormatException($"Invalid character {current} at index {index}");
            }

        intParsed:
            // parse decimal fragment
            if (current == '.')
            {
                if (!Read(s, ref index, ref current))
                {
                    goto parseCompleted;
                }
                switch (current)
                {
                    default:
                    case 'e':
                    case 'E':
                        if (!mantRead)
                        {
                            throw new FormatException($"Invalid character {current} at index {index}");
                        }
                        break;
                    case '0':
                        if (mantDig == 0)
                        {
                            for (; ; )
                            {
                                if (!Read(s, ref index, ref current))
                                {
                                    goto parseCompleted;
                                }
                                expAdj--;
                                switch (current)
                                {
                                    case '0':
                                        break;
                                    case '1':
                                    case '2':
                                    case '3':
                                    case '4':
                                    case '5':
                                    case '6':
                                    case '7':
                                    case '8':
                                    case '9':
                                        goto noneZero;
                                    default:
                                        if (!mantRead)
                                        {
                                            return 0.0f;
                                        }
                                        goto decimalParsed;

                                }
                            }
                        }
                    noneZero:
                        goto case '1';
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        for (; ; )
                        {
                            if (mantDig < 9)
                            {
                                mantDig++;
                                mant = mant * 10 + (current - '0');
                                expAdj--;
                            }
                            if (!Read(s, ref index, ref current))
                            {
                                goto parseCompleted;
                            }
                            switch (current)
                            {
                                default:
                                    goto decimalParsed;
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    break;
                            }
                        }
                }
            }

        decimalParsed:
            // parse exponent fragment
            switch (current)
            {
                case 'e':
                case 'E':
                    if (!Read(s, ref index, ref current))
                    {
                        goto parseCompleted;
                    }
                    switch (current)
                    {
                        case '-':
                            expPos = false;
                            goto case '+';
                        case '+':
                            if (!Read(s, ref index, ref current))
                            {
                                goto parseCompleted;
                            }
                            switch (current)
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    break;
                                default:
                                    throw new FormatException($"Invalid character {current} at index {index}");
                            }
                            goto case '0';
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            break;
                        default:
                            throw new FormatException($"Invalid character {current} at index {index}");
                    }

                    switch (current)
                    {
                        case '0':
                            for (; ; )
                            {
                                if (!Read(s, ref index, ref current))
                                {
                                    goto parseCompleted;
                                }
                                switch (current)
                                {
                                    case '0':
                                        break;
                                    case '1':
                                    case '2':
                                    case '3':
                                    case '4':
                                    case '5':
                                    case '6':
                                    case '7':
                                    case '8':
                                    case '9':
                                        goto noneZero;
                                    default:
                                        goto parseCompleted;
                                }
                            }
                        noneZero:
                            goto case '1';
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            for (; ; )
                            {
                                if (expDig < 3)
                                {
                                    expDig++;
                                    exp = exp * 10 + (current - '0');
                                }
                                if (!Read(s, ref index, ref current))
                                {
                                    goto parseCompleted;
                                }
                                switch (current)
                                {
                                    case '0':
                                    case '1':
                                    case '2':
                                    case '3':
                                    case '4':
                                    case '5':
                                    case '6':
                                    case '7':
                                    case '8':
                                    case '9':
                                        break;
                                    default:
                                        goto parseCompleted;
                                }
                            }
                    }
                    break;
                default:
                    break;
            }

        parseCompleted:
            if (!expPos)
            {
                exp = -exp;
            }
            exp += expAdj;
            if (!mantPos)
            {
                mant = -mant;
            }

            return BuildFloat(mant, exp);
        }

        private static bool Read(string s, ref int index, ref char current)
        {
            if (index < s.Length)
            {
                index++;
            }
            bool hasNext = index < s.Length;
            if (hasNext)
            {
                current = s[index];
            }
            return hasNext;
        }

        private static float BuildFloat(int mant, int exp)
        {
            if (exp < -125 || mant == 0)
            {
                return 0.0f;
            }
            if (exp >= 128)
            {
                return (mant > 0) ? float.PositiveInfinity : float.NegativeInfinity;
            }
            if (exp == 0)
            {
                return mant;
            }
            if (mant >= (1 << 26))
            {
                mant++;  // round up trailing bits if they will be dropped.
            }
            return (float)((exp > 0) ? mant * pow10[exp] : mant / pow10[-exp]);
        }

        private static readonly double[] pow10 = new double[128];

        static Helpers()
        {
            for (int i = 0; i < pow10.Length; i++)
            {
                pow10[i] = Math.Pow(10, i);
            }
        }
    }
}
