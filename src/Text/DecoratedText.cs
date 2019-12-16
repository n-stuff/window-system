using System;
using System.Collections.Generic;
using System.Text;

namespace NStuff.Text
{
    /// <summary>
    /// Represents some text organized in lines and columns, with generic decoration for each code point.
    /// </summary>
    /// <typeparam name="TDecoration">The type of some decoration to apply to each code point.</typeparam>
    public class DecoratedText<TDecoration>
    {
        private readonly PlainText plainText = new PlainText();
        private readonly GapBuffer<GapBuffer<TDecoration>> lines;

        /// <summary>
        /// Gets the number of lines of this text.
        /// </summary>
        public int LineCount => lines.Count;

        /// <summary>
        /// Gets a number that is incremented each time the text is modified.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Gets or sets the code point and its decoration at the specified location in this text.
        /// </summary>
        /// <param name="location">A valid location in this text.</param>
        /// <returns>The code point and decoration at the specified location.</returns>
        public (int codePoint, TDecoration decoration) this[(int line, int column) location] {
            get {
                var line = lines[location.line];
                return (plainText[location], line[location.column]);
            }
            set {
                plainText[location] = value.codePoint;
                lines[location.line][location.column] = value.decoration;
                Version++;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoratedText{TDecoration}"/> class.
        /// </summary>
        public DecoratedText()
        {
            lines = new GapBuffer<GapBuffer<TDecoration>>(PlainText.DefaultCapacity);
            lines.Insert(0, new GapBuffer<TDecoration>(PlainText.DefaultCapacity));
        }

        /// <summary>
        /// Gets the size of the code points that compose the supplied line.
        /// </summary>
        /// <param name="lineNumber">The number of a valid line.</param>
        /// <returns><c>1</c>, <c>2</c>, or <c>3</c>.</returns>
        public int GetLineMaxCodePointSize(int lineNumber) => plainText.GetLineMaxCodePointSize(lineNumber);

        /// <summary>
        /// Gets the number of code points of the supplied line.
        /// </summary>
        /// <param name="lineNumber">The number of a valid line.</param>
        /// <param name="ignoreNewline">A value indicating whether the trailing new line characters should ignored when
        /// counting the code points.</param>
        /// <returns>A number of code points.</returns>
        public int GetColumnCount(int lineNumber, bool ignoreNewline = false) => plainText.GetColumnCount(lineNumber, ignoreNewline);

        /// <summary>
        /// Inserts <paramref name="codePoint"/> at the supplied location in this text.
        /// </summary>
        /// <param name="location">A valid insertion point.</param>
        /// <param name="codePoint">A unicode code point to insert.</param>
        /// <returns>The location just after the inserted code point.</returns>
        public (int line, int column) Insert((int line, int column) location, int codePoint)
        {
            var result = plainText.Insert(location, codePoint);
            var line = lines[location.line];
            if (result.line == location.line)
            {
                if (result.column > location.column)
                {
                    TDecoration decoration;
                    if (location.column == 0 || location.column == line.Count)
                    {
                        decoration = default!;
                    }
                    else
                    {
                        if (EqualityComparer<TDecoration>.Default.Equals(line[location.column - 1], line[location.column]))
                        {
                            decoration = line[location.column];
                        }
                        else
                        {
                            decoration = default!;
                        }
                    }
                    line.Insert(location.column, decoration);
                }
                else
                {
                    var previousLine = lines[location.line - 1];
                    previousLine.Insert(previousLine.Count, default!);
                }
            }
            else
            {
                if (lines.Count == plainText.LineCount)
                {
                    line.Insert(line.Count, default!);
                }
                else
                {
                    var newLine = new GapBuffer<TDecoration>(PlainText.DefaultCapacity);
                    var n = 0;
                    for (int i = location.column; i < line.Count; i++)
                    {
                        newLine.Insert(n++, line[i]);
                    }
                    if (n > 0)
                    {
                        line.RemoveRange(location.column, n);
                    }
                    line.Insert(location.column, default!);
                    lines.Insert(location.line + 1, newLine);
                }
            }
            Version++;
            return result;
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
        /// Applies the specified decoration to the code points located between <paramref name="start"/> and <paramref name="end"/>.
        /// </summary>
        /// <param name="decoration">The decoration to apply to the text range.</param>
        /// <param name="start">The location of the first character to decorate.</param>
        /// <param name="end">The location just after the last character to decorate.</param>
        public void DecorateRange(TDecoration decoration, (int line, int column) start, (int line, int column) end)
        {
            if (end.line < start.line)
            {
                throw new ArgumentOutOfRangeException(nameof(end.line));
            }
            var startLine = lines[start.line];
            var changed = false;
            if (end.line == start.line)
            {
                if (end.column < start.column)
                {
                    throw new ArgumentOutOfRangeException(nameof(end.column));
                }
                for (int i = start.column; i < end.column; i++)
                {
                    startLine[i] = decoration;
                    changed = true;
                }
            }
            else
            {
                var length = startLine.Count;
                for (int i = start.column; i < length; i++)
                {
                    startLine[i] = decoration;
                    changed = true;
                }
                for (int j = start.line + 1; j < end.line; j++)
                {
                    var line = lines[j];
                    for (int i = 0; i < line.Count; i++)
                    {
                        line[i] = decoration;
                        changed = true;
                    }
                }
                var endLine = lines[end.line];
                for (int i = 0; i < end.column; i++)
                {
                    endLine[i] = decoration;
                    changed = true;
                }
            }
            if (changed)
            {
                Version++;
            }
        }

        /// <summary>
        /// Removes all code points between the two supplied locations in this text.
        /// </summary>
        /// <param name="start">A valid start location.</param>
        /// <param name="end">A valid end location.</param>
        public void RemoveRange((int line, int column) start, (int line, int column) end)
        {
            plainText.RemoveRange(start, end);
            var startLine = lines[start.line];
            if (end.line == start.line)
            {
                var length = end.column - start.column;
                if (length > 0)
                {
                    startLine.RemoveRange(start.column, length);
                    Version++;
                }
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
                Version++;
            }
            lines[start.line] = startLine;
        }

        /// <summary>
        /// Copies the code points composing the line at <paramref name="lineNumber"/> into the supplied string builder.
        /// </summary>
        /// <param name="lineNumber">A valid line number.</param>
        /// <param name="stringBuilder">The string builder to fill.</param>
        public void CopyLineTo(int lineNumber, StringBuilder stringBuilder) => plainText.CopyLineTo(lineNumber, stringBuilder);

        /// <summary>
        /// Copies all the contents of this text into the supplied string builder.
        /// </summary>
        /// <param name="stringBuilder">The string builder to fill.</param>
        public void CopyTo(StringBuilder stringBuilder) => plainText.CopyTo(stringBuilder);

        /// <summary>
        /// Reads at most <paramref name="count"/> characters starting from the supplied <paramref name="location"/>.
        /// </summary>
        /// <param name="buffer">The target buffer.</param>
        /// <param name="index">A valid start index in <paramref name="buffer"/>.</param>
        /// <param name="count">The maximum number of characters to read.</param>
        /// <param name="location">The location where the first character to read is located.</param>
        /// <returns>The number of character actually read.</returns>
        public int Read(char[] buffer, int index, int count, ref (int line, int column) location) =>
            plainText.Read(buffer, index, count, ref location);
    }
}
