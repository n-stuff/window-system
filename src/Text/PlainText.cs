using System;
using System.Text;

namespace NStuff.Text
{
    /// <summary>
    /// Represents a collection of unicode code point organized in lines and colums.
    /// </summary>
    public class PlainText
    {
        protected const int DefaultCapacity = 4;

        private readonly GapBuffer<CodePointBuffer> lines;

        /// <summary>
        /// Gets the number of lines of this text.
        /// </summary>
        /// <value>A positive integer.</value>
        public int LineCount => lines.Count;

        /// <summary>
        /// Gets or sets the code point at the supplied location in this text.
        /// </summary>
        /// <param name="location">An existing line / column pair in this text.</param>
        /// <returns>The code point at the supplied location.</returns>
        public int this[(int line, int column) location] {
            get {
                var line = lines[location.line];
                if ((uint)location.column >= (uint)line.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(location.column));
                }
                return line[location.column];
            }
            set {
                var line = lines[location.line];
                if ((uint)location.column >= (uint)line.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(location.column));
                }
                line[location.column] = value;
                lines[location.line] = line;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <c>PlainText</c> class.
        /// </summary>
        public PlainText()
        {
            lines = new GapBuffer<CodePointBuffer>(DefaultCapacity);
            lines.Insert(0, new CodePointBuffer(DefaultCapacity));
        }

        /// <summary>
        /// Initializes a new instance of the <c>PlainText</c> class using the supplied text.
        /// </summary>
        /// <param name="plainText">The text to copy.</param>
        public PlainText(PlainText plainText)
        {
            var lineCount = plainText.LineCount;
            lines = new GapBuffer<CodePointBuffer>((int)BitHelper.GetNextPowerOfTwo((uint)lineCount));
            for (int i = 0; i < lineCount; i++)
            {
                lines.Insert(i, new CodePointBuffer(plainText.lines[i]));
            }
        }

        /// <summary>
        /// Gets the size of the code points that compose the supplied line.
        /// </summary>
        /// <param name="lineNumber">The number of a valid line.</param>
        /// <returns><c>1</c>, <c>2</c>, or <c>3</c>.</returns>
        public int GetLineMaxCodePointSize(int lineNumber) => lines[lineNumber].ElementSize;

        /// <summary>
        /// Gets the number of code points of the supplied line.
        /// </summary>
        /// <param name="lineNumber">The number of a valid line.</param>
        /// <param name="ignoreNewline">A value indicating whether the trailing new line characters should ignored when
        /// counting the code points.</param>
        /// <returns>A number of code points.</returns>
        public int GetColumnCount(int lineNumber, bool ignoreNewline = false)
        {
            if (ignoreNewline)
            {
                var line = lines[lineNumber];
                var count = line.Count;
                switch (count)
                {
                    case 0:
                        return 0;
                    case 1:
                        return IsNewline(line[0]) ? 0 : 1;
                    default:
                        if (line[count - 2] == '\r')
                        {
                            return count - 2;
                        }
                        return IsNewline(line[count - 1]) ? count - 1 : count;
                }
            }
            else
            {
                return lines[lineNumber].Count;
            }
        }

        private static bool IsNewline(int codePoint)
        {
            return codePoint switch
            {
                '\r' or '\n' or 0x2028 or 0x2029 => true,
                _ => false,
            };
        }

        /// <summary>
        /// Inserts <paramref name="codePoint"/> at the supplied location in this text.
        /// </summary>
        /// <param name="location">A valid insertion point.</param>
        /// <param name="codePoint">A unicode code point to insert.</param>
        /// <returns>The location just after the inserted code point.</returns>
        public (int line, int column) Insert((int line, int column) location, int codePoint)
        {
            var line = lines[location.line];
            if ((uint)location.column > (uint)line.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(location.column));
            }
            switch (codePoint)
            {
                case '\n':
                    if (location.column == 0)
                    {
                        if (location.line > 0)
                        {
                            var previousLine = lines[location.line - 1];
                            if (previousLine[previousLine.Count - 1] == '\r')
                            {
                                previousLine.Insert(previousLine.Count, '\n');
                                lines[location.line - 1] = previousLine;
                                return location;
                            }
                        }
                    }
                    else if (location.column == line.Count)
                    {
                        if (line[location.column - 1] == '\r')
                        {
                            line.Insert(location.column, '\n');
                            lines[location.line] = line;
                            return location;
                        }
                    }
                    goto case 0x2028;

                case '\r':
                    if (location.column < line.Count && line[location.column] == '\n' &&
                        location.column - 1 > 0 && line[location.column - 1] != '\r')
                    {
                        line.Insert(location.column, codePoint);
                        return (location.line, location.column + 1);
                    }
                    goto case 0x2028;

                case 0x2028:
                case 0x2029:
                    var newLine = new CodePointBuffer(DefaultCapacity);
                    var n = 0;
                    for (int i = location.column; i < line.Count; i++)
                    {
                        newLine.Insert(n++, line[i]);
                    }
                    if (n > 0)
                    {
                        line.RemoveRange(location.column, n);
                    }
                    line.Insert(location.column, codePoint);
                    lines[location.line] = line;
                    lines.Insert(location.line + 1, newLine);
                    return (location.line + 1, 0);

                default:
                    line.Insert(location.column, codePoint);
                    lines[location.line] = line;
                    return (location.line, location.column + 1);
            }
        }

        /// <summary>
        /// Inserts the code points corresponding to the characters represented by <paramref name="text"/> at
        /// the supplied location in this text.
        /// </summary>
        /// <param name="location">A valid insertion point.</param>
        /// <param name="text">The text to insert.</param>
        /// <returns>The location just after the last inserted code point.</returns>
        public (int line, int column) Insert((int line, int column) location, string text)
        {
            int i = 0;
            while (TextHelper.TryGetCodePoint(text, ref i, out int c))
            {
                location = Insert(location, c);
            }
            return location;
        }

        /// <summary>
        /// Removes all code points between the two supplied locations in this text.
        /// </summary>
        /// <param name="start">A valid start location.</param>
        /// <param name="end">A valid end location.</param>
        public void RemoveRange((int line, int column) start, (int line, int column) end)
        {
            if (end.line < start.line)
            {
                throw new ArgumentOutOfRangeException(nameof(end.line));
            }

            var startLine = lines[start.line];
            if (end.line == start.line)
            {
                if (end.column < start.column)
                {
                    throw new ArgumentOutOfRangeException(nameof(end.column));
                }
                startLine.RemoveRange(start.column, end.column - start.column);
            }
            else
            {
                var endLine = lines[end.line];
                startLine.RemoveRange(start.column, startLine.Count - start.column);
                for (int i = end.column; i < endLine.Count; i++)
                {
                    startLine.Insert(startLine.Count, endLine[i]);
                }
                lines.RemoveRange(start.line + 1, end.line - start.line);
            }
            lines[start.line] = startLine;
        }

        /// <summary>
        /// Copies the code points composing the line at <paramref name="lineNumber"/> into the supplied string builder.
        /// </summary>
        /// <param name="lineNumber">A valid line number.</param>
        /// <param name="stringBuilder">The string builder to fill.</param>
        public void CopyLineTo(int lineNumber, StringBuilder stringBuilder)
        {
            var columnCount = GetColumnCount(lineNumber);
            for (int c = 0; c < columnCount; c++)
            {
                int cp = this[(lineNumber, c)];
                TextHelper.AppendCodePoint(stringBuilder, cp);
            }
        }

        /// <summary>
        /// Copies all the contents of this text into the supplied string builder.
        /// </summary>
        /// <param name="stringBuilder">The string builder to fill.</param>
        public void CopyTo(StringBuilder stringBuilder)
        {
            var lineCount = LineCount;
            for (int r = 0; r < lineCount; r++)
            {
                CopyLineTo(r, stringBuilder);
            }
        }

        /// <summary>
        /// Reads at most <paramref name="count"/> characters starting from the supplied <paramref name="location"/>.
        /// </summary>
        /// <param name="buffer">The target buffer.</param>
        /// <param name="index">A valid start index in <paramref name="buffer"/>.</param>
        /// <param name="count">The maximum number of characters to read.</param>
        /// <param name="location">The location where the first character to read is located.</param>
        /// <returns>The number of character actually read.</returns>
        public int Read(char[] buffer, int index, int count, ref (int line, int column) location)
        {
            var lineCount = LineCount;
            var line = location.line;
            if (line >= lineCount)
            {
                return 0;
            }
            var columnCount = GetColumnCount(line);
            var column = location.column;
            if (column > columnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(location.column));
            }
            var i = 0;
            for (;;)
            {
                if (column == columnCount)
                {
                    if (++line >= lineCount)
                    {
                        break;
                    }
                    column = 0;
                    location = (line, column);
                    columnCount = GetColumnCount(line);
                }
                while (column < columnCount && i < count)
                {
                    int codePoint = this[(line, column)];
                    switch (TextHelper.ConvertToUtf16(codePoint, out var c1, out var c2))
                    {
                        case 0:
                            throw new ArgumentOutOfRangeException(nameof(codePoint));
                        case 1:
                            buffer[index + i++] = c1;
                            break;
                        case 2:
                            if (index + i + 1 < count)
                            {
                                buffer[index + i++] = c1;
                                buffer[index + i++] = c2;
                            }
                            else
                            {
                                location = (line, column);
                                return i;
                            }
                            break;
                    }
                    column++;
                }
                if (i == count)
                {
                    break;
                }
            }
            location = (line, column);
            return i;
        }
    }
}
