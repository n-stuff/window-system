using NStuff.GraphicsBackend;
using NStuff.RasterGraphics;

namespace NStuff.VectorGraphics
{
    /// <summary>
    /// Represents an raster image drawing.
    /// </summary>
    public class ImageDrawing
    {
        /// <summary>
        /// Gets or sets the transform to apply to all coordinates of the drawing.
        /// </summary>
        /// <value>An affine transform. The initial value is the identity matrix.</value>
        public AffineTransform Transform { get; set; } = new AffineTransform(m11: 1, m22: 1);

        /// <summary>
        /// Gets or sets the raster image to render.
        /// </summary>
        /// <value>An object representing a raster image.</value>
        public RasterImage? Image { get; set; }

        /// <summary>
        /// Draws this image.
        /// </summary>
        /// <param name="context">The context used to draw this image.</param>
        public void Draw(DrawingContext context)
        {
            if (Image == null)
            {
                return;
            }
            context.SetColor(new RgbaColor(255, 255, 255, 255));
            context.SetTransform(Transform);
            context.SetupTrueColorImageRendering();
            context.DrawImage(Image);
        }
    }
}
