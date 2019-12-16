using System;
using System.Collections.Generic;
using System.Text;

namespace NStuff.Text
{
    /// <summary>
    /// Represents some text organized in lines and columns, with generic decoration for each code point.
    /// It also provides layout informations computed from the maximum length of a line, as well as caret management.
    /// </summary>
    /// <typeparam name="TDecoration">The type of some decoration to apply to each code point.</typeparam>
    public class MonospaceText<TDecoration>
    {
        private readonly DecoratedText<TDecoration> decoratedText;
        private readonly List<(int line, int column)> lineToTextLocation = new List<(int line, int column)>();
        private readonly List<int> textLocationLineToLine = new List<int>();
        private (int line, int column) caretLocation;
        private int caretColumn;
        private bool checkCaretLocation;
        private int maxLineLength;

        /// <summary>
        /// Gets a number that is incremented each time the text is modified.
        /// </summary>
        public int Version => decoratedText.Version;

        /// <summary>
        /// Gets or sets the maximum line length, used to format the text.
        /// </summary>
        /// <value>A number of columns. <c>0</c> means that there is no limit.</value>
        public int MaxLineLength {
            get => maxLineLength;
            set {
                if (maxLineLength != value)
                {
                    lineToTextLocation.Clear();
                    textLocationLineToLine.Clear();
                    maxLineLength = (value <= 0) ? 0 : value;
                    checkCaretLocation = true;
                }
            }
        }

        /// <summary>
        /// Gets the number of lines of this text.
        /// </summary>
        /// <value>A positive number.</value>
        public int LineCount => decoratedText.LineCount;

        /// <summary>
        /// Gets the current caret location, in formatted coordinates.
        /// </summary>
        public (int line, int column) CaretLocation {
            get {
                UpdateCaretLocation();
                return caretLocation;
            }
            set {
                caretLocation = value;
                caretColumn = caretLocation.column;
                checkCaretLocation = true;
            }
        }

        /// <summary>
        /// Gets or sets the code point and its decoration at the specified location in this text.
        /// </summary>
        /// <param name="location">A valid location in this text.</param>
        /// <returns>The code point and decoration at the specified location.</returns>
        public (int codePoint, TDecoration decoration) this[(int line, int column) location] {
            get => decoratedText[location];
            set => decoratedText[location] = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonospaceText{TDecoration}"/> class.
        /// </summary>
        /// <param name="decoratedText">A text used as storage.</param>
        public MonospaceText(DecoratedText<TDecoration> decoratedText) => this.decoratedText = decoratedText;

        /// <summary>
        /// Gets the size in bytes of the code points that compose the specified line.
        /// </summary>
        /// <param name="lineNumber">A valid line number.</param>
        /// <returns>1, 2, or 3</returns>
        public int GetLineMaxCodePointSize(int lineNumber) => decoratedText.GetLineMaxCodePointSize(lineNumber);

        /// <summary>
        /// Inserts the character with the specified code point at the current location of the caret.
        /// </summary>
        /// <param name="codePoint">The unicode code point of the character to insert.</param>
        public void Insert(int codePoint)
        {
            UpdateCaretLocation();

            var location = LocationToText(caretLocation);
            location = Insert(location, codePoint);
            caretLocation = LocationFromText(location);
            caretColumn = caretLocation.column;
        }

        /// <summary>
        /// Removes the character just before the current position of the caret.
        /// </summary>
        public void Remove()
        {
            UpdateCaretLocation();

            var end = LocationToText(caretLocation);
            (int line, int column) start;
            if (end.column == 0)
            {
                if (end.line == 0)
                {
                    return;
                }
                var length = GetColumnCount(end.line - 1, ignoreNewline: true);
                start = (end.line - 1, length);
            }
            else
            {
                start = (end.line, end.column - 1);
            }
            RemoveRange(start, end);
            checkCaretLocation = false;
            caretLocation = LocationFromText(start);
            caretColumn = caretLocation.column;
        }

        /// <summary>
        /// Moves the caret one line down, if possible.
        /// </summary>
        public void MoveDown()
        {
            UpdateCaretLocation();

            var lineCount = GetFormattedLineCount();
            var line = Math.Min(lineCount - 1, caretLocation.line + 1);
            var columnCount = GetFormattedColumnCount(line);
            caretLocation = (line, Math.Min(columnCount, caretColumn));
        }

        /// <summary>
        /// Moves the caret one line up, if possible.
        /// </summary>
        public void MoveUp()
        {
            UpdateCaretLocation();

            var line = Math.Max(0, caretLocation.line - 1);
            var columnCount = GetFormattedColumnCount(line);
            caretLocation = (line, Math.Min(columnCount, caretColumn));
        }

        /// <summary>
        /// Moves the caret one column to the left, if possible.
        /// </summary>
        public void MoveLeft()
        {
            UpdateCaretLocation();

            var line = caretLocation.line;
            var column = Math.Min(caretLocation.column, GetFormattedColumnCount(line));
            if (column == 0)
            {
                if (--line >= 0)
                {
                    column = GetFormattedColumnCount(line);
                    if (lineToTextLocation[line].line == lineToTextLocation[line + 1].line)
                    {
                        column--;
                    }
                    caretLocation = (line, column);
                    caretColumn = column;
                }
            }
            else
            {
                column--;
                caretLocation = (line, column);
                caretColumn = column;
            }
        }

        /// <summary>
        /// Moves the caret one column to the right, if possible.
        /// </summary>
        public void MoveRight()
        {
            UpdateCaretLocation();

            var line = caretLocation.line;
            var columnCount = GetFormattedColumnCount(line);
            var column = caretLocation.column;
            if (column + 1 > columnCount)
            {
                var lineCount = GetFormattedLineCount();
                if (++line < lineCount)
                {
                    caretLocation = (line, 0);
                    caretColumn = 0;
                }
            }
            else if (column + 1 == columnCount
                && line + 1 < lineToTextLocation.Count
                && lineToTextLocation[line].line == lineToTextLocation[line + 1].line)
            {
                caretLocation = (line + 1, 0);
                caretColumn = 0;
            }
            else
            {
                column++;
                caretLocation = (line, column);
                caretColumn = column;
            }
        }

        private void UpdateCaretLocation()
        {
            if (checkCaretLocation)
            {
                var line = caretLocation.line;
                if (line < 0)
                {
                    line = 0;
                }
                else
                {
                    var lineCount = GetFormattedLineCount();
                    if (line >= lineCount)
                    {
                        line = lineCount - 1;
                    }
                }
                var columnCount = GetFormattedColumnCount(line);
                caretLocation = (line, Math.Min(columnCount, caretColumn));

                checkCaretLocation = false;
            }
        }

        /// <summary>
        /// Gets the number of code points of the supplied line.
        /// </summary>
        /// <param name="lineNumber">The number of a valid line.</param>
        /// <param name="ignoreNewline">A value indicating whether the trailing new line characters should ignored when
        /// counting the code points.</param>
        /// <returns>A number of code points.</returns>
        public int GetColumnCount(int lineNumber, bool ignoreNewline = false) => decoratedText.GetColumnCount(lineNumber, ignoreNewline);

        /// <summary>
        /// Gets the number of line of this text, after formatting was applied.
        /// </summary>
        /// <returns>A number of lines.</returns>
        public int GetFormattedLineCount()
        {
            if (maxLineLength == 0)
            {
                return LineCount;
            }
            UpdateMappingsToTextLines(decoratedText.LineCount);
            return lineToTextLocation.Count;
        }

        /// <summary>
        /// Gets the number of columns of the specified line of text, after formatting was applied.
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns>A number of columns.</returns>
        public int GetFormattedColumnCount(int lineNumber)
        {
            if (maxLineLength == 0)
            {
                return GetColumnCount(lineNumber, ignoreNewline: true);
            }
            UpdateMappingsToLines(lineNumber + 1);
            if (lineNumber >= lineToTextLocation.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(lineNumber));
            }
            var (line, column) = lineToTextLocation[lineNumber];
            var count = GetColumnCount(line, ignoreNewline: true) - column;
            return Math.Min(count, maxLineLength);
        }

        /// <summary>
        /// Inserts <paramref name="codePoint"/> at the supplied location in this text.
        /// </summary>
        /// <param name="location">A valid insertion point.</param>
        /// <param name="codePoint">A unicode code point to insert.</param>
        /// <returns>The location just after the inserted code point.</returns>
        public (int line, int column) Insert((int line, int column) location, int codePoint)
        {
            var result = decoratedText.Insert(location, codePoint);
            InvalidateOnInsert(location, result);
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
            var result = decoratedText.Insert(location, text);
            InvalidateOnInsert(location, result);
            return result;
        }

        private void InvalidateOnInsert((int line, int column) location, (int line, int column) result)
        {
            if (maxLineLength != 0 && location.line < textLocationLineToLine.Count)
            {
                if (result.line == location.line)
                {
                    if (result.column > location.column && location.line + 1 < textLocationLineToLine.Count)
                    {
                        var line = textLocationLineToLine[location.line + 1] - 1;
                        var textLocation = lineToTextLocation[line];
                        if ((decoratedText.GetColumnCount(location.line, ignoreNewline: true) - textLocation.column) > maxLineLength)
                        {
                            InvalidateFromLine(line + 1);
                        }
                    }
                }
                else
                {
                    var line = GetTextLocationIndex(location);
                    var textLocation = lineToTextLocation[line];
                    if (textLocation.column == location.column)
                    {
                        InvalidateFromLine(line);
                    }
                    else
                    {
                        InvalidateFromLine(line + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Applies the specified decoration to the code points located between <paramref name="start"/> and <paramref name="end"/>.
        /// </summary>
        /// <param name="decoration">The decoration to apply to the text range.</param>
        /// <param name="start">The location of the first character to decorate.</param>
        /// <param name="end">The location just after the last character to decorate.</param>
        public void DecorateRange(TDecoration decoration, (int line, int column) start, (int line, int column) end) =>
            decoratedText.DecorateRange(decoration, start, end);

        /// <summary>
        /// Removes all code points between the two supplied locations in this text.
        /// </summary>
        /// <param name="start">A valid start location.</param>
        /// <param name="end">A valid end location.</param>
        public void RemoveRange((int line, int column) start, (int line, int column) end)
        {
            if (maxLineLength != 0 && start.line < textLocationLineToLine.Count)
            {
                if (start.line == end.line && start.column < end.column)
                {
                    if (start.line + 1 < textLocationLineToLine.Count)
                    {
                        var line = textLocationLineToLine[start.line + 1] - 1;
                        var textLocation = lineToTextLocation[line];
                        var lastLineLength = decoratedText.GetColumnCount(start.line, ignoreNewline: true) - textLocation.column;
                        var removedColumns = end.column - start.column;
                        if (lastLineLength < removedColumns)
                        {
                            InvalidateFromLine(line - (removedColumns - lastLineLength) / maxLineLength);
                            checkCaretLocation = true;
                        }
                    }
                }
                else
                {
                    var line = GetTextLocationIndex(start);
                    var textLocation = lineToTextLocation[line];
                    if (textLocation.column == start.column)
                    {
                        InvalidateFromLine(line);
                    }
                    else
                    {
                        InvalidateFromLine(line + 1);
                    }
                    checkCaretLocation = true;
                }
            }
            decoratedText.RemoveRange(start, end);
        }

        private void InvalidateFromLine(int line)
        {
            var textLocation = lineToTextLocation[line];
            textLocationLineToLine.RemoveRange(textLocation.line, textLocationLineToLine.Count - textLocation.line);
            lineToTextLocation.RemoveRange(line, lineToTextLocation.Count - line);
        }

        /// <summary>
        /// Converts the specified raw text location, to its corresponding formatted text location.
        /// </summary>
        /// <param name="location">A valid location in the raw text.</param>
        /// <returns>A location in the formatted text.</returns>
        public (int line, int column) LocationFromText((int line, int column) location)
        {
            if (location.line >= decoratedText.LineCount)
            {
                throw new ArgumentOutOfRangeException(nameof(location.line));
            }
            if (location.column > decoratedText.GetColumnCount(location.line, ignoreNewline: true))
            {
                throw new ArgumentOutOfRangeException(nameof(location.column));
            }
            if (maxLineLength == 0)
            {
                return location;
            }
            UpdateMappingsToTextLines(location.line + 1);
            var line = GetTextLocationIndex(location);
            return (line, location.column - lineToTextLocation[line].column);
        }

        /// <summary>
        /// Converts the specified location in the formatted text, to its corresponding location in the raw text.
        /// </summary>
        /// <param name="location">A valid location in the formatted text.</param>
        /// <returns>A location in the raw text.</returns>
        public (int line, int column) LocationToText((int line, int column) location)
        {
            if (maxLineLength == 0)
            {
                if (location.line >= decoratedText.LineCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(location.line));
                }
                if (location.column > decoratedText.GetColumnCount(location.line, ignoreNewline: true))
                {
                    throw new ArgumentOutOfRangeException(nameof(location.column));
                }
                return location;
            }
            UpdateMappingsToLines(location.line + 1);
            if (location.line >= lineToTextLocation.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(location.line));
            }
            var textLocation = lineToTextLocation[location.line];
            var column = textLocation.column + location.column;
            if (column > decoratedText.GetColumnCount(textLocation.line, ignoreNewline: true))
            {
                throw new ArgumentOutOfRangeException(nameof(location.column));
            }
            return (textLocation.line, column);
        }

        /// <summary>
        /// Copies the code points composing the line at <paramref name="lineNumber"/> into the supplied string builder.
        /// </summary>
        /// <param name="lineNumber">A valid line number.</param>
        /// <param name="stringBuilder">The string builder to fill.</param>
        public void CopyLineTo(int lineNumber, StringBuilder stringBuilder) => decoratedText.CopyLineTo(lineNumber, stringBuilder);

        /// <summary>
        /// Copies the code points composing the formatted line at <paramref name="lineNumber"/> into the supplied string builder.
        /// </summary>
        /// <param name="line">The number of the formatted line to copy.</param>
        /// <param name="stringBuilder">The destination <see cref="StringBuilder"/>.</param>
        public void CopyFormattedLineTo(int line, StringBuilder stringBuilder)
        {
            var columnCount = GetFormattedColumnCount(line);
            for (int i = 0; i < columnCount; i++)
            {
                var location = LocationToText((line, i));
                var codePoint = this[location].codePoint;
                TextHelper.AppendCodePoint(stringBuilder, codePoint);
            }
        }

        /// <summary>
        /// Copies all the contents of this text into the supplied string builder.
        /// </summary>
        /// <param name="stringBuilder">The string builder to fill.</param>
        public void CopyTo(StringBuilder stringBuilder) => decoratedText.CopyTo(stringBuilder);

        /// <summary>
        /// Reads at most <paramref name="count"/> characters starting from the supplied <paramref name="location"/>.
        /// </summary>
        /// <param name="buffer">The target buffer.</param>
        /// <param name="index">A valid start index in <paramref name="buffer"/>.</param>
        /// <param name="count">The maximum number of characters to read.</param>
        /// <param name="location">The location where the first character to read is located.</param>
        /// <returns>The number of character actually read.</returns>
        public int Read(char[] buffer, int index, int count, ref (int line, int column) location) =>
            decoratedText.Read(buffer, index, count, ref location);

        private int GetTextLocationIndex((int line, int column) location)
        {
            int index = textLocationLineToLine[location.line] + 1;
            while (index < lineToTextLocation.Count)
            {
                var (line, column) = lineToTextLocation[index];
                if (line > location.line || column > location.column)
                {
                    break;
                }
                index++;
            }
            return index - 1;
        }

        private void UpdateMappingsToTextLines(int lineCount)
        {
            var location = GetLastValidLocation();
            while (location.line < lineCount)
            {
                location = GetNextTextLocation(location);
                if (location.line >= decoratedText.LineCount)
                {
                    break;
                }
                lineToTextLocation.Add(location);
            }
            UpdateTextLocationStartToLine();
        }

        private void UpdateMappingsToLines(int lineCount)
        {
            if (lineToTextLocation.Count >= lineCount)
            {
                return;
            }
            var location = GetLastValidLocation();
            while (lineToTextLocation.Count < lineCount)
            {
                location = GetNextTextLocation(location);
                if (location.line >= decoratedText.LineCount)
                {
                    break;
                }
                lineToTextLocation.Add(location);
            }
            UpdateTextLocationStartToLine();
        }

        private void UpdateTextLocationStartToLine()
        {
            int line = textLocationLineToLine.Count;
            int index = (line == 0) ? 0 : textLocationLineToLine[line - 1];
            while (index < lineToTextLocation.Count && line < decoratedText.LineCount)
            {
                if (line == lineToTextLocation[index].line)
                {
                    line++;
                    textLocationLineToLine.Add(index);
                }
                index++;
            }
        }

        private (int line, int column) GetLastValidLocation()
        {
            (int line, int column) location;
            if (lineToTextLocation.Count == 0)
            {
                location = default;
                lineToTextLocation.Add(location);
            }
            else
            {
                location = lineToTextLocation[lineToTextLocation.Count - 1];
            }
            return location;
        }

        private (int line, int column) GetNextTextLocation((int line, int column) location)
        {
            var count = decoratedText.GetColumnCount(location.line, ignoreNewline: true) - location.column;
            if (count <= maxLineLength)
            {
                return (location.line + 1, 0);
            }
            else
            {
                count = maxLineLength;
                return (location.line, location.column + count);
            }
        }
    }
}
