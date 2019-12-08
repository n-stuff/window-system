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
        /// <value>A number of bits. Default is <c>24</c>.</value>
        public int ColorBits { get; set; } = 24;

        /// <summary>
        /// Gets or sets the number of bits used to represent an alpha component.
        /// </summary>
        /// <value>A number of bits. Default is <c>8</c>.</value>
        public int AlphaBits { get; set; } = 8;

        /// <summary>
        /// Gets or sets the number of bits used to represent the depth of a drawing surface.
        /// </summary>
        /// <value>A number of bits. Default is <c>16</c>.</value>
        public int DepthBits { get; set; } = 16;

        /// <summary>
        /// Gets or sets the number of bits used to represent the stencil of a drawing surface.
        /// </summary>
        /// <value>A number of bits. Default is <c>0</c>.</value>
        public int StencilBits { get; set; }

        /// <summary>
        /// Gets or sets the number of bits used to represent the accumulation buffer of a drawing surface.
        /// </summary>
        /// <value>A number of bits. Default is <c>0</c>.</value>
        public int AccumBits { get; set; }

        /// <summary>
        /// Gets or sets the number of auxiliary buffers.
        /// </summary>
        /// <value>A number of buffers. Default is <c>0</c>.</value>
        public int AuxBuffers { get; set; }

        /// <summary>
        /// Gets or sets a value telling whether to use stereoscopic rendering.
        /// </summary>
        /// <value><c>true</c> if stereoscopic rendering must be enabled. Default is <c>false</c>.</value>
        public bool Stereo { get; set; }

        /// <summary>
        /// Gets or sets the number of samples to use for antialiasing.
        /// </summary>
        /// <value>A number of samples. Default is <c>0</c>.</value>
        public int Samples { get; set; }

        /// <summary>
        /// Gets or sets a context used to share rendering objects.
        /// </summary>
        /// <value>A <see cref="Window"/>, or <c>null</c>.</value>
        public Window? ShareContext { get; set; }
    }
}
