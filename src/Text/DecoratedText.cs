using System;
using System.Collections.Generic;

namespace NStuff.Text
{
    /// <summary>
    /// Represents some text organized in lines and columns, with generic decoration for each code point.
    /// </summary>
    /// <typeparam name="TDecoration">The type of some decoration to apply to each code point.</typeparam>
    public class DecoratedText<TDecoration> : PlainText
    {
        private readonly GapBuffer<GapBuffer<TDecoration>> lines;

        /// <summary>
        /// Gets a number that is incremented each time the text is modified.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Gets or sets the code point and its decoration at the specified location in this text.
        /// </summary>
        /// <param name="location">A valid location in this text.</param>
        /// <returns>The code point and decoration at the specified location.</returns>
        public new (int codePoint, TDecoration decoration) this[(int line, int column) location] {
            get {
                var line = lines[location.line];
                return (((PlainText)this)[location], line[location.column]);
            }
            set {
                ((PlainText)this)[location] = value.codePoint;
                lines[location.line][location.column] = value.decoration;
                Version++;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoratedText{TDecoration}"/> class.
        /// </summary>
        public DecoratedText()
        {
            lines = new GapBuffer<GapBuffer<TDecoration>>(DefaultCapacity);
            lines.Insert(0, new GapBuffer<TDecoration>(DefaultCapacity));
        }

        /// <summary>
        /// Initializes a new instance of the <c>DecoratedText</c> class using the supplied text.
        /// </summary>
        /// <param name="plainText">The text to copy.</param>
        public DecoratedText(PlainText plainText) : base(plainText)
        {
            var lineCount = plainText.LineCount;
            lines = new GapBuffer<GapBuffer<TDecoration>>((int)BitHelper.GetNextPowerOfTwo((uint)lineCount));
            for (int i = 0; i < lineCount; i++)
            {
                var columnCount = plainText.GetColumnCount(i);
                var line = new GapBuffer<TDecoration>((int)BitHelper.GetNextPowerOfTwo((uint)columnCount));
                for (int j = 0; j < columnCount; j++)
                {
                    line.Insert(j, default!);
                }
                lines.Insert(i, line);
            }
        }

        /// <summary>
        /// Inserts <paramref name="codePoint"/> at the supplied location in this text.
        /// </summary>
        /// <param name="location">A valid insertion point.</param>
        /// <param name="codePoint">A unicode code point to insert.</param>
        /// <returns>The location just after the inserted code point.</returns>
        public new (int line, int column) Insert((int line, int column) location, int codePoint)
        {
            var result = ((PlainText)this).Insert(location, codePoint);
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
                if (lines.Count == LineCount)
                {
                    line.Insert(line.Count, default!);
                }
                else
                {
                    var newLine = new GapBuffer<TDecoration>(DefaultCapacity);
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
        public new (int line, int column) Insert((int line, int column) location, string text)
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
        public new void RemoveRange((int line, int column) start, (int line, int column) end)
        {
            ((PlainText)this).RemoveRange(start, end);
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
    }
}
