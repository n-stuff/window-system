using NStuff.GraphicsBackend;
using NStuff.Text;
using NStuff.VectorGraphics;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    public class StyledMonospaceText : MonospaceText<byte>
    {
        public MonospaceTextStyles Styles { get; }

        public MonospaceTextStyle DefaultStyle => Styles.GetStyle(0);

        public new(int codePoint, MonospaceTextStyle style) this[(int line, int column) location] {
            get {
                (var codePoint, var index) = ((MonospaceText<byte>)this)[location];
                return (codePoint, Styles.GetStyle(index));
            }
            set {
                ((MonospaceText<byte>)this)[location] = (value.codePoint, (byte)Styles.GetStyleIndex(value.style));
            }
        }

        public StyledMonospaceText(DecoratedText<byte> decoratedText, MonospaceTextStyles styles) : base(decoratedText) => Styles = styles;

        public void StyleRange(MonospaceTextStyle style, (int line, int column) start, (int line, int column) end) =>
            DecorateRange((byte)Styles.GetStyleIndex(style), start, end);

        internal void Draw(DrawingContext context, AffineTransform transform, string fontFamily, double fontPoints,
            int firstLine, int lineCount, double lineHeight = -1d)
        {
            var fontMetrics = context.SharedContext.FontMetrics;
            var fontInfo = fontMetrics.GetFontInfo(fontFamily, DefaultStyle.FontSubfamily);
            var ascent = fontMetrics.GetAscent(fontInfo, fontPoints);
            var descent = fontMetrics.GetDescent(fontInfo, fontPoints);
            var pixelScaling = context.SharedContext.PixelScaling;
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
            var x = 0d;
            var y = 0d;
            var pathDrawing = new PathDrawing()
            {
                Transform = transform
            };
            for (int i = firstLine; i < firstLine + lineCount; i++)
            {
                var columnCount = GetFormattedColumnCount(i);
                for (int j = 0; j < columnCount; j++)
                {
                    var location = LocationToText((i, j));
                    (var _, var style) = this[location];
                    if (style.Background.Alpha > 0)
                    {
                        pathDrawing.ClearCurves();
                        pathDrawing.FillColor = style.Background;
                        var x0 = x / pixelScaling;
                        var y0 = (y - ascent) / pixelScaling;
                        var x1 = (x + columnWidth) / pixelScaling;
                        var y1 = (y - descent) / pixelScaling;
                        pathDrawing.Move((x0, y0));
                        pathDrawing.AddLine((x0, y1));
                        pathDrawing.AddLine((x1, y1));
                        pathDrawing.AddLine((x1, y0));
                        pathDrawing.Draw(context);
                    }
                    x += columnWidth;
                }
                x = 0d;
                y += lineHeight;
            }

            x = 0d;
            y = 0d;
            var labelDrawing = new LabelDrawing()
            {
                FontFamily = fontFamily,
                FontPoints = fontPoints
            };
            for (int i = firstLine; i < firstLine + lineCount; i++)
            {
                var columnCount = GetFormattedColumnCount(i);
                MonospaceTextStyle? previousStyle = null;
                for (int j = 0; j < columnCount; j++)
                {
                    var location = LocationToText((i, j));
                    (var codePoint, var style) = this[location];
                    if (previousStyle != null && previousStyle != style)
                    {
                        labelDrawing.Draw(context);
                        labelDrawing.Clear();
                    }
                    if (previousStyle == null || previousStyle != style)
                    {
                        labelDrawing.Transform = new AffineTransform(m11: 1, m22: 1,
                            m31: transform.M31 + x / pixelScaling, m32: transform.M32 + y / pixelScaling);
                        labelDrawing.Color = style.Foreground;
                        labelDrawing.FontSubfamily = style.FontSubfamily;
                    }
                    if (style.Foreground.Alpha > 0)
                    {
                        labelDrawing.AppendCodePoint(codePoint);
                    }
                    x += columnWidth;
                    previousStyle = style;
                }
                labelDrawing.Draw(context);
                labelDrawing.Clear();
                x = 0d;
                y += lineHeight;
            }
        }
    }
}
