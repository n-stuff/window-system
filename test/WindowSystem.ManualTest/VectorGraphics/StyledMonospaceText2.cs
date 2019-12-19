using NStuff.GraphicsBackend;
using NStuff.Text;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    public class StyledMonospaceText2 : MonospaceText<byte>
    {
        public MonospaceTextStyles2 Styles { get; }

        public MonospaceTextStyle2 DefaultStyle => Styles.GetStyle(0);

        public new(int codePoint, MonospaceTextStyle2 style) this[(int line, int column) location] {
            get {
                (var codePoint, var index) = ((MonospaceText<byte>)this)[location];
                return (codePoint, Styles.GetStyle(index));
            }
            set {
                ((MonospaceText<byte>)this)[location] = (value.codePoint, (byte)Styles.GetStyleIndex(value.style));
            }
        }

        public StyledMonospaceText2(DecoratedText<byte> decoratedText, MonospaceTextStyles2 styles) : base(decoratedText) => Styles = styles;

        public void StyleRange(MonospaceTextStyle2 style, (int line, int column) start, (int line, int column) end) =>
            DecorateRange((byte)Styles.GetStyleIndex(style), start, end);

        internal void Draw(DrawingContext2 context, AffineTransform transform, string fontFamily, double fontPoints,
            int firstLine, int lineCount, double lineHeight = -1d)
        {
            var fontMetrics = context.GlyphAtlas.FontMetrics;
            var fontInfo = fontMetrics.GetFontInfo(fontFamily, DefaultStyle.FontSubfamily);
            var ascent = fontMetrics.GetAscent(fontInfo, fontPoints);
            var descent = fontMetrics.GetDescent(fontInfo, fontPoints);
            var pixelScaling = context.PixelScaling;
            if (lineHeight < 0f)
            {
                lineHeight = ascent - descent;
                lineHeight += fontMetrics.GetLineGap(fontInfo, fontPoints);
            }
            else
            {
                lineHeight *= pixelScaling;
            }

            var columnWidth = fontMetrics.GetAdvanceWidth(
                fontMetrics.GetGlyphInfo(fontFamily, DefaultStyle.FontSubfamily, 'e'), fontPoints);
            var previousStyle = default(MonospaceTextStyle2);
            var vertexBuffer = context.Vertices;
            var vertexCount = 0;
            var firstDraw = true;
            var x = 0d;
            var y = 0d;
            for (int i = firstLine; i < firstLine + lineCount; i++)
            {
                var columnCount = GetFormattedColumnCount(i);
                for (int j = 0; j < columnCount; j++)
                {
                    var location = LocationToText((i, j));
                    (var _, var style) = this[location];
                    if (previousStyle != style)
                    {
                        if (vertexCount > 0)
                        {
                            DrawRectangles(context, previousStyle!.Background, transform, ref firstDraw, ref vertexCount);
                        }
                    }
                    if (style.Background.Alpha > 0)
                    {
                        var x0 = x / pixelScaling;
                        var y0 = (y - ascent) / pixelScaling;
                        var x1 = (x + columnWidth) / pixelScaling;
                        var y1 = (y - descent) / pixelScaling;
                        vertexBuffer[vertexCount++] = new PointCoordinates(x0, y0);
                        vertexBuffer[vertexCount++] = new PointCoordinates(x0, y1);
                        vertexBuffer[vertexCount++] = new PointCoordinates(x1, y1);
                        vertexBuffer[vertexCount++] = new PointCoordinates(x1, y1);
                        vertexBuffer[vertexCount++] = new PointCoordinates(x1, y0);
                        vertexBuffer[vertexCount++] = new PointCoordinates(x0, y0);
                        if (vertexCount + 6 > vertexBuffer.Length)
                        {
                            DrawRectangles(context, style.Background, transform, ref firstDraw, ref vertexCount);
                        }
                    }
                    x += columnWidth;
                    previousStyle = style;
                }
                x = 0d;
                y += lineHeight;
            }
            if (vertexCount > 0)
            {
                DrawRectangles(context, previousStyle!.Background, transform, ref firstDraw, ref vertexCount);
            }

            previousStyle = default;
            x = 0d;
            y = 0d;
            vertexCount = 0;
            firstDraw = true;
            int imageIndex = -1;
            for (int i = firstLine; i < firstLine + lineCount; i++)
            {
                var columnCount = GetFormattedColumnCount(i);
                for (int j = 0; j < columnCount; j++)
                {
                    var location = LocationToText((i, j));
                    (var codePoint, var style) = this[location];
                    if (previousStyle != style)
                    {
                        if (vertexCount > 0)
                        {
                            DrawCharacters(context, previousStyle!.Foreground, transform, imageIndex, ref firstDraw, ref vertexCount);
                        }
                    }
                    if (style.Foreground.Alpha > 0)
                    {
                        var glyph = context.GetGlyph(fontFamily, style.FontSubfamily, fontPoints, codePoint);
                        if ((imageIndex >= 0 && imageIndex != glyph.Index) || vertexCount + 6 > context.TexturedVertices.Length)
                        {
                            DrawCharacters(context, style.Foreground, transform, imageIndex, ref firstDraw, ref vertexCount);
                        }
                        context.ComputeGlyphVertices(glyph, x, y, ref vertexCount);
                        imageIndex = glyph.Index;
                        if (vertexCount + 6 > context.TexturedVertices.Length)
                        {
                            DrawCharacters(context, style.Foreground, transform, imageIndex, ref firstDraw, ref vertexCount);
                        }
                    }
                    x += columnWidth;
                    previousStyle = style;
                }
                x = 0d;
                y += lineHeight;
            }
            if (vertexCount > 0)
            {
                DrawCharacters(context, previousStyle!.Foreground, transform, imageIndex, ref firstDraw, ref vertexCount);
            }
        }

        private static void DrawRectangles(DrawingContext2 context, RgbaColor color, AffineTransform transform,
            ref bool firstDraw, ref int vertexCount)
        {
            var commandCount = 0;
            if (firstDraw)
            {
                context.CommandBuffers[commandCount++] = context.SetupPlainColorCommandBuffer;
                firstDraw = false;
            }
            context.Colors[0] = color;
            context.Backend.UpdateUniformBuffer(context.SingleColorBuffer, context.Colors, 0, 1);
            context.Transforms[0] = transform;
            context.Backend.UpdateUniformBuffer(context.SingleTransformBuffer, context.Transforms, 0, 1);
            context.Backend.UpdateVertexBuffer(context.VertexBuffer, context.Vertices, 0, vertexCount);
            context.VertexRanges[0] = new VertexRange(0, vertexCount);
            context.Backend.UpdateVertexRangeBuffer(context.SingleVertexRangeBuffer, context.VertexRanges, 0, 1);

            context.CommandBuffers[commandCount++] = context.DrawIndirectCommandBuffer;
            context.Backend.SubmitCommands(context.CommandBuffers, 0, commandCount);
            vertexCount = 0;
        }

        private static void DrawCharacters(DrawingContext2 context, RgbaColor color, AffineTransform transform,
            int imageIndex, ref bool firstDraw, ref int vertexCount)
        {
            var commandCount = 0;
            if (firstDraw)
            {
                context.CommandBuffers[commandCount++] = context.SetupGreyscaleImageColorCommandBuffer;
                firstDraw = false;
            }
            context.CommandBuffers[commandCount++] = context.BindGlyphImageCommandBuffers[imageIndex];
            context.Colors[0] = color;
            context.Backend.UpdateUniformBuffer(context.SingleColorBuffer, context.Colors, 0, 1);
            context.Transforms[0] = transform;
            context.Backend.UpdateUniformBuffer(context.SingleTransformBuffer, context.Transforms, 0, 1);
            context.Backend.UpdateVertexBuffer(context.TexturedVertexBuffer, context.TexturedVertices, 0, vertexCount);
            context.VertexRanges[0] = new VertexRange(0, vertexCount);
            context.Backend.UpdateVertexRangeBuffer(context.SingleVertexRangeBuffer, context.VertexRanges, 0, 1);

            context.CommandBuffers[commandCount++] = context.DrawIndirectCommandBuffer;
            context.Backend.SubmitCommands(context.CommandBuffers, 0, commandCount);
            vertexCount = 0;
        }
    }
}
