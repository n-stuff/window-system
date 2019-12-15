using NStuff.GraphicsBackend;
using NStuff.Text;
using NStuff.Typography.Font;
using NStuff.Typography.Typesetting;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    public class LabelDrawing : DrawingBase
    {
        public string? FontFamily { get; set; }

        public FontSubfamily? FontSubfamily { get; set; }

        public double FontPoints { get; set; }

        public string? Text { get; set; }

        public RgbaColor Color { get; set; } = new RgbaColor(0, 0, 0, 255);

        internal override void Draw(DrawingContext context)
        {
            if (FontFamily == null || FontSubfamily == null || FontPoints == 0 || string.IsNullOrEmpty(Text))
            {
                return;
            }
            var glyphLayout = new GlyphLayout(context.GlyphAtlas.FontMetrics)
            {
                FontFamily = FontFamily,
                FontSubFamily = FontSubfamily,
                FontPoints = FontPoints
            };
            var index = 0;
            var vertexCount = 0;
            var y = 0;
            int imageIndex = -1;
            while (TextHelper.TryGetCodePoint(Text, ref index, out var codePoint))
            {
                var x = glyphLayout.X;
                glyphLayout.Insert(codePoint);
                var glyph = context.GetGlyph(FontFamily, FontSubfamily.Value, FontPoints, codePoint);
                if ((imageIndex >= 0 && imageIndex != glyph.Index) || vertexCount + 6 > context.TexturedVertices.Length)
                {
                    DrawCharacters(context, Color, Transform, imageIndex, ref vertexCount);
                }
                context.ComputeGlyphVertices(glyph, x, y, ref vertexCount);
                imageIndex = glyph.Index;
            }
            if (vertexCount > 0)
            {
                DrawCharacters(context, Color, Transform, imageIndex, ref vertexCount);
            }
        }

        private static void DrawCharacters(DrawingContext context, RgbaColor color, AffineTransform transform, int imageIndex, ref int vertexCount)
        {
            var commandCount = 0;
            context.CommandBuffers[commandCount++] = context.SetupGreyscaleImageColorCommandBuffer;
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
