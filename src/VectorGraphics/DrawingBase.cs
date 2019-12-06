namespace NStuff.VectorGraphics
{
    /// <summary>
    /// Provides a base class for objects that can be drawn using a <see cref="DrawingContext"/>.
    /// </summary>
    public abstract class DrawingBase
    {
        internal abstract void Draw(DrawingContext context);
    }
}
