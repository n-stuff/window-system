using NStuff.GraphicsBackend;
using NStuff.Typography.Font;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    public class MonospaceTextStyle2
    {
        public FontSubfamily FontSubfamily { get; }

        public RgbaColor Foreground { get; }

        public RgbaColor Background { get; }

        internal MonospaceTextStyle2(FontSubfamily fontSubfamily, RgbaColor foreground, RgbaColor background)
        {
            FontSubfamily = fontSubfamily;
            Foreground = foreground;
            Background = background;
        }
    }
}
