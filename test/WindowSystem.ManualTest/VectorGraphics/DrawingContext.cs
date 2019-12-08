using NStuff.Geometry;
using NStuff.GraphicsBackend;
using NStuff.Tessellation;
using System;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    /// <summary>
    /// Provides shared resources and services to render vector graphics.
    /// </summary>
    public sealed class DrawingContext : IDisposable
    {
        private DrawingBackendBase? backend;
        private RgbaColor clearColor = new RgbaColor(255, 255, 255, 255);
        private CommandBufferHandle clearCommandBuffer;

        internal BezierApproximator BezierApproximator { get; } = new BezierApproximator();
        internal PolylineStroker PolylineStroker { get; }
        internal Tessellator2D<int, int> Tessellator { get; }
        internal DrawingBackendBase Backend => backend ?? throw new ObjectDisposedException(GetType().FullName);

        internal CommandBufferHandle[] CommandBuffers { get; } = new CommandBufferHandle[16];
        internal RgbaColor[] Colors { get; } = new RgbaColor[16];
        internal VertexRange[] VertexRanges { get; } = new VertexRange[16];
        internal AffineTransform[] Transforms { get; } = new AffineTransform[16];
        internal PointCoordinates[] Vertices { get; } = new PointCoordinates[1024];

        internal UniformBufferHandle SingleColorBuffer { get; }
        internal UniformBufferHandle SingleTransformBuffer { get; }
        internal VertexRangeBufferHandle SingleVertexRangeBuffer { get; }
        internal VertexBufferHandle VertexBuffer { get; }

        internal CommandBufferHandle SetupPlainColorCommandBuffer { get; private set; }
        internal CommandBufferHandle DrawIndirectCommandBuffer { get; private set; }

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before drawing.
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
        /// Gets a value indicating whether the backend's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public bool Disposed => backend == null;

        /// <summary>
        /// Gets or sets the scaling to apply to coordinates to get the number of pixels.
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
            Tessellator = new Tessellator2D<int, int>(new TessellateHandler())
            {
                OutputKind = OutputKind.TriangleEnumerator
            };
            PolylineStroker = new PolylineStroker(new PolylineStrokeHandler(Tessellator));

            SingleColorBuffer = backend.CreateUniformBuffer(UniformType.RgbaColor, 1);
            SingleTransformBuffer = backend.CreateUniformBuffer(UniformType.AffineTransform, 1);
            SingleVertexRangeBuffer = backend.CreateVertexRangeBuffer(1);
            VertexBuffer = backend.CreateVertexBuffer(VertexType.PointCoordinates, Vertices.Length);

            SetupPlainColorCommandBuffer = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(SetupPlainColorCommandBuffer);
            backend.AddUseShaderCommand(SetupPlainColorCommandBuffer, ShaderKind.PlainColor);
            backend.AddBindVertexBufferCommand(SetupPlainColorCommandBuffer, VertexBuffer);
            backend.EndRecordCommands(SetupPlainColorCommandBuffer);

            DrawIndirectCommandBuffer = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(DrawIndirectCommandBuffer);
            // TODO: updating uniform buffer should update the bound uniform. The binding should be done once when setting the shader.
            backend.AddBindUniformCommand(DrawIndirectCommandBuffer, Uniform.Transform, SingleTransformBuffer, 0);
            backend.AddBindUniformCommand(DrawIndirectCommandBuffer, Uniform.Color, SingleColorBuffer, 0);
            backend.AddDrawIndirectCommand(DrawIndirectCommandBuffer, DrawingPrimitive.Triangles, SingleVertexRangeBuffer, 0);
            backend.EndRecordCommands(DrawIndirectCommandBuffer);
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
                    backend.DestroyCommandBuffer(DrawIndirectCommandBuffer);
                    backend.DestroyCommandBuffer(SetupPlainColorCommandBuffer);
                    backend.DestroyVertexBuffer(VertexBuffer);
                    backend.DestroyCommandBuffer(clearCommandBuffer);
                    backend.DestroyVertexRangeBuffer(SingleVertexRangeBuffer);
                    backend.DestroyUniformBuffer(SingleTransformBuffer);
                    backend.DestroyUniformBuffer(SingleColorBuffer);
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

        internal static (double x, double y) TransformPoint((double x, double y) point, ref AffineTransform transform)
        {
            return
                ((point.x * transform.M11 + point.y * transform.M21 + transform.M31),
                 (point.x * transform.M12 + point.y * transform.M22 + transform.M32));
        }

        private class PolylineStrokeHandler : IPolylineStrokeHandler
        {
            private readonly Tessellator2D<int, int> tessellator;

            internal PolylineStrokeHandler(Tessellator2D<int, int> tessellator) => this.tessellator = tessellator;

            public void BeginPolygon() => tessellator.BeginPolygon(0);

            public void BeginContour() => tessellator.BeginContour();

            public void AddPoint(double x, double y) => tessellator.AddVertex(x, y, 0);

            public void EndContour() => tessellator.EndContour();

            public void EndPolygon() => tessellator.EndPolygon();
        }

        private class TessellateHandler : ITessellateHandler<int, int>
        {
            public void Begin(PrimitiveKind primitiveKind, int data)
            {
            }

            public void End(int data)
            {
            }

            public void FlagEdges(bool onPolygonBoundary)
            {
            }

            public void AddVertex(double x, double y, double z, int data)
            {
            }

            public int CombineEdges(double x, double y, double z,
                (int data, double weight) origin1, (int data, double weight) destination1,
                (int data, double weight) origin2, (int data, double weight) destination2, int polygonData)
            {
                return 0;
            }
        }
    }
}
