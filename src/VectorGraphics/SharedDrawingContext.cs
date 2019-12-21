using NStuff.Geometry;
using NStuff.GraphicsBackend;
using NStuff.Tessellation;
using NStuff.Typography.Font;
using NStuff.Typography.Typesetting;
using System;
using System.Collections.Generic;

namespace NStuff.VectorGraphics
{
    /// <summary>
    /// Provides services that can be shared between <see cref="DrawingContext"/> instances.
    /// </summary>
    public sealed class SharedDrawingContext : IDisposable
    {
        private DrawingBackendBase? backend;
        private readonly GlyphAtlas glyphAtlas;
        private readonly List<ImageHandle> glyphImages = new List<ImageHandle>();
        private readonly List<CommandBufferHandle> bindGlyphImageCommandBuffers = new List<CommandBufferHandle>();
        private byte[] buffer = new byte[1024];

        internal DrawingBackendBase Backend => backend ?? throw new ObjectDisposedException(GetType().FullName);
        internal BezierApproximator BezierApproximator { get; } = new BezierApproximator();
        internal PolylineStroker PolylineStroker { get; }
        internal Tessellator2D<int, int> Tessellator { get; }
        internal GlyphLayout GlyphLayout { get; }

        internal CommandBufferHandle[] CommandBuffers { get; } = new CommandBufferHandle[16];
        internal RgbaColor[] Colors { get; } = new RgbaColor[16];
        internal VertexRange[] VertexRanges { get; } = new VertexRange[16];
        internal AffineTransform[] Transforms { get; } = new AffineTransform[16];
        internal PointCoordinates[] Vertices { get; } = new PointCoordinates[1024];
        internal PointAndImageCoordinates[] TexturedVertices { get; } = new PointAndImageCoordinates[1024];

        internal int ImageDimension => glyphAtlas.ImageDimension;

        /// <summary>
        /// Gets a value indicating whether the context's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public bool Disposed => backend == null || backend.Disposed;

        /// <summary>
        /// Gets the <c>FontMetrics</c> instance used to draw text.
        /// </summary>
        /// <value>The <c>FontMetrics</c> instance used to draw text.</value>
        public FontMetrics FontMetrics {
            get {
                CheckIfAlive();
                return glyphAtlas.FontMetrics;
            }
        }

        /// <summary>
        /// Gets or sets the scaling to apply to coordinates to get the number of pixels.
        /// </summary>
        public double PixelScaling {
            get {
                CheckIfAlive();
                return Backend.PixelScaling;
            }
            set {
                CheckIfAlive();
                Backend.PixelScaling = value;
                glyphAtlas.FontMetrics.PixelScaling = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <c>SharedDrawingContext</c> class using the provided drawing
        /// <paramref name="backend"/> and <paramref name="openTypeCollection"/>.
        /// </summary>
        /// <param name="backend">The drawing backend to use for rendering.</param>
        /// <param name="openTypeCollection">A collecion of fonts.</param>
        public SharedDrawingContext(DrawingBackendBase backend, OpenTypeCollection openTypeCollection)
        {
            this.backend = backend;
            glyphAtlas = new GlyphAtlas(new FontMetrics(openTypeCollection), Math.Min(backend.GetMaxTextureDimension(), 2048));
            Tessellator = new Tessellator2D<int, int>(new TessellateHandler())
            {
                OutputKind = OutputKind.TriangleEnumerator
            };
            PolylineStroker = new PolylineStroker(new PolylineStrokeHandler(Tessellator));
            GlyphLayout = new GlyphLayout(glyphAtlas.FontMetrics);
        }

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector
        /// reclaims the <see cref="SharedDrawingContext"/>.
        /// </summary>
        ~SharedDrawingContext() => FreeResources();

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
                if (!backend.Disposed)
                {
                    foreach (var commandBuffer in bindGlyphImageCommandBuffers)
                    {
                        backend.DestroyCommandBuffer(commandBuffer);
                    }
                }
                backend = null;
            }
        }

        internal Glyph GetGlyph(string fontFamily, FontSubfamily fontSubfamily, double fontPoints, int codePoint)
        {
            var glyph = glyphAtlas.GetGlyph(fontFamily, fontSubfamily, fontPoints, codePoint, out var newGlyph);
            if (newGlyph && glyph.HasImage)
            {
                var dimension = glyphAtlas.ImageDimension;
                for (int i = glyphImages.Count; i <= glyph.Index; i++)
                {
                    var imageHandle = backend!.CreateImage(dimension, dimension, ImageFormat.GreyscaleAlpha, ImageComponentType.UnsignedByte);
                    glyphImages.Add(imageHandle);
                    var commandBuffer = backend.CreateCommandBuffer();
                    backend.BeginRecordCommands(commandBuffer);
                    backend.AddBindImageCommand(commandBuffer, imageHandle);
                    backend.EndRecordCommands(commandBuffer);
                    bindGlyphImageCommandBuffers.Add(commandBuffer);
                }
                var width = glyph.Width;
                var height = glyph.Height;
                if (buffer.Length < width * height)
                {
                    buffer = new byte[width * height * 2];
                }
                var data = glyphAtlas.Images[glyph.Index];
                for (int i = 0; i < height; i++)
                {
                    Array.Copy(data, glyph.X + (glyph.Y + i) * dimension, buffer, i * width, width);
                }
                backend!.UpdateImage(glyphImages[glyph.Index], buffer, glyph.X, glyph.Y, glyph.Width, glyph.Height);
            }
            return glyph;
        }

        internal CommandBufferHandle GetBindGlyphImageCommandBuffer(int index) => bindGlyphImageCommandBuffers[index];

        private void CheckIfAlive()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
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
