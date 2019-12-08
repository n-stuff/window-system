using System;

namespace NStuff.RasterGraphics
{
    /// <summary>
    /// Represents a rectangular grid of pixels.
    /// </summary>
    public class RasterImage
    {
        /// <summary>
        /// Gets or sets the format of this image.
        /// </summary>
        /// <value>One of the values that specifies the format of the image.</value>
        public RasterImageFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the type of each pixel's component.
        /// </summary>
        /// <value>One of the values that specifies the type of component.</value>
        public RasterImageComponentType ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the size of the image in pixels.
        /// </summary>
        /// <value>A size in pixels.</value>
        public (int width, int height) Size { get; set; }

        /// <summary>
        /// Gets or sets the bytes composing the pixels of this image.
        /// </summary>
        /// <value>The raw binary representation of the image.</value>
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
