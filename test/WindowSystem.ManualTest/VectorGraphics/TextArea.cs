﻿using System;
using System.IO;
using NStuff.GraphicsBackend;
using NStuff.Text;
using NStuff.Typography.Font;
using NStuff.VectorGraphics;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    public class TextArea
    {
        private readonly string fontFamily;
        private readonly FontSubfamily fontSubfamily;
        private StyledMonospaceText text;
        private double fontPoints;
        private double renderX;
        private double renderY;
        private double renderWidth;
        private double renderHeight;
        private double topOffset;
        private bool hideCaret;
        private bool caretMoved;
        private double scrollDeltaY;
        private int caretDeltaLine;
        private int caretDeltaColumn;
        private double mousePressedX = -1;
        private double mousePressedY = -1;

        public double FontPoints {
            get => fontPoints;
            set {
                if (value != fontPoints)
                {
                    fontPoints = value;
                    RequireRender = true;
                }
            }
        }

        public bool RequireRender { get; private set; }
        public bool RequireRestyle { get; private set; }

        public bool HideCaret {
            get => hideCaret;
            set {
                if (hideCaret != value)
                {
                    hideCaret = value;
                    RequireRender = true;
                }
            }
        }

        public int Version => text.Version;

        public TextArea(MonospaceTextStyles styles, string fontFamily, FontSubfamily fontSubfamily, double fontPoints)
        {
            this.fontFamily = fontFamily;
            this.fontSubfamily = fontSubfamily;
            this.fontPoints = fontPoints;

            text = new StyledMonospaceText(new DecoratedText<byte>(), styles);
        }

        public void Load(string path)
        {
            text.Insert(default, File.ReadAllText(path));
            RequireRender = true;
            RequireRestyle = true;
        }

        public DecoratedText<byte> GetText() => new DecoratedText<byte>(text.DecoratedText);

        public void SetText(StyledMonospaceText text)
        {
            text.CaretLocation = this.text.CaretLocation;
            this.text = text;
            RequireRender = true;
            RequireRestyle = false;
        }

        public void SetViewRectangle(double left, double top, double width, double height)
        {
            renderX = left;
            renderY = top;
            renderWidth = width;
            renderHeight = height;
            RequireRender = true;
        }

        public void ScrollVertically(double increment)
        {
            scrollDeltaY += increment;
            RequireRender = true;
        }

        public void MoveUp()
        {
            caretDeltaLine--;
            RequireRender = true;
            caretMoved = true;
        }

        public void MoveDown()
        {
            caretDeltaLine++;
            RequireRender = true;
            caretMoved = true;
        }

        public void MoveLeft()
        {
            caretDeltaColumn--;
            RequireRender = true;
            caretMoved = true;
        }

        public void MoveRight()
        {
            caretDeltaColumn++;
            RequireRender = true;
            caretMoved = true;
        }

        public void LeftMouseDown(double x, double y)
        {
            mousePressedX = x - renderX;
            mousePressedY = y - renderY;
            RequireRender = true;
        }

        public void Backspace()
        {
            text.Remove();
            RequireRender = true;
            caretMoved = true;
            RequireRestyle = true;
        }

        public void Tab()
        {
            for (int i = 0; i < 4; i++)
            {
                text.Insert(' ');
            }
            RequireRender = true;
            caretMoved = true;
            RequireRestyle = true;
        }

        public void Enter()
        {
            text.Insert('\n');
            RequireRender = true;
            caretMoved = true;
            RequireRestyle = true;
        }

        public void Insert(int codePoint)
        {
            text.Insert(codePoint);
            RequireRender = true;
            caretMoved = true;
            RequireRestyle = true;
        }

        public void Render(DrawingContext drawingContext)
        {
            RequireRender = false;

            var pixelScaling = drawingContext.SharedContext.PixelScaling;

            var fontMetrics = drawingContext.SharedContext.FontMetrics;
            var advanceWidth = fontMetrics.GetAdvanceWidth(fontMetrics.GetGlyphInfo(fontFamily, fontSubfamily, 'e'), fontPoints);
            text.MaxLineLength = (int)Math.Floor(renderWidth * pixelScaling / advanceWidth);

            var fontInfo = fontMetrics.GetFontInfo(fontFamily, fontSubfamily);
            var ascent = fontMetrics.GetAscent(fontInfo, fontPoints);
            var descent = fontMetrics.GetDescent(fontInfo, fontPoints);
            var lineGap = fontMetrics.GetLineGap(fontInfo, fontPoints);

            var lineCount = text.GetFormattedLineCount();
            var lineHeight = Math.Ceiling(ascent - descent + lineGap + pixelScaling);
            var renderLineCount = (int)Math.Floor(renderHeight * pixelScaling / lineHeight);
            var columnWidth = advanceWidth;

            if (scrollDeltaY != 0)
            {
                topOffset = Math.Min(Math.Max(0, topOffset + scrollDeltaY), (lineCount - 1) * lineHeight);
                scrollDeltaY = 0;
            }

            (int line, int column) caretLocation;
            if (caretMoved)
            {
                caretMoved = false;
                if (caretDeltaLine > 0)
                {
                    do
                    {
                        text.MoveDown();
                    }
                    while (--caretDeltaLine > 0);
                }
                else if (caretDeltaLine < 0)
                {
                    do
                    {
                        text.MoveUp();
                    }
                    while (++caretDeltaLine < 0);
                }
                if (caretDeltaColumn > 0)
                {
                    do
                    {
                        text.MoveRight();
                    }
                    while (--caretDeltaColumn > 0);
                }
                else if (caretDeltaColumn < 0)
                {
                    do
                    {
                        text.MoveLeft();
                    }
                    while (++caretDeltaColumn < 0);
                }

                caretLocation = text.CaretLocation;
                if (caretLocation.line >= (int)Math.Round(topOffset / lineHeight) + renderLineCount - 1)
                {
                    topOffset = (caretLocation.line - renderLineCount + 1) * lineHeight;
                }
                else if (caretLocation.line < (int)Math.Round(topOffset / lineHeight))
                {
                    topOffset = caretLocation.line * lineHeight;
                }
            }

            var firstLine = (int)Math.Round(topOffset / lineHeight);
            var verticalOffset = topOffset - firstLine * lineHeight;

            if (mousePressedX > 0 && mousePressedY > 0 &&
                mousePressedX < renderWidth && mousePressedY < renderHeight)
            {
                var column = (int)Math.Floor(mousePressedX * pixelScaling / columnWidth);
                var line = firstLine + (int)Math.Floor((mousePressedY + verticalOffset) * pixelScaling / lineHeight);
                if (line >= lineCount)
                {
                    line = lineCount - 1;
                }
                var columns = text.GetFormattedColumnCount(line);
                if (column >= columns)
                {
                    column = columns;
                }
                text.CaretLocation = (line, column);
                mousePressedX = -1;
                mousePressedY = -1;
            }

            var firstGlyphX = 0d;
            var firstGlyphY = -verticalOffset * pixelScaling + descent;

            caretLocation = text.CaretLocation;
            var caretTop = (caretLocation.line - firstLine) * lineHeight - verticalOffset * pixelScaling;
            var caretLeft = caretLocation.column * columnWidth;

            if (firstLine == 0)
            {
                firstGlyphY += lineHeight;
            }
            else
            {
                firstLine--;
            }

            var transform = new AffineTransform(
                m11: 1,
                m22: 1,
                m31: renderX + firstGlyphX / pixelScaling,
                m32: renderY + firstGlyphY / pixelScaling
            );

            text.Draw(drawingContext, transform, fontFamily, fontPoints, firstLine,
                (int)Math.Min(lineCount - firstLine, Math.Ceiling((renderHeight * pixelScaling) / lineHeight + 1)),
                lineHeight / pixelScaling);

            if (!hideCaret)
            {
                var pathDrawing = new PathDrawing()
                {
                    Transform = new AffineTransform(m11: 1, m22: 1, m31: renderX, m32: renderY),
                    FillColor = new RgbaColor(255, 255, 128, 255)
                };

                double x0 = caretLeft / pixelScaling - 1;
                double y0 = caretTop / pixelScaling;
                double x1 = x0 + 2;
                double y1 = (caretTop + lineHeight) / pixelScaling;

                pathDrawing.Move((x0, y0));
                pathDrawing.AddLine((x0, y1));
                pathDrawing.AddLine((x1, y1));
                pathDrawing.AddLine((x1, y0));

                pathDrawing.Draw(drawingContext);
            }
        }
    }
}
