using NStuff.GraphicsBackend;

namespace NStuff.VectorGraphics
{
    /// <summary>
    /// Provides a base class for objects that can be drawn using a <see cref="DrawingContext"/>.
    /// </summary>
    public abstract class DrawingBase
    {
        /// <summary>
        /// Gets or sets the transform to apply to all coordinates of the drawing.
        /// </summary>
        public AffineTransform Transform { get; set; } = new AffineTransform(m11: 1, m22: 1);

        internal abstract void Draw(DrawingContext context);
    }
}
