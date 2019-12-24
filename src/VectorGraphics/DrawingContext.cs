using NStuff.Geometry;
using NStuff.GraphicsBackend;
using NStuff.RasterGraphics;
using NStuff.Tessellation;
using NStuff.Typography.Font;
using NStuff.Typography.Typesetting;
using System;

namespace NStuff.VectorGraphics
{
    /// <summary>
    /// Provides services to render vector graphics on an implementation of <see cref="DrawingBackendBase"/>.
    /// </summary>
    public sealed class DrawingContext : IDisposable
    {
        private SharedDrawingContext? sharedContext;
        private RgbaColor clearColor = new RgbaColor(255, 255, 255, 255);
        private CommandBufferHandle clearCommandBuffer;

        private DrawingBackendBase Backend => sharedContext!.Backend;
        private RgbaColor[] Colors => sharedContext!.Colors;
        private VertexRange[] VertexRanges => sharedContext!.VertexRanges;
        private AffineTransform[] Transforms => sharedContext!.Transforms;
        private PointCoordinates[] Vertices => sharedContext!.Vertices;
        private PointAndImageCoordinates[] TexturedVertices => sharedContext!.TexturedVertices;
        private CommandBufferHandle[] CommandBuffers => sharedContext!.CommandBuffers;

        private readonly UniformBufferHandle singleColorBuffer;
        private readonly UniformBufferHandle singleTransformBuffer;
        private readonly VertexRangeBufferHandle singleVertexRangeBuffer;
        private readonly VertexBufferHandle vertexBuffer;
        private readonly VertexBufferHandle texturedVertexBuffer;

        private readonly CommandBufferHandle setupPlainColorCommandBuffer;
        private readonly CommandBufferHandle setupGreyscaleImageCommandBuffer;
        private readonly CommandBufferHandle setupTrueColorImageCommandBuffer;
        private readonly CommandBufferHandle drawIndirectCommandBuffer;

        private int vertexCount;
        private int texturedVertexCount;
        private int commandCount;
        private RgbaColor? currentColor;
        private AffineTransform? currentTransform;
        private CommandBufferHandle? shaderCommandBuffer;
        private int? currentImageIndex;


        internal double PixelScaling => sharedContext!.PixelScaling;
        internal BezierApproximator BezierApproximator => sharedContext!.BezierApproximator;
        internal Tessellator2D<int, int> Tessellator => sharedContext!.Tessellator;
        internal PolylineStroker PolylineStroker => sharedContext!.PolylineStroker;
        internal GlyphLayout GlyphLayout => sharedContext!.GlyphLayout;

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before drawing.
        /// </summary>
        /// <value>A RGBA color used to clear the framebuffer.</value>
        public RgbaColor ClearColor {
            get {
                CheckIfAlive();
                return clearColor;
            }
            set {
                CheckIfAlive();
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
        /// Gets a value indicating whether the context's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public bool Disposed => sharedContext == null || sharedContext.Disposed;

        /// <summary>
        /// Gets the shared context used to create this context.
        /// </summary>
        /// <value>The shared context used to create this context.</value>
        public SharedDrawingContext SharedContext {
            get {
                CheckIfAlive();
                return sharedContext!;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <c>DrawingContext</c> class using the supplied <paramref name="sharedContext"/>.
        /// </summary>
        /// <param name="sharedContext">The object used to share ressources between contexts.</param>
        public DrawingContext(SharedDrawingContext sharedContext)
        {
            this.sharedContext = sharedContext;
            CheckIfAlive();

            singleColorBuffer = Backend.CreateUniformBuffer(UniformType.RgbaColor, 1);
            singleTransformBuffer = Backend.CreateUniformBuffer(UniformType.AffineTransform, 1);
            singleVertexRangeBuffer = Backend.CreateVertexRangeBuffer(1);
            vertexBuffer = Backend.CreateVertexBuffer(VertexType.PointCoordinates, Vertices.Length);
            texturedVertexBuffer = Backend.CreateVertexBuffer(VertexType.PointAndImageCoordinates, TexturedVertices.Length);

            setupPlainColorCommandBuffer = Backend.CreateCommandBuffer();
            Backend.BeginRecordCommands(setupPlainColorCommandBuffer);
            Backend.AddUseShaderCommand(setupPlainColorCommandBuffer, ShaderKind.PlainColor);
            Backend.AddBindVertexBufferCommand(setupPlainColorCommandBuffer, vertexBuffer);
            Backend.EndRecordCommands(setupPlainColorCommandBuffer);

            setupGreyscaleImageCommandBuffer = Backend.CreateCommandBuffer();
            Backend.BeginRecordCommands(setupGreyscaleImageCommandBuffer);
            Backend.AddUseShaderCommand(setupGreyscaleImageCommandBuffer, ShaderKind.GreyscaleImage);
            Backend.AddBindVertexBufferCommand(setupGreyscaleImageCommandBuffer, texturedVertexBuffer);
            Backend.EndRecordCommands(setupGreyscaleImageCommandBuffer);

            setupTrueColorImageCommandBuffer = Backend.CreateCommandBuffer();
            Backend.BeginRecordCommands(setupTrueColorImageCommandBuffer);
            Backend.AddUseShaderCommand(setupTrueColorImageCommandBuffer, ShaderKind.TrueColorImage);
            Backend.AddBindVertexBufferCommand(setupTrueColorImageCommandBuffer, texturedVertexBuffer);
            Backend.EndRecordCommands(setupTrueColorImageCommandBuffer);

            drawIndirectCommandBuffer = Backend.CreateCommandBuffer();
            Backend.BeginRecordCommands(drawIndirectCommandBuffer);
            Backend.AddBindUniformCommand(drawIndirectCommandBuffer, Uniform.Transform, singleTransformBuffer, 0);
            Backend.AddBindUniformCommand(drawIndirectCommandBuffer, Uniform.Color, singleColorBuffer, 0);
            Backend.AddDrawIndirectCommand(drawIndirectCommandBuffer, DrawingPrimitive.Triangles, singleVertexRangeBuffer, 0);
            Backend.EndRecordCommands(drawIndirectCommandBuffer);
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
            if (sharedContext != null)
            {
                if (!sharedContext.Disposed)
                {
                    if (clearCommandBuffer != default)
                    {
                        Backend.DestroyCommandBuffer(clearCommandBuffer);
                    }
                    Backend.DestroyCommandBuffer(drawIndirectCommandBuffer);
                    Backend.DestroyCommandBuffer(setupGreyscaleImageCommandBuffer);
                    Backend.DestroyCommandBuffer(setupPlainColorCommandBuffer);
                    Backend.DestroyVertexBuffer(texturedVertexBuffer);
                    Backend.DestroyVertexBuffer(vertexBuffer);
                    Backend.DestroyVertexRangeBuffer(singleVertexRangeBuffer);
                    Backend.DestroyUniformBuffer(singleTransformBuffer);
                    Backend.DestroyUniformBuffer(singleColorBuffer);
                }
                sharedContext = null;
            }
        }

        /// <summary>
        /// Starts drawing a new image.
        /// </summary>
        public void StartDrawing()
        {
            CheckIfAlive();

            if (clearCommandBuffer == default)
            {
                clearCommandBuffer = Backend.CreateCommandBuffer();
                Backend.BeginRecordCommands(clearCommandBuffer);
                Backend.AddClearCommand(clearCommandBuffer, ClearColor);
                Backend.EndRecordCommands(clearCommandBuffer);
            }

            Backend.BeginRenderFrame();
            CommandBuffers[0] = clearCommandBuffer;

            vertexCount = 0;
            texturedVertexCount = 0;
            commandCount = 1;
            currentColor = null;
            currentTransform = null;
            shaderCommandBuffer = null;
            currentImageIndex = null;
        }

        /// <summary>
        /// Finishes drawing the current image.
        /// </summary>
        public void FinishDrawing()
        {
            CheckIfAlive();

            SubmitDrawingCommand();
            if (commandCount > 0)
            {
                Backend.SubmitCommands(CommandBuffers, 0, commandCount);
            }
            Backend.EndRenderFrame();
        }

        internal void SetColor(RgbaColor color)
        {
            if (!currentColor.HasValue || currentColor.Value != color)
            {
                SubmitDrawingCommand();
                currentColor = color;
                Colors[0] = color;
                Backend.UpdateUniformBuffer(singleColorBuffer, Colors, 0, 1);
            }
        }

        internal void SetTransform(AffineTransform transform)
        {
            if (!currentTransform.HasValue || currentTransform.Value != transform)
            {
                SubmitDrawingCommand();
                currentTransform = transform;
                Transforms[0] = transform;
                Backend.UpdateUniformBuffer(singleTransformBuffer, Transforms, 0, 1);
            }
        }

        internal void SetupPlainColorRendering()
        {
            if (!shaderCommandBuffer.HasValue || shaderCommandBuffer.Value != setupPlainColorCommandBuffer)
            {
                SubmitDrawingCommand();
                shaderCommandBuffer = setupPlainColorCommandBuffer;
                CommandBuffers[commandCount++] = setupPlainColorCommandBuffer;
            }
        }

        internal void SetupGreyScaleImageRendering()
        {
            if (!shaderCommandBuffer.HasValue || shaderCommandBuffer.Value != setupGreyscaleImageCommandBuffer)
            {
                SubmitDrawingCommand();
                shaderCommandBuffer = setupGreyscaleImageCommandBuffer;
                CommandBuffers[commandCount++] = setupGreyscaleImageCommandBuffer;
            }
        }

        internal void SetupTrueColorImageRendering()
        {
            if (!shaderCommandBuffer.HasValue || shaderCommandBuffer.Value != setupTrueColorImageCommandBuffer)
            {
                SubmitDrawingCommand();
                shaderCommandBuffer = setupTrueColorImageCommandBuffer;
                CommandBuffers[commandCount++] = setupTrueColorImageCommandBuffer;
            }
        }

        internal void BindGlyphImage(int imageIndex)
        {
            if (!currentImageIndex.HasValue || currentImageIndex.Value != imageIndex)
            {
                SubmitDrawingCommand();
                currentImageIndex = imageIndex;
                CommandBuffers[commandCount++] = sharedContext!.GetBindGlyphImageCommandBuffer(imageIndex);
            }
        }

        internal void DrawImage(RasterImage image)
        {
            var texturedVertices = TexturedVertices;
            if (texturedVertexCount > 0)
            {
                SubmitDrawingCommand();
            }
            var handle = Backend.CreateImage(image.Size.width, image.Size.height, ImageFormat.TrueColorAlpha, ImageComponentType.UnsignedByte);
            Backend.UpdateImage(handle, image.Data, 0, 0, image.Size.width, image.Size.height);

            var commandBuffer = Backend.CreateCommandBuffer();
            Backend.BeginRecordCommands(commandBuffer);
            Backend.AddBindImageCommand(commandBuffer, handle);
            Backend.EndRecordCommands(commandBuffer);
            CommandBuffers[commandCount++] = commandBuffer;

            var left = 0d;
            var right = image.Size.width;
            var top = 0d;
            var bottom = image.Size.height;

            var textureLeft = 0d;
            var textureRight = 1d;
            var textureTop = 0d;
            var textureBottom = 1d;

            texturedVertices[0] = new PointAndImageCoordinates(left, bottom, textureLeft, textureBottom);
            texturedVertices[1] = new PointAndImageCoordinates(right, top, textureRight, textureTop);
            texturedVertices[2] = new PointAndImageCoordinates(left, top, textureLeft, textureTop);
            texturedVertices[3] = new PointAndImageCoordinates(left, bottom, textureLeft, textureBottom);
            texturedVertices[4] = new PointAndImageCoordinates(right, bottom, textureRight, textureBottom);
            texturedVertices[5] = new PointAndImageCoordinates(right, top, textureRight, textureTop);
            texturedVertexCount = 6;

            SubmitDrawingCommand();

            Backend.DestroyCommandBuffer(commandBuffer);
            Backend.DestroyImage(handle);
        }

        internal void AppendTriangleVertices((double x, double y) p1, (double x, double y) p2, (double x, double y) p3)
        {
            var vertices = Vertices;
            if (vertexCount + 3 > vertices.Length)
            {
                SubmitDrawingCommand();
            }

            var i = vertexCount;
            vertices[i++] = new PointCoordinates(p1.x, p1.y);
            vertices[i++] = new PointCoordinates(p2.x, p2.y);
            vertices[i++] = new PointCoordinates(p3.x, p3.y);
            vertexCount += 3;
        }

        internal void AppendGlyphVertices(Glyph glyph, double x, double y)
        {
            var texturedVertices = TexturedVertices;
            if (texturedVertexCount + 6 > texturedVertices.Length)
            {
                SubmitDrawingCommand();
            }

            var imageDimension = sharedContext!.ImageDimension;
            var pixelScaling = sharedContext!.PixelScaling;

            var left = Math.Round(x + glyph.Left) / pixelScaling;
            var top = Math.Round(y + glyph.Top) / pixelScaling;
            var right = Math.Round(x + glyph.Left + glyph.Width) / pixelScaling;
            var bottom = Math.Round(y + glyph.Top + glyph.Height) / pixelScaling;

            var textureLeft = (double)glyph.X / imageDimension;
            var textureTop = (double)glyph.Y / imageDimension;
            var textureRight = (glyph.X + glyph.Width) / (double)imageDimension;
            var textureBottom = (glyph.Y + glyph.Height) / (double)imageDimension;

            var i = texturedVertexCount;
            texturedVertices[i++] = new PointAndImageCoordinates(left, bottom, textureLeft, textureBottom);
            texturedVertices[i++] = new PointAndImageCoordinates(right, top, textureRight, textureTop);
            texturedVertices[i++] = new PointAndImageCoordinates(left, top, textureLeft, textureTop);
            texturedVertices[i++] = new PointAndImageCoordinates(left, bottom, textureLeft, textureBottom);
            texturedVertices[i++] = new PointAndImageCoordinates(right, bottom, textureRight, textureBottom);
            texturedVertices[i++] = new PointAndImageCoordinates(right, top, textureRight, textureTop);
            texturedVertexCount += 6;
        }

        internal Glyph GetGlyph(string fontFamily, FontSubfamily fontSubfamily, double fontPoints, int codePoint) =>
            sharedContext!.GetGlyph(fontFamily, fontSubfamily, fontPoints, codePoint);

        private void SubmitDrawingCommand()
        {
            var draw = false;
            if (vertexCount > 0)
            {
                if (!currentColor.HasValue || !currentTransform.HasValue || !shaderCommandBuffer.HasValue)
                {
                    throw new InvalidOperationException();
                }

                Backend.UpdateVertexBuffer(vertexBuffer, Vertices, 0, vertexCount);
                VertexRanges[0] = new VertexRange(0, vertexCount);

                vertexCount = 0;
                draw = true;
            }
            else if (texturedVertexCount > 0)
            {
                if (!currentColor.HasValue || !currentTransform.HasValue || !shaderCommandBuffer.HasValue) // || !currentImageIndex.HasValue)
                {
                    throw new InvalidOperationException();
                }

                Backend.UpdateVertexBuffer(texturedVertexBuffer, TexturedVertices, 0, texturedVertexCount);
                VertexRanges[0] = new VertexRange(0, texturedVertexCount);

                texturedVertexCount = 0;
                draw = true;
            }

            if (draw)
            {
                Backend.UpdateVertexRangeBuffer(singleVertexRangeBuffer, VertexRanges, 0, 1);
                CommandBuffers[commandCount++] = drawIndirectCommandBuffer;
                Backend.SubmitCommands(CommandBuffers, 0, commandCount);
                commandCount = 0;
            }
        }

        private void CheckIfAlive()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
