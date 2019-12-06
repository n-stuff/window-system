using NStuff.GraphicsBackend;
using System;

namespace NStuff.VectorGraphics
{
    /// <summary>
    /// Provides shared resources and services to render vector graphics.
    /// </summary>
    public sealed class DrawingContext : IDisposable
    {
        private DrawingBackendBase? backend;
        private RgbaColor clearColor = new RgbaColor(255, 255, 255, 255);
        private CommandBufferHandle clearCommandBuffer;

        internal DrawingBackendBase Backend => backend ?? throw new ObjectDisposedException(GetType().FullName);
        internal CommandBufferHandle[] CommandBuffers { get; } = new CommandBufferHandle[16];

        /// <summary>
        /// The color used to clear the framebuffer before drawing.
        /// </summary>
        public RgbaColor ClearColor {
            get => clearColor;
            set {
                if (clearColor != value)
                {
                    clearColor = value;
                    if (clearCommandBuffer != default)
                    {
                        Backend.DestroyCommandBuffer(clearCommandBuffer);
                        clearCommandBuffer = default;
                    }
                }
            }
        }

        /// <summary>
        /// A value indicating whether the backend's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public bool Disposed => backend == null;

        /// <summary>
        /// The scaling to apply to coordinates to get the number of pixels.
        /// </summary>
        public double PixelScaling {
            get => Backend.PixelScaling;
            set => Backend.PixelScaling = value;
        }

        /// <summary>
        /// Initializes a new instance of the <c>DrawingContext</c> class using the provided drawing <paramref name="backend"/>.
        /// </summary>
        /// <param name="backend">The drawing backend to use for rendering.</param>
        public DrawingContext(DrawingBackendBase backend)
        {
            this.backend = backend;
        }

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector
        /// reclaims the <see cref="DrawingContext"/>.
        /// </summary>
        ~DrawingContext() => FreeResources();

        /// <summary>
        /// Releases the resources associated with this object. After calling this method, calling the other methods of this object
        /// is throwing an <c>ObjectDisposedException</c>.
        /// </summary>
        public void Dispose()
        {
            FreeResources();
            GC.SuppressFinalize(this);
        }

        private void FreeResources()
        {
            if (backend != null)
            {
                if (clearCommandBuffer != default)
                {
                    backend.DestroyCommandBuffer(clearCommandBuffer);
                }
                backend = null;
            }
        }

        /// <summary>
        /// Starts drawing a new image.
        /// </summary>
        public void StartDrawing()
        {
            if (clearCommandBuffer == default)
            {
                clearCommandBuffer = Backend.CreateCommandBuffer();
                Backend.BeginRecordCommands(clearCommandBuffer);
                Backend.AddClearCommand(clearCommandBuffer, ClearColor);
                Backend.EndRecordCommands(clearCommandBuffer);
            }

            Backend.BeginRenderFrame();
            CommandBuffers[0] = clearCommandBuffer;
            Backend.SubmitCommands(CommandBuffers, 0, 1);
        }

        /// <summary>
        /// Draws the supplied object.
        /// </summary>
        /// <param name="drawing">An object to draw.</param>
        public void Draw(DrawingBase drawing) => drawing.Draw(this);

        /// <summary>
        /// Finishes the current image.
        /// </summary>
        public void FinishDrawing()
        {
            Backend.EndRenderFrame();
        }
    }
}
