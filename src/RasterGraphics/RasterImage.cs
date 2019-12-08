using System;
using System.Collections.Generic;
using System.Text;

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
        public RasterImageFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the type of each pixel's component.
        /// </summary>
        public RasterImageComponentType ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the size of the image in pixels.
        /// </summary>
        public (int width, int height) Size { get; set; }

        /// <summary>
        /// Gets or sets the bytes composing the pixels of this image.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
