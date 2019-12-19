using NStuff.Geometry;
using NStuff.GraphicsBackend;
using NStuff.Tessellation;
using NStuff.Typography.Font;
using NStuff.Typography.Typesetting;
using System;
using System.Collections.Generic;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    /// <summary>
    /// Provides shared resources and services to render vector graphics.
    /// </summary>
    public sealed class DrawingContext2 : IDisposable
    {
        private DrawingBackendBase? backend;
        private RgbaColor clearColor = new RgbaColor(255, 255, 255, 255);
        private CommandBufferHandle clearCommandBuffer;
        private byte[] buffer = new byte[1024];

        internal BezierApproximator BezierApproximator { get; } = new BezierApproximator();
        internal PolylineStroker PolylineStroker { get; }
        internal Tessellator2D<int, int> Tessellator { get; }
        internal GlyphAtlas GlyphAtlas { get; }
        internal DrawingBackendBase Backend => backend ?? throw new ObjectDisposedException(GetType().FullName);

        internal CommandBufferHandle[] CommandBuffers { get; } = new CommandBufferHandle[16];
        internal RgbaColor[] Colors { get; } = new RgbaColor[16];
        internal VertexRange[] VertexRanges { get; } = new VertexRange[16];
        internal AffineTransform[] Transforms { get; } = new AffineTransform[16];
        internal PointCoordinates[] Vertices { get; } = new PointCoordinates[1024];
        internal PointAndImageCoordinates[] TexturedVertices { get; } = new PointAndImageCoordinates[1024];

        internal UniformBufferHandle SingleColorBuffer { get; }
        internal UniformBufferHandle SingleTransformBuffer { get; }
        internal VertexRangeBufferHandle SingleVertexRangeBuffer { get; }
        internal VertexBufferHandle VertexBuffer { get; }
        internal VertexBufferHandle TexturedVertexBuffer { get; }

        internal CommandBufferHandle SetupPlainColorCommandBuffer { get; private set; }
        internal CommandBufferHandle SetupGreyscaleImageColorCommandBuffer { get; private set; }
        internal CommandBufferHandle DrawIndirectCommandBuffer { get; private set; }

        internal List<ImageHandle> GlyphImages { get; } = new List<ImageHandle>();
        internal List<CommandBufferHandle> BindGlyphImageCommandBuffers { get; } = new List<CommandBufferHandle>();

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
            set {
                Backend.PixelScaling = value;
                GlyphAtlas.FontMetrics.PixelScaling = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <c>DrawingContext</c> class using the provided drawing <paramref name="backend"/>.
        /// </summary>
        /// <param name="backend">The drawing backend to use for rendering.</param>
        /// <param name="openTypeCollection">A collecion of fonts.</param>
        public DrawingContext2(DrawingBackendBase backend, OpenTypeCollection openTypeCollection)
        {
            this.backend = backend;
            Tessellator = new Tessellator2D<int, int>(new TessellateHandler())
            {
                OutputKind = OutputKind.TriangleEnumerator
            };
            PolylineStroker = new PolylineStroker(new PolylineStrokeHandler(Tessellator));
            GlyphAtlas = new GlyphAtlas(new FontMetrics(openTypeCollection), Math.Min(backend.GetMaxTextureDimension(), 2048));
            GlyphAtlas.FontMetrics.PixelScaling = backend.PixelScaling;

            SingleColorBuffer = backend.CreateUniformBuffer(UniformType.RgbaColor, 1);
            SingleTransformBuffer = backend.CreateUniformBuffer(UniformType.AffineTransform, 1);
            SingleVertexRangeBuffer = backend.CreateVertexRangeBuffer(1);
            VertexBuffer = backend.CreateVertexBuffer(VertexType.PointCoordinates, Vertices.Length);
            TexturedVertexBuffer = backend.CreateVertexBuffer(VertexType.PointAndImageCoordinates, TexturedVertices.Length);

            SetupPlainColorCommandBuffer = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(SetupPlainColorCommandBuffer);
            backend.AddUseShaderCommand(SetupPlainColorCommandBuffer, ShaderKind.PlainColor);
            backend.AddBindVertexBufferCommand(SetupPlainColorCommandBuffer, VertexBuffer);
            backend.EndRecordCommands(SetupPlainColorCommandBuffer);

            SetupGreyscaleImageColorCommandBuffer = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(SetupGreyscaleImageColorCommandBuffer);
            backend.AddUseShaderCommand(SetupGreyscaleImageColorCommandBuffer, ShaderKind.GreyscaleImage);
            backend.AddBindVertexBufferCommand(SetupGreyscaleImageColorCommandBuffer, TexturedVertexBuffer);
            backend.EndRecordCommands(SetupGreyscaleImageColorCommandBuffer);

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
        /// reclaims the <see cref="DrawingContext2"/>.
        /// </summary>
        ~DrawingContext2() => FreeResources();

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
                if (clearCommandBuffer != default && !backend.Disposed)
                {
                    foreach (var commandBuffer in BindGlyphImageCommandBuffers)
                    {
                        backend.DestroyCommandBuffer(commandBuffer);
                    }
                    backend.DestroyCommandBuffer(DrawIndirectCommandBuffer);
                    backend.DestroyCommandBuffer(SetupGreyscaleImageColorCommandBuffer);
                    backend.DestroyCommandBuffer(SetupPlainColorCommandBuffer);
                    backend.DestroyVertexBuffer(TexturedVertexBuffer);
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
        public void Draw(DrawingBase2 drawing) => drawing.Draw(this);

        /// <summary>
        /// Finishes the current image.
        /// </summary>
        public void FinishDrawing()
        {
            Backend.EndRenderFrame();
        }

        internal Glyph GetGlyph(string fontFamily, FontSubfamily fontSubfamily, double fontPoints, int codePoint)
        {
            var glyph = GlyphAtlas.GetGlyph(fontFamily, fontSubfamily, fontPoints, codePoint, out var newGlyph);
            if (newGlyph && glyph.HasImage)
            {
                var dimension = GlyphAtlas.ImageDimension;
                for (int i = GlyphImages.Count; i <= glyph.Index; i++)
                {
                    var imageHandle = Backend.CreateImage(dimension, dimension, ImageFormat.GreyscaleAlpha, ImageComponentType.UnsignedByte);
                    GlyphImages.Add(imageHandle);
                    var commandBuffer = Backend.CreateCommandBuffer();
                    Backend.BeginRecordCommands(commandBuffer);
                    Backend.AddBindImageCommand(commandBuffer, imageHandle);
                    Backend.EndRecordCommands(commandBuffer);
                    BindGlyphImageCommandBuffers.Add(commandBuffer);
                }
                var width = glyph.Width;
                var height = glyph.Height;
                if (buffer.Length < width * height)
                {
                    buffer = new byte[width * height * 2];
                }
                var data = GlyphAtlas.Images[glyph.Index];
                for (int i = 0; i < height; i++)
                {
                    Array.Copy(data, glyph.X + (glyph.Y + i) * dimension, buffer, i * width, width);
                }
                Backend.UpdateImage(GlyphImages[glyph.Index], buffer, glyph.X, glyph.Y, glyph.Width, glyph.Height);
            }
            return glyph;
        }

        internal void ComputeGlyphVertices(Glyph glyph, double x, double y, ref int vertexCount)
        {
            if (glyph.HasImage)
            {
                var imageDimension = GlyphAtlas.ImageDimension;

                var left = Math.Round(x + glyph.Left) / PixelScaling;
                var top = Math.Round(y + glyph.Top) / PixelScaling;
                var right = Math.Round(x + glyph.Left + glyph.Width) / PixelScaling;
                var bottom = Math.Round(y + glyph.Top + glyph.Height) / PixelScaling;

                var textureLeft = (double)glyph.X / imageDimension;
                var textureTop = (double)glyph.Y / imageDimension;
                var textureRight = (glyph.X + glyph.Width) / (double)imageDimension;
                var textureBottom = (glyph.Y + glyph.Height) / (double)imageDimension;

                var i = vertexCount;
                TexturedVertices[i++] = new PointAndImageCoordinates(left, bottom, textureLeft, textureBottom);
                TexturedVertices[i++] = new PointAndImageCoordinates(right, top, textureRight, textureTop);
                TexturedVertices[i++] = new PointAndImageCoordinates(left, top, textureLeft, textureTop);
                TexturedVertices[i++] = new PointAndImageCoordinates(left, bottom, textureLeft, textureBottom);
                TexturedVertices[i++] = new PointAndImageCoordinates(right, bottom, textureRight, textureBottom);
                TexturedVertices[i++] = new PointAndImageCoordinates(right, top, textureRight, textureTop);
                vertexCount += 6;
            }
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
