namespace NStuff.Typography.Font
{
    internal abstract class GlyphDecoder
    {
        internal PathCommand PathCommand { get; set; }
        internal double Scale { get; set; }
        internal double X { get; set; }
        internal double Y { get; set; }
        internal double Cx { get; set; }
        internal double Cy { get; set; }
        internal double Cx1 { get; set; }
        internal double Cy1 { get; set; }
        internal double XMin { get; set; }
        internal double XMax { get; set; }
        internal double YMin { get; set; }
        internal double YMax { get; set; }
        internal abstract bool Setup(OpenType openType, double pixelSize, uint glyphIndex);
        internal abstract bool Move();
    }
}
