using NStuff.WindowSystem;

namespace NStuff.OpenGL.Context
{
    /// <summary>
    /// Used to specify desired rendering context properties during the creation of a window.
    /// </summary>
    public sealed class RenderingSettings
    {
        /// <summary>
        /// Gets or sets the number of bits used to represent a RGB color.
        /// </summary>
        public int ColorBits { get; set; } = 24;

        /// <summary>
        /// Gets or sets the number of bits used to represent an alpha component.
        /// </summary>
        public int AlphaBits { get; set; } = 8;

        /// <summary>
        /// Gets or sets the number of bits used to represent the depth of a drawing surface.
        /// </summary>
        public int DepthBits { get; set; } = 16;

        /// <summary>
        /// Gets or sets the number of bits used to represent the stencil of a drawing surface.
        /// </summary>
        public int StencilBits { get; set; }

        /// <summary>
        /// Gets or sets the number of bits used to represent the accumulation buffer of a drawing surface.
        /// </summary>
        public int AccumBits { get; set; }

        /// <summary>
        /// Gets or sets the number of auxiliary buffers.
        /// </summary>
        public int AuxBuffers { get; set; }

        /// <summary>
        /// Gets or sets a value telling whether to use stereoscopic rendering.
        /// </summary>
        public bool Stereo { get; set; }

        /// <summary>
        /// Gets or sets the number of samples to use for antialiasing.
        /// </summary>
        public int Samples { get; set; }

        /// <summary>
        /// Gets or sets a context used to share rendering objects.
        /// </summary>
        public Window? ShareContext { get; set; }
    }
}
